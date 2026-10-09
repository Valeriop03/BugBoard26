using System.Globalization;
using System.Net;
using System.Text;
using BugBoard26.Api.Contracts.Issues;
using BugBoard26.Api.Data;
using BugBoard26.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BugBoard26.Tests.Integration;

[Trait("Category", "Integration")]
public class IssueExportIntegrationTests : ApiIntegrationTest
{
    private const string Header = "Id,Title,Type,Priority,Status,CreatedByEmail,AssignedToEmail,CreatedAt";

    [Fact]
    public async Task Export_ReturnsUtf8AttachmentWithHeadersAndData_IncludingMissingAssigneeAndPriority()
    {
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var issue = await db.Issues.SingleAsync(issue => issue.Id == IssueId);
            issue.Title = "Caff\u00e8 API";
            issue.Type = IssueType.Documentation;
            issue.Priority = null;
            issue.Status = IssueStatus.InProgress;
            issue.AssignedToId = null;
            await db.SaveChangesAsync();
        }
        await AuthenticateAsync("READONLY");
        var before = await SnapshotAsync();
        var dateBefore = DateTime.UtcNow;

        using var response = await Client.GetAsync("/api/issues/export");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType!.MediaType);
        Assert.Equal("utf-8", response.Content.Headers.ContentType.CharSet);
        var disposition = response.Content.Headers.ContentDisposition;
        Assert.NotNull(disposition);
        Assert.Equal("attachment", disposition.DispositionType);
        var fileName = (disposition.FileNameStar ?? disposition.FileName)?.Trim('"');
        Assert.Contains(fileName, new[]
        {
            $"bugboard26-issues-{dateBefore:yyyyMMdd}.csv",
            $"bugboard26-issues-{DateTime.UtcNow:yyyyMMdd}.csv"
        });
        var csv = Encoding.UTF8.GetString(await response.Content.ReadAsByteArrayAsync());
        var rows = SplitUnescapedRows(csv);
        Assert.Equal(Header.Split(','), rows[0]);
        Assert.Equal(3, rows.Length);
        Assert.Equal(new[]
        {
            IssueId.ToString(CultureInfo.InvariantCulture), "Caff\u00e8 API", "Documentation", "", "InProgress",
            Email("USER"), "", "2026-01-01T00:00:00.0000000Z"
        }, Assert.Single(rows.Skip(1), row => row[0] == IssueId.ToString(CultureInfo.InvariantCulture)));
        Assert.Equal(new[]
        {
            OriginalIssueId.ToString(CultureInfo.InvariantCulture), "Issue originale", "Bug", "High", "Todo",
            Email("USER"), Email("USER"), "2026-01-01T00:00:00.0000000Z"
        }, Assert.Single(rows.Skip(1), row => row[0] == OriginalIssueId.ToString(CultureInfo.InvariantCulture)));
        Assert.Equal(before, await SnapshotAsync());
    }

    [Theory]
    [InlineData("Titolo, con virgola", "\"Titolo, con virgola\"")]
    [InlineData("Titolo \"citato\"", "\"Titolo \"\"citato\"\"\"")]
    [InlineData("Prima\nSeconda", "\"Prima\nSeconda\"")]
    [InlineData("Prima\rSeconda", "\"Prima\rSeconda\"")]
    [InlineData("Prima\r\nSeconda", "\"Prima\r\nSeconda\"")]
    [InlineData("Prima, \"citata\"\r\nSeconda", "\"Prima, \"\"citata\"\"\r\nSeconda\"")]
    public async Task Export_EscapesDelimitersQuotesAndLineBreaks(string title, string escapedTitle)
    {
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            (await db.Issues.SingleAsync(issue => issue.Id == IssueId)).Title = title;
            (await db.Issues.SingleAsync(issue => issue.Id == OriginalIssueId)).IsArchived = true;
            await db.SaveChangesAsync();
        }
        await AuthenticateAsync("USER");
        var before = await SnapshotAsync();

        using var response = await Client.GetAsync("/api/issues/export");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var expected = Header + Environment.NewLine +
            $"{IssueId},{escapedTitle},Bug,High,Todo,{Email("USER")},{Email("USER")},2026-01-01T00:00:00.0000000Z" + Environment.NewLine;
        Assert.Equal(expected, await response.Content.ReadAsStringAsync());
        Assert.Equal(before, await SnapshotAsync());
    }

    [Theory]
    [InlineData("all")]
    [InlineData("type")]
    [InlineData("status")]
    [InlineData("priority")]
    [InlineData("creator")]
    [InlineData("assignee")]
    [InlineData("title-keyword")]
    [InlineData("description-keyword")]
    [InlineData("combined")]
    [InlineData("no-matches")]
    [InlineData("date-asc")]
    [InlineData("date-desc")]
    [InlineData("priority-asc")]
    [InlineData("priority-desc")]
    [InlineData("status-asc")]
    [InlineData("status-desc")]
    [InlineData("title-asc")]
    [InlineData("title-desc")]
    public async Task Export_MatchesFilteredAndSortedList_AndExcludesArchivedIssues(string scenario)
    {
        var fixture = await PrepareFilterIssuesAsync();
        var allByDate = new[] { fixture.QuestionId, fixture.FeatureId, OriginalIssueId, IssueId };
        var (query, expectedIds) = scenario switch
        {
            "all" => ("", allByDate),
            "type" => ("?type=Feature", new[] { fixture.FeatureId }),
            "status" => ("?status=InProgress", new[] { OriginalIssueId }),
            "priority" => ("?priority=High", new[] { IssueId }),
            "creator" => ($"?createdById={fixture.AdminId}", new[] { fixture.FeatureId, OriginalIssueId }),
            "assignee" => ($"?assignedToId={fixture.AdminId}", new[] { OriginalIssueId }),
            "title-keyword" => ("?keyword=login", new[] { IssueId }),
            "description-keyword" => ("?keyword=ricerca", new[] { fixture.FeatureId }),
            "combined" => ($"?type=Bug&status=InProgress&priority=Low&createdById={fixture.AdminId}&assignedToId={fixture.AdminId}&keyword=api", new[] { OriginalIssueId }),
            "no-matches" => ("?keyword=nessun-risultato", Array.Empty<int>()),
            "date-asc" => ("?sortBy=Date&sortDirection=Asc", allByDate.Reverse().ToArray()),
            "date-desc" => ("?sortBy=Date&sortDirection=Desc", allByDate),
            "priority-asc" => ("?sortBy=Priority&sortDirection=Asc", new[] { OriginalIssueId, fixture.FeatureId, IssueId, fixture.QuestionId }),
            "priority-desc" => ("?sortBy=Priority&sortDirection=Desc", new[] { fixture.QuestionId, IssueId, fixture.FeatureId, OriginalIssueId }),
            "status-asc" => ("?sortBy=Status&sortDirection=Asc", new[] { IssueId, OriginalIssueId, fixture.FeatureId, fixture.QuestionId }),
            "status-desc" => ("?sortBy=Status&sortDirection=Desc", new[] { fixture.QuestionId, fixture.FeatureId, OriginalIssueId, IssueId }),
            "title-asc" => ("?sortBy=Title&sortDirection=Asc", new[] { fixture.FeatureId, IssueId, fixture.QuestionId, OriginalIssueId }),
            "title-desc" => ("?sortBy=Title&sortDirection=Desc", new[] { OriginalIssueId, fixture.QuestionId, IssueId, fixture.FeatureId }),
            _ => throw new ArgumentException("Scenario non previsto.", nameof(scenario))
        };
        await AuthenticateAsync("READONLY");
        var before = await SnapshotAsync();
        var issues = await ReadIssuesAsync("/api/issues" + query);
        Assert.Equal(expectedIds, issues.Select(issue => issue.Id));

        using var response = await Client.GetAsync("/api/issues/export" + query);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var rows = SplitUnescapedRows(await response.Content.ReadAsStringAsync());
        Assert.Equal(Header.Split(','), rows[0]);
        var data = rows.Skip(1).ToArray();
        Assert.Equal(expectedIds, data.Select(row => int.Parse(row[0], CultureInfo.InvariantCulture)));
        Assert.DoesNotContain(fixture.ArchivedId, data.Select(row => int.Parse(row[0], CultureInfo.InvariantCulture)));
        for (var index = 0; index < issues.Length; index++)
        {
            Assert.Equal(ExpectedRow(issues[index]), data[index]);
        }
        Assert.Equal(before, await SnapshotAsync());
    }

    private async Task<(int AdminId, int FeatureId, int QuestionId, int ArchivedId)> PrepareFilterIssuesAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var adminId = await db.Users.Where(user => user.Email == Email("ADMIN")).Select(user => user.Id).SingleAsync();
        var issue = await db.Issues.SingleAsync(issue => issue.Id == IssueId);
        issue.Title = "Beta LOGIN";
        var original = await db.Issues.SingleAsync(issue => issue.Id == OriginalIssueId);
        original.Title = "Zeta API";
        original.Status = IssueStatus.InProgress;
        original.Priority = IssuePriority.Low;
        original.CreatedById = adminId;
        original.AssignedToId = adminId;
        original.CreatedAt = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc);
        var feature = new Issue
        {
            Title = "Alfa nuova", Description = "RICERCA presente soltanto nella descrizione.",
            Type = IssueType.Feature, Priority = IssuePriority.Medium, Status = IssueStatus.Resolved,
            CreatedById = adminId, AssignedToId = null,
            CreatedAt = new DateTime(2026, 1, 3, 0, 0, 0, DateTimeKind.Utc),
            ResolvedAt = new DateTime(2026, 1, 3, 1, 0, 0, DateTimeKind.Utc)
        };
        var question = new Issue
        {
            Title = "Gamma quesito", Description = "Issue con priorita' e assegnatario assenti.",
            Type = IssueType.Question, Priority = null, Status = IssueStatus.Closed,
            CreatedById = UserId, AssignedToId = null,
            CreatedAt = new DateTime(2026, 1, 4, 0, 0, 0, DateTimeKind.Utc)
        };
        var archived = new Issue
        {
            Title = "Archiviata LOGIN API", Description = "RICERCA da non esportare.",
            Type = IssueType.Feature, Priority = IssuePriority.High, Status = IssueStatus.InProgress,
            CreatedById = adminId, AssignedToId = adminId, IsArchived = true,
            CreatedAt = new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc)
        };
        db.Issues.AddRange(feature, question, archived);
        await db.SaveChangesAsync();
        return (adminId, feature.Id, question.Id, archived.Id);
    }

    private static string[][] SplitUnescapedRows(string csv) =>
        csv.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Select(row => row.Split(',')).ToArray();

    private static string[] ExpectedRow(IssueResponse issue) => new[]
    {
        issue.Id.ToString(CultureInfo.InvariantCulture), issue.Title, issue.Type.ToString(),
        issue.Priority?.ToString() ?? "", issue.Status.ToString(), issue.CreatedByEmail,
        issue.AssignedToEmail ?? "", issue.CreatedAt.ToString("O", CultureInfo.InvariantCulture)
    };
}

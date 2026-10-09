using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BugBoard26.Api.Contracts.Issues;
using BugBoard26.Api.Data;
using BugBoard26.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BugBoard26.Tests.Integration;

[Trait("Category", "Integration")]
public class IssueSuggestionIntegrationTests : ApiIntegrationTest
{
    [Theory]
    [InlineData("USER", 3, 2)]
    [InlineData("ADMIN", 1, 1)]
    public async Task Suggestion_SelectsUserOrAdminWithLowestLoad(string expectedRole, int adminLoad, int expectedLoad)
    {
        var adminId = await PrepareCandidatesAsync(adminLoad);
        await AuthenticateAsync("READONLY");
        var before = await SnapshotAsync();

        using var response = await Client.GetAsync("/api/issues/suggest-assignee");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var suggestion = await response.Content.ReadFromJsonAsync<SuggestAssigneeResponse>();
        Assert.NotNull(suggestion);
        Assert.Equal(expectedRole == "USER" ? UserId : adminId, suggestion.UserId);
        Assert.Equal(Email(expectedRole), suggestion.Email);
        Assert.Equal(expectedLoad, suggestion.OpenAssignedIssues);
        Assert.Equal(before, await SnapshotAsync());
    }

    [Fact]
    public async Task Suggestion_CountsOnlyAssignedTodoAndInProgressIssues_ThatAreNotArchived()
    {
        await PrepareCandidatesAsync(3);
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var resolved = AssignedIssue(UserId, IssueStatus.Resolved);
            resolved.ResolvedAt = DateTime.UtcNow;
            var duplicate = AssignedIssue(UserId, IssueStatus.Duplicate);
            duplicate.DuplicateOfIssueId = OriginalIssueId;
            db.Issues.AddRange(
                resolved, AssignedIssue(UserId, IssueStatus.Closed), duplicate,
                AssignedIssue(UserId, IssueStatus.Todo, true),
                AssignedIssue(UserId, IssueStatus.InProgress, true),
                AssignedIssue(null, IssueStatus.Todo));
            await db.SaveChangesAsync();
        }
        await AuthenticateAsync("READONLY");
        var before = await SnapshotAsync();

        using var response = await Client.GetAsync("/api/issues/suggest-assignee");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var suggestion = await response.Content.ReadFromJsonAsync<SuggestAssigneeResponse>();
        Assert.NotNull(suggestion);
        Assert.Equal(UserId, suggestion.UserId);
        Assert.Equal(Email("USER"), suggestion.Email);
        Assert.Equal(2, suggestion.OpenAssignedIssues);
        Assert.Equal(before, await SnapshotAsync());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    public async Task Suggestion_BreaksEqualLoadTiesByLowestId_NotEmail(int load)
    {
        var adminId = await PrepareCandidatesAsync(load);
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var firstId = Math.Min(adminId, UserId);
            var lastId = Math.Max(adminId, UserId);
            (await db.Users.SingleAsync(user => user.Id == firstId)).Email = "z-candidate@integration.bugboard26.local";
            (await db.Users.SingleAsync(user => user.Id == lastId)).Email = "a-candidate@integration.bugboard26.local";
            if (load == 0)
            {
                foreach (var issue in await db.Issues.Where(issue => issue.AssignedToId == UserId).ToListAsync())
                {
                    issue.AssignedToId = null;
                }
            }
            await db.SaveChangesAsync();
        }
        await AuthenticateAsync("READONLY");
        var before = await SnapshotAsync();

        using var response = await Client.GetAsync("/api/issues/suggest-assignee");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var suggestion = await response.Content.ReadFromJsonAsync<SuggestAssigneeResponse>();
        Assert.NotNull(suggestion);
        Assert.Equal(Math.Min(adminId, UserId), suggestion.UserId);
        Assert.Equal("z-candidate@integration.bugboard26.local", suggestion.Email);
        Assert.Equal(load, suggestion.OpenAssignedIssues);
        Assert.Equal(before, await SnapshotAsync());
    }

    [Fact]
    public async Task Suggestion_WhenNoUserIsAssignable_ReturnsNotFoundAndExpectedMessage()
    {
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            foreach (var user in await db.Users.Where(user => user.Role != UserRole.Readonly).ToListAsync())
            {
                user.IsActive = false;
            }
            await db.SaveChangesAsync();
        }
        await AuthenticateAsync("READONLY");
        var before = await SnapshotAsync();

        using var response = await Client.GetAsync("/api/issues/suggest-assignee");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Nessun utente disponibile per l'assegnazione.", body.RootElement.GetProperty("message").GetString());
        Assert.Equal(before, await SnapshotAsync());
    }

    private async Task<int> PrepareCandidatesAsync(int adminLoad)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var adminId = await db.Users.Where(user => user.Email == Email("ADMIN")).Select(user => user.Id).SingleAsync();
        foreach (var user in await db.Users.Where(user => user.IsActive && user.Role != UserRole.Readonly &&
            user.Id != adminId && user.Id != UserId).ToListAsync())
        {
            user.IsActive = false;
        }
        (await db.Issues.SingleAsync(issue => issue.Id == OriginalIssueId)).Status = IssueStatus.InProgress;
        for (var index = 0; index < adminLoad; index++)
        {
            db.Issues.Add(AssignedIssue(adminId, IssueStatus.Todo));
        }
        await db.SaveChangesAsync();
        return adminId;
    }

    private Issue AssignedIssue(int? assignedToId, IssueStatus status, bool archived = false) => new()
    {
        Title = "Issue per il calcolo del carico", Description = "Fixture del suggerimento assegnatario.",
        Type = IssueType.Bug, Status = status, CreatedById = UserId, AssignedToId = assignedToId,
        IsArchived = archived
    };
}

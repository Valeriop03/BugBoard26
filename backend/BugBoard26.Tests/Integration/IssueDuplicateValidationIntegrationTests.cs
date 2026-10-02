using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BugBoard26.Api.Data;
using BugBoard26.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BugBoard26.Tests.Integration;

[Trait("Category", "Integration")]
public class IssueDuplicateValidationIntegrationTests : ApiIntegrationTest
{
    [Theory]
    [InlineData("ADMIN")]
    [InlineData("USER")]
    public async Task StatusEndpoint_CannotSetDuplicate_EvenForAdmin_AndDatabaseIsUnchanged(string role)
    {
        await AuthenticateAsync(role);
        var before = await SnapshotAsync();

        using var response = await Client.PatchAsJsonAsync($"/api/issues/{IssueId}/status", new { status = "Duplicate" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Contains("/duplicate", body.RootElement.GetProperty("message").GetString()!);
        Assert.Equal(before, await SnapshotAsync());
    }

    [Theory]
    [InlineData("ADMIN", "Todo")]
    [InlineData("ADMIN", "InProgress")]
    [InlineData("ADMIN", "Resolved")]
    [InlineData("ADMIN", "Closed")]
    [InlineData("USER", "Todo")]
    [InlineData("USER", "InProgress")]
    [InlineData("USER", "Resolved")]
    [InlineData("USER", "Closed")]
    public async Task DuplicatedIssue_CannotChangeStatus_AndDatabaseIsUnchanged(string role, string status)
    {
        await AuthenticateAsync("ADMIN");
        using var duplicateResponse = await Client.PatchAsJsonAsync($"/api/issues/{IssueId}/duplicate", new
        {
            originalIssueId = OriginalIssueId
        });
        Assert.Equal(HttpStatusCode.OK, duplicateResponse.StatusCode);
        await AuthenticateAsync(role);
        var before = await SnapshotAsync();

        using var response = await Client.PatchAsJsonAsync($"/api/issues/{IssueId}/status", new { status });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Contains("duplicat", body.RootElement.GetProperty("message").GetString()!, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(before, await SnapshotAsync());
    }

    [Theory]
    [InlineData(IssueStatus.Duplicate, false)]
    [InlineData(IssueStatus.Todo, true)]
    public async Task LegacyDuplicateMarkers_BlockStatusChange_AndDatabaseIsUnchanged(IssueStatus status, bool hasReference)
    {
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var issue = await db.Issues.SingleAsync(issue => issue.Id == IssueId);
            issue.Status = status;
            issue.DuplicateOfIssueId = hasReference ? OriginalIssueId : null;
            await db.SaveChangesAsync();
        }
        await AuthenticateAsync("ADMIN");
        var before = await SnapshotAsync();

        using var response = await Client.PatchAsJsonAsync($"/api/issues/{IssueId}/status", new { status = "Resolved" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(before, await SnapshotAsync());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Admin_DuplicatesIssue_WithCoherentDatesAndNoNewResolutionNotification(bool resolveFirst)
    {
        await AuthenticateAsync("ADMIN");
        if (resolveFirst)
        {
            using var resolvedResponse = await Client.PatchAsJsonAsync($"/api/issues/{IssueId}/status", new { status = "Resolved" });
            Assert.Equal(HttpStatusCode.OK, resolvedResponse.StatusCode);
        }
        var before = await ReadIssueFromDatabaseAsync(IssueId);
        Assert.Equal(resolveFirst, before.ResolvedAt.HasValue);
        var originalBefore = await ReadIssueFromDatabaseAsync(OriginalIssueId);
        var notificationsBefore = await ReadNotificationsFromDatabaseAsync();
        Assert.Equal(resolveFirst ? 1 : 0, notificationsBefore.Length);

        using var response = await Client.PatchAsJsonAsync($"/api/issues/{IssueId}/duplicate", new
        {
            originalIssueId = OriginalIssueId
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var returned = await ReadIssueFromResponseAsync(response);
        var saved = await ReadIssueFromDatabaseAsync(IssueId);
        Assert.Equal(IssueStatus.Duplicate, returned.Status);
        Assert.Equal(IssueStatus.Duplicate, saved.Status);
        Assert.Equal(OriginalIssueId, returned.DuplicateOfIssueId);
        Assert.Equal(OriginalIssueId, saved.DuplicateOfIssueId);
        Assert.Null(returned.ResolvedAt);
        Assert.Null(saved.ResolvedAt);
        Assert.True(saved.UpdatedAt > before.UpdatedAt);
        Assert.Equal(saved.UpdatedAt, returned.UpdatedAt);
        Assert.Equal(before.CreatedAt, saved.CreatedAt);
        Assert.Equal(before.CreatedById, saved.CreatedById);
        Assert.Equal(before.AssignedToId, saved.AssignedToId);
        Assert.Equal(JsonSerializer.Serialize(originalBefore), JsonSerializer.Serialize(await ReadIssueFromDatabaseAsync(OriginalIssueId)));
        Assert.Equal(JsonSerializer.Serialize(notificationsBefore), JsonSerializer.Serialize(await ReadNotificationsFromDatabaseAsync()));
    }

    private async Task<Notification[]> ReadNotificationsFromDatabaseAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await db.Notifications.AsNoTracking().OrderBy(notification => notification.Id).ToArrayAsync();
    }
}

using System.Net;
using System.Net.Http.Json;
using BugBoard26.Api.Contracts.Notifications;
using BugBoard26.Api.Data;
using BugBoard26.Api.Models;
using BugBoard26.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BugBoard26.Tests.Integration;

[Trait("Category", "Integration")]
public class IssueStatusIntegrationTests : ApiIntegrationTest
{
    [Fact]
    public async Task AssignedUser_CanResolveIssue_WithPersistedDatesAndNotificationForDifferentCreator()
    {
        var creatorId = await CreateOtherUserAsync();
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var issue = await db.Issues.SingleAsync(issue => issue.Id == IssueId);
            issue.CreatedById = creatorId;
            await db.SaveChangesAsync();
        }
        await AuthenticateAsync("USER");
        var before = await ReadIssueFromDatabaseAsync(IssueId);

        using var response = await Client.PatchAsJsonAsync($"/api/issues/{IssueId}/status", new { status = "Resolved" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var returned = await ReadIssueFromResponseAsync(response);
        var saved = await ReadIssueFromDatabaseAsync(IssueId);
        Assert.Equal(IssueStatus.Resolved, returned.Status);
        Assert.Equal(IssueStatus.Resolved, saved.Status);
        Assert.Equal(creatorId, saved.CreatedById);
        Assert.Equal(UserId, saved.AssignedToId);
        Assert.NotNull(saved.ResolvedAt);
        Assert.True(saved.UpdatedAt > before.UpdatedAt);
        Assert.True(saved.ResolvedAt.Value >= saved.UpdatedAt);
        Assert.Equal(saved.UpdatedAt, returned.UpdatedAt);
        Assert.Equal(saved.ResolvedAt, returned.ResolvedAt);

        var notification = Assert.Single(await ReadNotificationsFromDatabaseAsync());
        Assert.Equal(creatorId, notification.UserId);
        Assert.Equal(IssueId, notification.IssueId);
        Assert.False(notification.IsRead);
        Assert.Contains(saved.Title, notification.Message);
        Assert.True(notification.CreatedAt >= saved.ResolvedAt.Value);

        var assigneeNotifications = await Client.GetFromJsonAsync<NotificationResponse[]>("/api/notifications");
        Assert.NotNull(assigneeNotifications);
        Assert.Empty(assigneeNotifications);
        await AuthenticateAsync("OTHER");
        var creatorNotifications = await Client.GetFromJsonAsync<NotificationResponse[]>("/api/notifications");
        Assert.NotNull(creatorNotifications);
        var visibleNotification = Assert.Single(creatorNotifications);
        Assert.Equal(notification.Id, visibleNotification.Id);
        Assert.Equal(creatorId, visibleNotification.UserId);
        Assert.Equal(IssueId, visibleNotification.IssueId);
    }

    [Theory]
    [InlineData("OTHER", false)]
    [InlineData("READONLY", false)]
    [InlineData("READONLY", true)]
    public async Task OtherUserOrReadonly_CannotResolveIssue_AndDatabaseIsUnchanged(string role, bool assignReadonly)
    {
        await CreateOtherUserAsync();
        if (assignReadonly)
        {
            using var scope = Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var readonlyUser = await db.Users.SingleAsync(user => user.Email == Email("READONLY"));
            var issue = await db.Issues.SingleAsync(issue => issue.Id == IssueId);
            issue.AssignedToId = readonlyUser.Id;
            await db.SaveChangesAsync();
        }
        await AuthenticateAsync(role);
        var before = await SnapshotAsync();

        using var response = await Client.PatchAsJsonAsync($"/api/issues/{IssueId}/status", new { status = "Resolved" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(before, await SnapshotAsync());
    }

    [Theory]
    [InlineData("InProgress", IssueStatus.InProgress)]
    [InlineData("Resolved", IssueStatus.Resolved)]
    [InlineData("Closed", IssueStatus.Closed)]
    public async Task Admin_CanChangeStatusOfIssueAssignedToAnotherUser(string status, IssueStatus expectedStatus)
    {
        await AuthenticateAsync("ADMIN");
        var before = await ReadIssueFromDatabaseAsync(IssueId);

        using var response = await Client.PatchAsJsonAsync($"/api/issues/{IssueId}/status", new { status });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var returned = await ReadIssueFromResponseAsync(response);
        var saved = await ReadIssueFromDatabaseAsync(IssueId);
        Assert.Equal(expectedStatus, returned.Status);
        Assert.Equal(expectedStatus, saved.Status);
        Assert.Equal(UserId, saved.AssignedToId);
        Assert.Equal(UserId, saved.CreatedById);
        Assert.True(saved.UpdatedAt > before.UpdatedAt);
        Assert.Equal(saved.UpdatedAt, returned.UpdatedAt);
        Assert.Equal(saved.ResolvedAt, returned.ResolvedAt);

        var notifications = await ReadNotificationsFromDatabaseAsync();
        if (expectedStatus == IssueStatus.Resolved)
        {
            Assert.NotNull(saved.ResolvedAt);
            var notification = Assert.Single(notifications);
            Assert.Equal(UserId, notification.UserId);
            Assert.Equal(IssueId, notification.IssueId);
            Assert.False(notification.IsRead);
        }
        else
        {
            Assert.Null(saved.ResolvedAt);
            Assert.Empty(notifications);
        }
    }

    [Fact]
    public async Task RepeatingResolved_DoesNotCreateAnotherNotificationOrChangeDates()
    {
        await AuthenticateAsync("USER");
        using var firstResponse = await Client.PatchAsJsonAsync($"/api/issues/{IssueId}/status", new { status = "Resolved" });
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        var resolved = await ReadIssueFromDatabaseAsync(IssueId);
        Assert.Equal(IssueStatus.Resolved, resolved.Status);
        Assert.NotNull(resolved.ResolvedAt);
        var firstNotification = Assert.Single(await ReadNotificationsFromDatabaseAsync());
        var beforeRepeat = await SnapshotAsync();

        using var repeatedResponse = await Client.PatchAsJsonAsync($"/api/issues/{IssueId}/status", new { status = "Resolved" });

        Assert.Equal(HttpStatusCode.OK, repeatedResponse.StatusCode);
        var returned = await ReadIssueFromResponseAsync(repeatedResponse);
        Assert.Equal(resolved.UpdatedAt, returned.UpdatedAt);
        Assert.Equal(resolved.ResolvedAt, returned.ResolvedAt);
        Assert.Equal(firstNotification.Id, Assert.Single(await ReadNotificationsFromDatabaseAsync()).Id);
        Assert.Equal(beforeRepeat, await SnapshotAsync());
    }

    [Fact]
    public async Task WithoutToken_CannotChangeStatus_AndDatabaseIsUnchanged()
    {
        var before = await SnapshotAsync();

        using var response = await Client.PatchAsJsonAsync($"/api/issues/{IssueId}/status", new { status = "Resolved" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(before, await SnapshotAsync());
    }

    [Fact]
    public async Task MissingIssue_CannotChangeStatus_AndDatabaseIsUnchanged()
    {
        await AuthenticateAsync("ADMIN");
        var before = await SnapshotAsync();

        using var response = await Client.PatchAsJsonAsync($"/api/issues/{int.MaxValue}/status", new { status = "Resolved" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(before, await SnapshotAsync());
    }

    private async Task<int> CreateOtherUserAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var user = new User
        {
            Email = Email("OTHER"),
            PasswordHash = hasher.Hash(Password),
            Role = UserRole.User,
            IsActive = true
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user.Id;
    }

    private async Task<Notification[]> ReadNotificationsFromDatabaseAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await db.Notifications.AsNoTracking().OrderBy(notification => notification.Id).ToArrayAsync();
    }
}

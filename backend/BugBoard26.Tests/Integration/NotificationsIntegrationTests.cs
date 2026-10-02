using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BugBoard26.Api.Contracts.Notifications;
using BugBoard26.Api.Data;
using BugBoard26.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BugBoard26.Tests.Integration;

[Trait("Category", "Integration")]
public class NotificationsIntegrationTests : ApiIntegrationTest
{
    [Theory]
    [InlineData("USER")]
    [InlineData("READONLY")]
    [InlineData("ADMIN")]
    public async Task List_ReturnsOnlyCurrentUsersNotifications_IncludingReadAndUnread(string role)
    {
        var data = await CreateNotificationsAsync(role);
        await AuthenticateAsync(role);
        var before = await SnapshotAsync();

        using var response = await Client.GetAsync("/api/notifications");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var notifications = await response.Content.ReadFromJsonAsync<NotificationResponse[]>();
        Assert.NotNull(notifications);
        Assert.Equal(new[] { data.UnreadId, data.ReadId }, notifications.Select(notification => notification.Id).ToArray());
        Assert.All(notifications, notification => Assert.Equal(data.OwnerId, notification.UserId));
        Assert.DoesNotContain(notifications, notification => notification.Id == data.OtherId);
        Assert.False(notifications[0].IsRead);
        Assert.True(notifications[1].IsRead);
        Assert.Equal(IssueId, notifications[0].IssueId);
        Assert.Equal("Errore LOGIN", notifications[0].IssueTitle);
        Assert.Equal("Notifica personale non letta.", notifications[0].Message);
        Assert.Equal(before, await SnapshotAsync());
    }

    [Theory]
    [InlineData("USER")]
    [InlineData("READONLY")]
    [InlineData("ADMIN")]
    public async Task Owner_CanMarkNotificationAsRead_WithoutChangingOtherNotifications(string role)
    {
        var data = await CreateNotificationsAsync(role);
        await AuthenticateAsync(role);
        var expectedNotifications = await ReadNotificationsFromDatabaseAsync();
        var expectedRead = Assert.Single(expectedNotifications, notification => notification.Id == data.UnreadId);
        Assert.False(expectedRead.IsRead);
        expectedRead.IsRead = true;
        var issueBefore = await ReadIssueFromDatabaseAsync(IssueId);

        using var response = await Client.PatchAsJsonAsync($"/api/notifications/{data.UnreadId}/read", new { });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var returned = await response.Content.ReadFromJsonAsync<NotificationResponse>();
        Assert.NotNull(returned);
        Assert.Equal(data.UnreadId, returned.Id);
        Assert.Equal(data.OwnerId, returned.UserId);
        Assert.Equal(IssueId, returned.IssueId);
        Assert.True(returned.IsRead);
        Assert.Equal(expectedRead.Message, returned.Message);
        Assert.Equal(expectedRead.CreatedAt, returned.CreatedAt);

        var savedNotifications = await ReadNotificationsFromDatabaseAsync();
        Assert.Equal(JsonSerializer.Serialize(expectedNotifications), JsonSerializer.Serialize(savedNotifications));
        Assert.Equal(issueBefore.UpdatedAt, (await ReadIssueFromDatabaseAsync(IssueId)).UpdatedAt);
        using var listResponse = await Client.GetAsync("/api/notifications");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var visibleNotifications = await listResponse.Content.ReadFromJsonAsync<NotificationResponse[]>();
        Assert.NotNull(visibleNotifications);
        Assert.True(Assert.Single(visibleNotifications, notification => notification.Id == data.UnreadId).IsRead);
    }

    [Theory]
    [InlineData("USER")]
    [InlineData("READONLY")]
    [InlineData("ADMIN")]
    public async Task NonOwner_CannotMarkAnotherUsersNotificationAsRead_AndDatabaseIsUnchanged(string role)
    {
        var data = await CreateNotificationsAsync(role);
        await AuthenticateAsync(role);
        var before = await SnapshotAsync();

        using var response = await Client.PatchAsJsonAsync($"/api/notifications/{data.OtherId}/read", new { });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(before, await SnapshotAsync());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task WithoutToken_CannotListOrReadNotifications_AndDatabaseIsUnchanged(bool markAsRead)
    {
        var data = await CreateNotificationsAsync("USER");
        var before = await SnapshotAsync();

        using var response = markAsRead
            ? await Client.PatchAsJsonAsync($"/api/notifications/{data.UnreadId}/read", new { })
            : await Client.GetAsync("/api/notifications");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(before, await SnapshotAsync());
    }

    [Fact]
    public async Task MissingNotification_CannotBeMarkedAsRead_AndDatabaseIsUnchanged()
    {
        await CreateNotificationsAsync("USER");
        await AuthenticateAsync("USER");
        var before = await SnapshotAsync();

        using var response = await Client.PatchAsJsonAsync($"/api/notifications/{int.MaxValue}/read", new { });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(before, await SnapshotAsync());
    }

    [Fact]
    public async Task UserWithoutNotifications_GetsEmptyListEvenWhenAnotherUserHasNotifications()
    {
        await CreateNotificationsAsync("USER");
        await AuthenticateAsync("READONLY");
        var before = await SnapshotAsync();

        using var response = await Client.GetAsync("/api/notifications");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var notifications = await response.Content.ReadFromJsonAsync<NotificationResponse[]>();
        Assert.NotNull(notifications);
        Assert.Empty(notifications);
        Assert.Equal(before, await SnapshotAsync());
    }

    private async Task<(int OwnerId, int UnreadId, int ReadId, int OtherId)> CreateNotificationsAsync(string ownerRole)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var owner = await db.Users.SingleAsync(user => user.Email == Email(ownerRole));
        var otherRole = ownerRole == "USER" ? "ADMIN" : "USER";
        var otherUser = await db.Users.SingleAsync(user => user.Email == Email(otherRole));
        var createdAt = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc);
        var unread = new Notification
        {
            UserId = owner.Id,
            IssueId = IssueId,
            Message = "Notifica personale non letta.",
            IsRead = false,
            CreatedAt = createdAt
        };
        var read = new Notification
        {
            UserId = owner.Id,
            IssueId = OriginalIssueId,
            Message = "Notifica personale gia' letta.",
            IsRead = true,
            CreatedAt = createdAt.AddMinutes(-1)
        };
        var other = new Notification
        {
            UserId = otherUser.Id,
            IssueId = IssueId,
            Message = "Notifica di un altro utente sulla stessa issue.",
            IsRead = false,
            CreatedAt = createdAt.AddMinutes(1)
        };
        db.Notifications.AddRange(unread, read, other);
        await db.SaveChangesAsync();
        return (owner.Id, unread.Id, read.Id, other.Id);
    }

    private async Task<Notification[]> ReadNotificationsFromDatabaseAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await db.Notifications.AsNoTracking().OrderBy(notification => notification.Id).ToArrayAsync();
    }
}

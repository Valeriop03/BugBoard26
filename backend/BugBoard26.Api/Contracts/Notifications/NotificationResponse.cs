namespace BugBoard26.Api.Contracts.Notifications;

public class NotificationResponse
{
    public int Id { get; init; }

    public int UserId { get; init; }

    public int IssueId { get; init; }

    public string IssueTitle { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;

    public bool IsRead { get; init; }

    public DateTime CreatedAt { get; init; }
}

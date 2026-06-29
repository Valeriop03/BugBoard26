namespace BugBoard26.Api.Models;

public class Issue
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IssueType Type { get; set; }
    public IssuePriority? Priority { get; set; }
    public IssueStatus Status { get; set; } = IssueStatus.Todo;
    public int CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;
    public int? AssignedToId { get; set; }
    public User? AssignedTo { get; set; }
    public int? DuplicateOfIssueId { get; set; }
    public Issue? DuplicateOfIssue { get; set; }
    public string? ImagePath { get; set; }
    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}

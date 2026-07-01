using BugBoard26.Api.Models;

namespace BugBoard26.Api.Contracts.Issues;

public class IssueResponse
{
    public int Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public IssueType Type { get; init; }

    public IssuePriority? Priority { get; init; }

    public IssueStatus Status { get; init; }

    public int CreatedById { get; init; }

    public string CreatedByEmail { get; init; } = string.Empty;

    public int? AssignedToId { get; init; }

    public string? AssignedToEmail { get; init; }

    public int? DuplicateOfIssueId { get; init; }

    public bool IsArchived { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; init; }

    public DateTime? ResolvedAt { get; init; }
}

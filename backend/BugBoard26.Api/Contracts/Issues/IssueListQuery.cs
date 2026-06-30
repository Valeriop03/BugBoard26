using BugBoard26.Api.Models;

namespace BugBoard26.Api.Contracts.Issues;

public class IssueListQuery
{
    public IssueType? Type { get; set; }

    public IssueStatus? Status { get; set; }

    public IssuePriority? Priority { get; set; }

    public string? Keyword { get; set; }

    public IssueSortField SortBy { get; set; } = IssueSortField.Date;

    public SortDirection SortDirection { get; set; } = SortDirection.Desc;
}

public enum IssueSortField
{
    Date,
    Priority,
    Status,
    Title
}

public enum SortDirection
{
    Asc,
    Desc
}

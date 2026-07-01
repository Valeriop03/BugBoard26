namespace BugBoard26.Api.Contracts.Issues;

public class SuggestAssigneeResponse
{
    public int UserId { get; init; }

    public string Email { get; init; } = string.Empty;

    public int OpenAssignedIssues { get; init; }
}

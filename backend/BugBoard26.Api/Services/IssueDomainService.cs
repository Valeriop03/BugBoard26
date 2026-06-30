using BugBoard26.Api.Models;

namespace BugBoard26.Api.Services;

public class IssueDomainService
{
    public IEnumerable<Issue> SearchIssues(IEnumerable<Issue> issues, string? keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return issues;
        }

        var normalizedKeyword = keyword.Trim();

        return issues.Where(issue =>
            issue.Title.Contains(normalizedKeyword, StringComparison.OrdinalIgnoreCase) ||
            issue.Description.Contains(normalizedKeyword, StringComparison.OrdinalIgnoreCase));
    }

    public bool CanChangeIssueStatus(User user, Issue issue)
    {
        if (!user.IsActive || user.Role == UserRole.Readonly)
        {
            return false;
        }

        return user.Role == UserRole.Admin || issue.AssignedToId == user.Id;
    }

    public User? SuggestAssignee(IEnumerable<User> users, IEnumerable<Issue> issues)
    {
        var assignableUsers = users
            .Where(user => user.IsActive && user.Role != UserRole.Readonly)
            .ToList();

        if (assignableUsers.Count == 0)
        {
            return null;
        }

        return assignableUsers
            .OrderBy(user => CountOpenAssignedIssues(user.Id, issues))
            .ThenBy(user => user.Id)
            .First();
    }

    private static int CountOpenAssignedIssues(int userId, IEnumerable<Issue> issues)
    {
        return issues.Count(issue =>
            issue.AssignedToId == userId &&
            !issue.IsArchived &&
            (issue.Status == IssueStatus.Todo || issue.Status == IssueStatus.InProgress));
    }
}

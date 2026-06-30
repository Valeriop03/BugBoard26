using BugBoard26.Api.Models;
using BugBoard26.Api.Services;

namespace BugBoard26.Tests;

public class IssueDomainServiceTests
{
    private readonly IssueDomainService _service = new();

    [Theory]
    [InlineData("login", 1)]
    [InlineData("API", 1)]
    [InlineData("grafica", 0)]
    public void SearchIssues_FiltersByTitleOrDescription(string keyword, int expectedCount)
    {
        var issues = new List<Issue>
        {
            new() { Id = 1, Title = "Errore login", Description = "La password corretta non viene accettata" },
            new() { Id = 2, Title = "Export CSV", Description = "Aggiungere endpoint API per esportare le issue" },
            new() { Id = 3, Title = "Documentazione", Description = "Aggiornare il testo della guida" }
        };

        var result = _service.SearchIssues(issues, keyword).ToList();

        Assert.Equal(expectedCount, result.Count);
    }

    [Theory]
    [InlineData(UserRole.Admin, 10, null, true, true)]
    [InlineData(UserRole.User, 10, 10, true, true)]
    [InlineData(UserRole.User, 10, 20, true, false)]
    [InlineData(UserRole.Readonly, 10, 10, true, false)]
    [InlineData(UserRole.User, 10, 10, false, false)]
    public void CanChangeIssueStatus_ChecksRoleAndAssignment(
        UserRole role,
        int userId,
        int? assignedToId,
        bool isActive,
        bool expected)
    {
        var user = new User
        {
            Id = userId,
            Role = role,
            IsActive = isActive
        };
        var issue = new Issue
        {
            Id = 1,
            AssignedToId = assignedToId
        };

        var result = _service.CanChangeIssueStatus(user, issue);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0, 2, 1)]
    [InlineData(3, 1, 2)]
    public void SuggestAssignee_ReturnsUserWithLessOpenIssues(
        int firstUserOpenIssues,
        int secondUserOpenIssues,
        int expectedUserId)
    {
        var users = new List<User>
        {
            new() { Id = 1, Role = UserRole.User, IsActive = true },
            new() { Id = 2, Role = UserRole.User, IsActive = true },
            new() { Id = 3, Role = UserRole.Readonly, IsActive = true }
        };
        var issues = CreateAssignedIssues(1, firstUserOpenIssues)
            .Concat(CreateAssignedIssues(2, secondUserOpenIssues))
            .Concat(new[]
            {
                new Issue { AssignedToId = 3, Status = IssueStatus.Todo },
                new Issue { AssignedToId = 1, Status = IssueStatus.Resolved },
                new Issue { AssignedToId = 2, Status = IssueStatus.Closed }
            })
            .ToList();

        var result = _service.SuggestAssignee(users, issues);

        Assert.NotNull(result);
        Assert.Equal(expectedUserId, result.Id);
    }

    [Fact]
    public void SuggestAssignee_ReturnsNullWhenThereAreNoAssignableUsers()
    {
        var users = new List<User>
        {
            new() { Id = 1, Role = UserRole.Readonly, IsActive = true },
            new() { Id = 2, Role = UserRole.User, IsActive = false }
        };

        var result = _service.SuggestAssignee(users, new List<Issue>());

        Assert.Null(result);
    }

    private static IEnumerable<Issue> CreateAssignedIssues(int userId, int count)
    {
        return Enumerable.Range(1, count)
            .Select(_ => new Issue
            {
                AssignedToId = userId,
                Status = IssueStatus.Todo
            });
    }
}

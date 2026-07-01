using BugBoard26.Api.Contracts.Issues;
using BugBoard26.Api.Data;
using BugBoard26.Api.Extensions;
using BugBoard26.Api.Models;
using BugBoard26.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Text;

namespace BugBoard26.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IssuesController : ControllerBase
{
    private static readonly Expression<Func<Issue, int>> PriorityOrderExpression = issue =>
        issue.Priority == IssuePriority.Low ? 1 :
        issue.Priority == IssuePriority.Medium ? 2 :
        issue.Priority == IssuePriority.High ? 3 :
        issue.Priority == IssuePriority.Critical ? 4 : 5;

    private static readonly Expression<Func<Issue, int>> StatusOrderExpression = issue =>
        issue.Status == IssueStatus.Todo ? 1 :
        issue.Status == IssueStatus.InProgress ? 2 :
        issue.Status == IssueStatus.Resolved ? 3 :
        issue.Status == IssueStatus.Closed ? 4 : 5;

    private readonly ApplicationDbContext _dbContext;
    private readonly IssueDomainService _issueDomainService;

    public IssuesController(ApplicationDbContext dbContext, IssueDomainService issueDomainService)
    {
        _dbContext = dbContext;
        _issueDomainService = issueDomainService;
    }

    [HttpPost]
    public async Task<ActionResult<IssueResponse>> Create([FromBody] CreateIssueRequest request, CancellationToken cancellationToken)
    {
        var currentUser = await GetCurrentUserAsync(cancellationToken);

        if (currentUser is null)
        {
            return Unauthorized();
        }

        if (currentUser.Role == UserRole.Readonly)
        {
            return Forbid();
        }

        var title = (request.Title ?? string.Empty).Trim();
        var description = (request.Description ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(title))
        {
            ModelState.AddModelError(nameof(request.Title), "Title is required.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            ModelState.AddModelError(nameof(request.Description), "Description is required.");
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (request.AssignedToId.HasValue)
        {
            var assigneeExists = await _dbContext.Users
                .AsNoTracking()
                .AnyAsync(user => user.Id == request.AssignedToId.Value, cancellationToken);

            if (!assigneeExists)
            {
                ModelState.AddModelError(nameof(request.AssignedToId), "Assigned user does not exist.");
                return ValidationProblem(ModelState);
            }
        }

        var issue = new Issue
        {
            Title = title,
            Description = description,
            Type = request.Type!.Value,
            Priority = request.Priority,
            Status = IssueStatus.Todo,
            CreatedById = currentUser.Id,
            AssignedToId = request.AssignedToId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Issues.Add(issue);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var createdIssue = await ProjectToResponse(_dbContext.Issues.AsNoTracking())
            .SingleAsync(currentIssue => currentIssue.Id == issue.Id, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = issue.Id }, createdIssue);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<IssueResponse>>> GetAll([FromQuery] IssueListQuery query, CancellationToken cancellationToken)
    {
        var issuesQuery = ApplyIssueQuery(_dbContext.Issues.AsNoTracking(), query);

        var issues = await ProjectToResponse(issuesQuery).ToListAsync(cancellationToken);

        return Ok(issues);
    }

    [HttpGet("suggest-assignee")]
    public async Task<ActionResult<SuggestAssigneeResponse>> SuggestAssignee(CancellationToken cancellationToken)
    {
        var users = await _dbContext.Users
            .AsNoTracking()
            .Where(user => user.IsActive && user.Role != UserRole.Readonly)
            .ToListAsync(cancellationToken);

        var issues = await _dbContext.Issues
            .AsNoTracking()
            .Where(issue => !issue.IsArchived &&
                (issue.Status == IssueStatus.Todo || issue.Status == IssueStatus.InProgress))
            .ToListAsync(cancellationToken);

        var suggestedUser = _issueDomainService.SuggestAssignee(users, issues);

        if (suggestedUser is null)
        {
            return NotFound(new { message = "Nessun utente disponibile per l'assegnazione." });
        }

        return Ok(new SuggestAssigneeResponse
        {
            UserId = suggestedUser.Id,
            Email = suggestedUser.Email,
            OpenAssignedIssues = issues.Count(issue => issue.AssignedToId == suggestedUser.Id)
        });
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] IssueListQuery query, CancellationToken cancellationToken)
    {
        var issues = await ProjectToResponse(ApplyIssueQuery(_dbContext.Issues.AsNoTracking(), query))
            .ToListAsync(cancellationToken);

        var csv = BuildCsv(issues);
        var fileName = $"bugboard26-issues-{DateTime.UtcNow:yyyyMMdd}.csv";

        return File(Encoding.UTF8.GetBytes(csv), "text/csv; charset=utf-8", fileName);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<IssueResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var issue = await ProjectToResponse(_dbContext.Issues.AsNoTracking())
            .SingleOrDefaultAsync(currentIssue => currentIssue.Id == id, cancellationToken);

        if (issue is null)
        {
            return NotFound();
        }

        return Ok(issue);
    }

    private async Task<User?> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (!userId.HasValue)
        {
            return null;
        }

        return await _dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(user => user.Id == userId.Value && user.IsActive, cancellationToken);
    }

    private static IQueryable<Issue> ApplyIssueQuery(IQueryable<Issue> issuesQuery, IssueListQuery query)
    {
        issuesQuery = issuesQuery.Where(issue => !issue.IsArchived);

        if (query.Type.HasValue)
        {
            issuesQuery = issuesQuery.Where(issue => issue.Type == query.Type.Value);
        }

        if (query.Status.HasValue)
        {
            issuesQuery = issuesQuery.Where(issue => issue.Status == query.Status.Value);
        }

        if (query.Priority.HasValue)
        {
            issuesQuery = issuesQuery.Where(issue => issue.Priority == query.Priority.Value);
        }

        var keyword = query.Keyword?.Trim();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var pattern = $"%{keyword}%";

            issuesQuery = issuesQuery.Where(issue =>
                EF.Functions.ILike(issue.Title, pattern) ||
                EF.Functions.ILike(issue.Description, pattern));
        }

        return ApplySorting(issuesQuery, query);
    }

    private static IQueryable<Issue> ApplySorting(IQueryable<Issue> issuesQuery, IssueListQuery query)
    {
        return (query.SortBy, query.SortDirection) switch
        {
            (IssueSortField.Priority, SortDirection.Asc) => issuesQuery
                .OrderBy(PriorityOrderExpression)
                .ThenByDescending(issue => issue.CreatedAt),
            (IssueSortField.Priority, SortDirection.Desc) => issuesQuery
                .OrderByDescending(PriorityOrderExpression)
                .ThenByDescending(issue => issue.CreatedAt),
            (IssueSortField.Status, SortDirection.Asc) => issuesQuery
                .OrderBy(StatusOrderExpression)
                .ThenByDescending(issue => issue.CreatedAt),
            (IssueSortField.Status, SortDirection.Desc) => issuesQuery
                .OrderByDescending(StatusOrderExpression)
                .ThenByDescending(issue => issue.CreatedAt),
            (IssueSortField.Title, SortDirection.Asc) => issuesQuery
                .OrderBy(issue => issue.Title)
                .ThenByDescending(issue => issue.CreatedAt),
            (IssueSortField.Title, SortDirection.Desc) => issuesQuery
                .OrderByDescending(issue => issue.Title)
                .ThenByDescending(issue => issue.CreatedAt),
            (IssueSortField.Date, SortDirection.Asc) => issuesQuery
                .OrderBy(issue => issue.CreatedAt),
            _ => issuesQuery
                .OrderByDescending(issue => issue.CreatedAt)
        };
    }

    private static IQueryable<IssueResponse> ProjectToResponse(IQueryable<Issue> issuesQuery)
    {
        return issuesQuery.Select(issue => new IssueResponse
        {
            Id = issue.Id,
            Title = issue.Title,
            Description = issue.Description,
            Type = issue.Type,
            Priority = issue.Priority,
            Status = issue.Status,
            CreatedById = issue.CreatedById,
            CreatedByEmail = issue.CreatedBy.Email,
            AssignedToId = issue.AssignedToId,
            AssignedToEmail = issue.AssignedTo != null ? issue.AssignedTo.Email : null,
            IsArchived = issue.IsArchived,
            CreatedAt = issue.CreatedAt,
            UpdatedAt = issue.UpdatedAt,
            ResolvedAt = issue.ResolvedAt
        });
    }

    private static string BuildCsv(IEnumerable<IssueResponse> issues)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Id,Title,Type,Priority,Status,CreatedByEmail,AssignedToEmail,CreatedAt");

        foreach (var issue in issues)
        {
            var values = new[]
            {
                issue.Id.ToString(),
                issue.Title,
                issue.Type.ToString(),
                issue.Priority?.ToString() ?? string.Empty,
                issue.Status.ToString(),
                issue.CreatedByEmail,
                issue.AssignedToEmail ?? string.Empty,
                issue.CreatedAt.ToString("O")
            };

            builder.AppendLine(string.Join(",", values.Select(EscapeCsvValue)));
        }

        return builder.ToString();
    }

    private static string EscapeCsvValue(string value)
    {
        if (!value.Contains(',') && !value.Contains('"') && !value.Contains('\n') && !value.Contains('\r'))
        {
            return value;
        }

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}

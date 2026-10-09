using BugBoard26.Api.Contracts.Issues;
using BugBoard26.Api.Data;
using BugBoard26.Api.Extensions;
using BugBoard26.Api.Filters;
using BugBoard26.Api.Models;
using BugBoard26.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Text;

namespace BugBoard26.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
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
    private readonly IssueImageStorage _imageStorage;
    private readonly ILogger<IssuesController> _logger;

    public IssuesController(ApplicationDbContext dbContext, IssueDomainService issueDomainService,
        IssueImageStorage imageStorage, ILogger<IssuesController> logger)
    {
        _dbContext = dbContext;
        _issueDomainService = issueDomainService;
        _imageStorage = imageStorage;
        _logger = logger;
    }

    [HttpPost]
    [Authorize(Roles = UserRoleMapper.Admin + "," + UserRoleMapper.User)]
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
            var assigneeIsValid = await _dbContext.Users
                .AsNoTracking()
                .AnyAsync(user => user.Id == request.AssignedToId.Value &&
                    user.IsActive && user.Role != UserRole.Readonly, cancellationToken);

            if (!assigneeIsValid)
            {
                ModelState.AddModelError(nameof(request.AssignedToId),
                    "L'assegnatario deve essere un utente esistente, attivo e non READONLY.");
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

    [HttpGet("archived")]
    public async Task<ActionResult<IReadOnlyList<IssueResponse>>> GetArchived(CancellationToken cancellationToken)
    {
        var issues = await ProjectToResponse(_dbContext.Issues
                .AsNoTracking()
                .Where(issue => issue.IsArchived)
                .OrderByDescending(issue => issue.UpdatedAt))
            .ToListAsync(cancellationToken);

        return Ok(issues);
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

    [HttpPost("{id:int}/image")]
    [ReadImageFormInAction]
    [Authorize(Roles = UserRoleMapper.Admin + "," + UserRoleMapper.User)]
    [RequestSizeLimit(IssueImageStorage.MaxImageBytes + 65536)]
    [RequestFormLimits(MultipartBodyLengthLimit = IssueImageStorage.MaxImageBytes, ValueCountLimit = 8, ValueLengthLimit = 1024)]
    public async Task<ActionResult<IssueResponse>> UploadImage(int id, CancellationToken cancellationToken)
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
        var issue = await _dbContext.Issues.SingleOrDefaultAsync(issue => issue.Id == id, cancellationToken);
        if (issue is null)
        {
            return NotFound();
        }
        if (currentUser.Role != UserRole.Admin && issue.CreatedById != currentUser.Id)
        {
            return Forbid();
        }

        byte[] data;
        string extension;
        try
        {
            if (!Request.HasFormContentType ||
                !Request.ContentType!.StartsWith("multipart/form-data", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "Usa multipart/form-data con il campo image." });
            }
            var form = await Request.ReadFormAsync(cancellationToken);
            if (form.Files.Count != 1 || form.Files.GetFile("image") is null)
            {
                return BadRequest(new { message = "Invia una sola immagine nel campo image." });
            }
            (data, extension) = await _imageStorage.ValidateAsync(form.Files.GetFile("image"), cancellationToken);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (Exception exception) when (exception is InvalidDataException or BadHttpRequestException or IOException)
        {
            return BadRequest(new { message = "File o multipart non valido. Usa PNG/JPEG di massimo 5242880 byte (5 MiB)." });
        }

        string? newName = null;
        var committed = false;
        try
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            // Serialize replacements of the same issue before writing any file.
            _dbContext.Entry(issue).State = EntityState.Detached;
            issue = await _dbContext.Issues.FromSqlInterpolated(
                $"SELECT * FROM \"Issues\" WHERE \"Id\" = {id} FOR UPDATE")
                .SingleOrDefaultAsync(cancellationToken);
            if (issue is null)
            {
                return NotFound();
            }
            if (currentUser.Role != UserRole.Admin && issue.CreatedById != currentUser.Id)
            {
                return Forbid();
            }

            var oldName = issue.ImagePath;
            newName = await _imageStorage.SaveAsync(data, extension, cancellationToken);
            issue.ImagePath = newName;
            issue.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            committed = true;
            _imageStorage.Delete(oldName);

            var updated = await ProjectToResponse(_dbContext.Issues.AsNoTracking())
                .SingleAsync(issue => issue.Id == id, cancellationToken);
            return Ok(updated);
        }
        catch (Exception exception) when (exception is DbUpdateException or Npgsql.NpgsqlException or IOException or UnauthorizedAccessException)
        {
            _logger.LogError(exception, "Errore durante il salvataggio dell'immagine per la issue {IssueId}.", id);
            return StatusCode(500, new { message = "Non e' stato possibile completare il salvataggio dell'immagine." });
        }
        finally
        {
            if (!committed && newName is not null)
            {
                _imageStorage.Delete(newName);
            }
        }
    }

    [HttpGet("{id:int}/image")]
    public async Task<IActionResult> GetImage(int id, CancellationToken cancellationToken)
    {
        if (await GetCurrentUserAsync(cancellationToken) is null)
        {
            return Unauthorized();
        }
        var issue = await _dbContext.Issues.AsNoTracking().SingleOrDefaultAsync(issue => issue.Id == id, cancellationToken);
        if (issue is null)
        {
            return NotFound();
        }
        FileStream? stream;
        try
        {
            stream = _imageStorage.OpenRead(issue.ImagePath);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            _logger.LogError(exception, "Errore durante la lettura dell'immagine per la issue {IssueId}.", id);
            return StatusCode(500, new { message = "Non e' stato possibile leggere l'immagine." });
        }
        if (stream is null)
        {
            return NotFound();
        }
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        Response.Headers.CacheControl = "private, no-store";
        return File(stream, _imageStorage.GetContentType(issue.ImagePath!));
    }

    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = UserRoleMapper.Admin + "," + UserRoleMapper.User)]
    public async Task<ActionResult<IssueResponse>> UpdateStatus(int id, [FromBody] UpdateIssueStatusRequest request, CancellationToken cancellationToken)
    {
        var currentUser = await GetCurrentUserAsync(cancellationToken);

        if (currentUser is null)
        {
            return Unauthorized();
        }

        var issue = await _dbContext.Issues
            .SingleOrDefaultAsync(currentIssue => currentIssue.Id == id, cancellationToken);

        if (issue is null)
        {
            return NotFound();
        }

        if (!_issueDomainService.CanChangeIssueStatus(currentUser, issue))
        {
            return Forbid();
        }

        var newStatus = request.Status!.Value;

        if (newStatus == IssueStatus.Duplicate)
        {
            return BadRequest(new
            {
                message = $"Per contrassegnare un duplicato usa PATCH /api/issues/{id}/duplicate e indica la issue originale."
            });
        }

        if (issue.Status == IssueStatus.Duplicate || issue.DuplicateOfIssueId.HasValue)
        {
            return Conflict(new
            {
                message = "Non e' possibile cambiare lo stato di una issue contrassegnata come duplicata."
            });
        }

        if (issue.Status != newStatus)
        {
            issue.Status = newStatus;
            issue.UpdatedAt = DateTime.UtcNow;

            if (newStatus == IssueStatus.Resolved)
            {
                issue.ResolvedAt = DateTime.UtcNow;

                _dbContext.Notifications.Add(new Notification
                {
                    UserId = issue.CreatedById,
                    IssueId = issue.Id,
                    Message = $"The issue '{issue.Title}' has been resolved.",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                issue.ResolvedAt = null;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var updatedIssue = await ProjectToResponse(_dbContext.Issues.AsNoTracking())
            .SingleAsync(currentIssue => currentIssue.Id == issue.Id, cancellationToken);

        return Ok(updatedIssue);
    }

    [Authorize(Roles = UserRoleMapper.Admin)]
    [HttpPatch("{id:int}/archive")]
    public async Task<ActionResult<IssueResponse>> Archive(int id, CancellationToken cancellationToken)
    {
        var issue = await _dbContext.Issues.SingleOrDefaultAsync(issue => issue.Id == id, cancellationToken);

        if (issue is null)
        {
            return NotFound();
        }

        issue.IsArchived = true;
        issue.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        var archivedIssue = await ProjectToResponse(_dbContext.Issues.AsNoTracking())
            .SingleAsync(currentIssue => currentIssue.Id == id, cancellationToken);

        return Ok(archivedIssue);
    }

    [Authorize(Roles = UserRoleMapper.Admin)]
    [HttpPatch("{id:int}/duplicate")]
    public async Task<ActionResult<IssueResponse>> MarkAsDuplicate(
        int id,
        [FromBody] DuplicateIssueRequest request,
        CancellationToken cancellationToken)
    {
        if (id == request.OriginalIssueId)
        {
            return BadRequest(new { message = "Una issue non puo' essere duplicata di se stessa." });
        }

        var issue = await _dbContext.Issues.SingleOrDefaultAsync(issue => issue.Id == id, cancellationToken);

        if (issue is null)
        {
            return NotFound();
        }

        var originalExists = await _dbContext.Issues
            .AsNoTracking()
            .AnyAsync(issue => issue.Id == request.OriginalIssueId, cancellationToken);

        if (!originalExists)
        {
            return BadRequest(new { message = "La issue originale indicata non esiste." });
        }

        issue.Status = IssueStatus.Duplicate;
        issue.DuplicateOfIssueId = request.OriginalIssueId;
        issue.ResolvedAt = null;
        issue.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        var duplicatedIssue = await ProjectToResponse(_dbContext.Issues.AsNoTracking())
            .SingleAsync(currentIssue => currentIssue.Id == id, cancellationToken);

        return Ok(duplicatedIssue);
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

        if (query.CreatedById.HasValue)
        {
            issuesQuery = issuesQuery.Where(issue => issue.CreatedById == query.CreatedById.Value);
        }

        if (query.AssignedToId.HasValue)
        {
            issuesQuery = issuesQuery.Where(issue => issue.AssignedToId == query.AssignedToId.Value);
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
            DuplicateOfIssueId = issue.DuplicateOfIssueId,
            IsArchived = issue.IsArchived,
            HasImage = issue.ImagePath != null && issue.ImagePath != "",
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

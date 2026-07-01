using BugBoard26.Api.Contracts.Notifications;
using BugBoard26.Api.Data;
using BugBoard26.Api.Extensions;
using BugBoard26.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BugBoard26.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public NotificationsController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NotificationResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var currentUser = await GetCurrentUserAsync(cancellationToken);

        if (currentUser is null)
        {
            return Unauthorized();
        }

        var notifications = await _dbContext.Notifications
            .AsNoTracking()
            .Where(notification => notification.UserId == currentUser.Id)
            .OrderByDescending(notification => notification.CreatedAt)
            .Select(notification => new NotificationResponse
            {
                Id = notification.Id,
                UserId = notification.UserId,
                IssueId = notification.IssueId,
                IssueTitle = notification.Issue.Title,
                Message = notification.Message,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Ok(notifications);
    }

    [HttpPatch("{id:int}/read")]
    public async Task<ActionResult<NotificationResponse>> MarkAsRead(int id, CancellationToken cancellationToken)
    {
        var currentUser = await GetCurrentUserAsync(cancellationToken);

        if (currentUser is null)
        {
            return Unauthorized();
        }

        var notification = await _dbContext.Notifications
            .Include(currentNotification => currentNotification.Issue)
            .SingleOrDefaultAsync(currentNotification => currentNotification.Id == id, cancellationToken);

        if (notification is null)
        {
            return NotFound();
        }

        if (notification.UserId != currentUser.Id)
        {
            return Forbid();
        }

        notification.IsRead = true;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new NotificationResponse
        {
            Id = notification.Id,
            UserId = notification.UserId,
            IssueId = notification.IssueId,
            IssueTitle = notification.Issue.Title,
            Message = notification.Message,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt
        });
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
}

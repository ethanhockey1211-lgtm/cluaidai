using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Norta.Api.Data;
using Norta.Api.DTOs;
using System.Security.Claims;

namespace Norta.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly AppDbContext _db;

    public NotificationsController(AppDbContext db)
    {
        _db = db;
    }

    private string GetCurrentUserId() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

    [HttpGet]
    public async Task<ActionResult<List<NotificationDto>>> GetNotifications([FromQuery] int page = 0, [FromQuery] int size = 20)
    {
        var userId = GetCurrentUserId();

        var notifications = await _db.Notifications
            .Where(n => n.UserId == userId)
            .Include(n => n.Actor)
            .OrderByDescending(n => n.CreatedAt)
            .Skip(page * size)
            .Take(size)
            .ToListAsync();

        return Ok(notifications.Select(n => new NotificationDto(
            n.Id,
            n.Type,
            n.Actor != null ? new UserSummaryDto(n.Actor.Id, n.Actor.DisplayName, n.Actor.AvatarUrl) : null,
            n.PostId,
            n.Message,
            n.IsRead,
            n.CreatedAt
        )).ToList());
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<int>> GetUnreadCount()
    {
        var userId = GetCurrentUserId();
        var count = await _db.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);

        return Ok(count);
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var userId = GetCurrentUserId();
        var notification = await _db.Notifications.FindAsync(id);

        if (notification == null) return NotFound();
        if (notification.UserId != userId) return Forbid();

        notification.IsRead = true;
        await _db.SaveChangesAsync();

        return Ok();
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = GetCurrentUserId();
        var notifications = await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }

        await _db.SaveChangesAsync();

        return Ok();
    }
}

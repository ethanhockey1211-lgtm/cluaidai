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
public class MessagesController : ControllerBase
{
    private readonly AppDbContext _db;

    public MessagesController(AppDbContext db)
    {
        _db = db;
    }

    private string GetCurrentUserId() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

    [HttpGet("conversations")]
    public async Task<ActionResult<List<ConversationDto>>> GetConversations()
    {
        var userId = GetCurrentUserId();

        // Get all users that current user has messaged or received messages from
        var conversations = await _db.Messages
            .Where(m => m.FromUserId == userId || m.ToUserId == userId)
            .Include(m => m.FromUser)
            .Include(m => m.ToUser)
            .GroupBy(m => m.FromUserId == userId ? m.ToUserId : m.FromUserId)
            .Select(g => new
            {
                OtherUserId = g.Key,
                LastMessage = g.OrderByDescending(m => m.SentAt).First(),
                UnreadCount = g.Count(m => m.ToUserId == userId && !m.Seen)
            })
            .ToListAsync();

        var conversationDtos = new List<ConversationDto>();
        foreach (var conv in conversations)
        {
            var otherUser = await _db.Users.FindAsync(conv.OtherUserId);
            if (otherUser != null)
            {
                conversationDtos.Add(new ConversationDto(
                    new UserSummaryDto(otherUser.Id, otherUser.DisplayName, otherUser.AvatarUrl),
                    new MessageDto(
                        conv.LastMessage.Id,
                        conv.LastMessage.FromUserId,
                        conv.LastMessage.ToUserId,
                        new UserSummaryDto(
                            conv.LastMessage.FromUser!.Id,
                            conv.LastMessage.FromUser.DisplayName,
                            conv.LastMessage.FromUser.AvatarUrl
                        ),
                        conv.LastMessage.Text,
                        conv.LastMessage.Delivered,
                        conv.LastMessage.Seen,
                        conv.LastMessage.SentAt,
                        conv.LastMessage.SeenAt
                    ),
                    conv.UnreadCount
                ));
            }
        }

        return Ok(conversationDtos.OrderByDescending(c => c.LastMessage?.SentAt).ToList());
    }

    [HttpGet("conversation/{otherUserId}")]
    public async Task<ActionResult<List<MessageDto>>> GetConversation(string otherUserId, [FromQuery] int page = 0, [FromQuery] int size = 50)
    {
        var userId = GetCurrentUserId();

        var messages = await _db.Messages
            .Where(m =>
                (m.FromUserId == userId && m.ToUserId == otherUserId) ||
                (m.FromUserId == otherUserId && m.ToUserId == userId))
            .Include(m => m.FromUser)
            .OrderByDescending(m => m.SentAt)
            .Skip(page * size)
            .Take(size)
            .ToListAsync();

        // Mark messages as seen
        var unseenMessages = messages.Where(m => m.ToUserId == userId && !m.Seen).ToList();
        foreach (var msg in unseenMessages)
        {
            msg.Seen = true;
            msg.SeenAt = DateTime.UtcNow;
        }
        if (unseenMessages.Any())
        {
            await _db.SaveChangesAsync();
        }

        return Ok(messages.Select(m => new MessageDto(
            m.Id,
            m.FromUserId,
            m.ToUserId,
            new UserSummaryDto(m.FromUser!.Id, m.FromUser.DisplayName, m.FromUser.AvatarUrl),
            m.Text,
            m.Delivered,
            m.Seen,
            m.SentAt,
            m.SeenAt
        )).Reverse().ToList());
    }
}

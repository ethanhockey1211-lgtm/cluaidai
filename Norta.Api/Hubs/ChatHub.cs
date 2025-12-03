using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Norta.Api.Data;
using Norta.Api.DTOs;
using Norta.Api.Models;
using System.Security.Claims;

namespace Norta.Api.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly AppDbContext _db;

    public ChatHub(AppDbContext db)
    {
        _db = db;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }
        await base.OnConnectedAsync();
    }

    public async Task SendMessage(string toUserId, string messageText)
    {
        var fromUserId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (fromUserId == null) return;

        var fromUser = await _db.Users.FindAsync(fromUserId);
        if (fromUser == null) return;

        var message = new Message
        {
            FromUserId = fromUserId,
            ToUserId = toUserId,
            Text = messageText,
            SentAt = DateTime.UtcNow
        };

        _db.Messages.Add(message);
        await _db.SaveChangesAsync();

        var messageDto = new MessageDto(
            message.Id,
            message.FromUserId,
            message.ToUserId,
            new UserSummaryDto(fromUser.Id, fromUser.DisplayName, fromUser.AvatarUrl),
            message.Text,
            message.Delivered,
            message.Seen,
            message.SentAt,
            message.SeenAt
        );

        // Send to recipient
        await Clients.User(toUserId).SendAsync("ReceiveMessage", messageDto);

        // Send confirmation to sender
        await Clients.Caller.SendAsync("MessageSent", messageDto);

        // Mark as delivered
        message.Delivered = true;
        await _db.SaveChangesAsync();
    }

    public async Task MarkAsRead(Guid messageId)
    {
        var message = await _db.Messages.FindAsync(messageId);
        if (message == null) return;

        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (message.ToUserId != userId) return;

        message.Seen = true;
        message.SeenAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        // Notify sender
        await Clients.User(message.FromUserId).SendAsync("MessageRead", messageId, message.SeenAt);
    }

    public async Task Typing(string toUserId)
    {
        var fromUserId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (fromUserId != null)
        {
            await Clients.User(toUserId).SendAsync("UserTyping", fromUserId);
        }
    }
}

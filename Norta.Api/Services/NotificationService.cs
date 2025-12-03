using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Norta.Api.Data;
using Norta.Api.DTOs;
using Norta.Api.Hubs;
using Norta.Api.Models;

namespace Norta.Api.Services;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _db;
    private readonly IHubContext<NotificationsHub> _hubContext;

    public NotificationService(AppDbContext db, IHubContext<NotificationsHub> hubContext)
    {
        _db = db;
        _hubContext = hubContext;
    }

    public async Task CreateNotificationAsync(string userId, NotificationType type, string actorId, Guid? postId = null)
    {
        var actor = await _db.Users.FindAsync(actorId);
        if (actor == null) return;

        var message = type switch
        {
            NotificationType.Like => $"{actor.DisplayName} liked your post",
            NotificationType.Comment => $"{actor.DisplayName} commented on your post",
            NotificationType.Follow => $"{actor.DisplayName} started following you",
            NotificationType.Message => $"{actor.DisplayName} sent you a message",
            _ => "New notification"
        };

        var notification = new Notification
        {
            UserId = userId,
            Type = type,
            ActorId = actorId,
            PostId = postId,
            Message = message
        };

        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync();

        // Send real-time notification via SignalR
        var notificationDto = new NotificationDto(
            notification.Id,
            notification.Type,
            new UserSummaryDto(actor.Id, actor.DisplayName, actor.AvatarUrl),
            notification.PostId,
            notification.Message,
            notification.IsRead,
            notification.CreatedAt
        );

        await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", notificationDto);
    }

    public async Task SendPushNotificationAsync(string userId, string title, string message)
    {
        // TODO: Implement APNs/FCM push notification logic
        // This would require device tokens stored per user and proper APNs/FCM setup
        // For now, this is a placeholder
        await Task.CompletedTask;
    }
}

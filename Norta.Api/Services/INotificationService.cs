using Norta.Api.Models;

namespace Norta.Api.Services;

public interface INotificationService
{
    Task CreateNotificationAsync(string userId, NotificationType type, string actorId, Guid? postId = null);
    Task SendPushNotificationAsync(string userId, string title, string message);
}

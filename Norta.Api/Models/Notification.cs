namespace Norta.Api.Models;

public class Notification
{
    public Guid Id { get; set; }

    public string UserId { get; set; } = null!;
    public AppUser? User { get; set; }

    public NotificationType Type { get; set; }
    public string? ActorId { get; set; } // User who triggered the notification
    public AppUser? Actor { get; set; }

    public Guid? PostId { get; set; }
    public Post? Post { get; set; }

    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum NotificationType
{
    Like,
    Comment,
    Follow,
    Message
}

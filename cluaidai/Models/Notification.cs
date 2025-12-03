namespace cluaidai.Models;

public class Notification
{
    public Guid Id { get; set; }
    public NotificationType Type { get; set; }
    public UserSummary? Actor { get; set; }
    public Guid? PostId { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum NotificationType
{
    Like,
    Comment,
    Follow,
    Message
}

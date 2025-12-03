namespace Norta.Api.Models;

public class Message
{
    public Guid Id { get; set; }

    public string FromUserId { get; set; } = null!;
    public AppUser? FromUser { get; set; }

    public string ToUserId { get; set; } = null!;
    public AppUser? ToUser { get; set; }

    public string Text { get; set; } = string.Empty;
    public bool Delivered { get; set; } = false;
    public bool Seen { get; set; } = false;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public DateTime? SeenAt { get; set; }
}

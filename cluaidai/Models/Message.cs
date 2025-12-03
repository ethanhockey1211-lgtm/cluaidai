namespace cluaidai.Models;

public class Message
{
    public Guid Id { get; set; }
    public string FromUserId { get; set; } = string.Empty;
    public string ToUserId { get; set; } = string.Empty;
    public UserSummary FromUser { get; set; } = new();
    public string Text { get; set; } = string.Empty;
    public bool Delivered { get; set; }
    public bool Seen { get; set; }
    public DateTime SentAt { get; set; }
    public DateTime? SeenAt { get; set; }
}

public class Conversation
{
    public UserSummary OtherUser { get; set; } = new();
    public Message? LastMessage { get; set; }
    public int UnreadCount { get; set; }
}

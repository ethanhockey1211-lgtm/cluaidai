namespace cluaidai.Models;

public class Post
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public UserSummary User { get; set; } = new();
    public string Caption { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int LikesCount { get; set; }
    public int CommentsCount { get; set; }
    public bool IsLiked { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Comment
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public UserSummary User { get; set; } = new();
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

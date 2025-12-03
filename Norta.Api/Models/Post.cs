namespace Norta.Api.Models;

public class Post
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = null!;
    public AppUser? User { get; set; }

    public string Caption { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Like> Likes { get; set; } = new List<Like>();
}

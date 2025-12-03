namespace Norta.Api.Models;

public class Like
{
    public string UserId { get; set; } = null!;
    public AppUser? User { get; set; }

    public Guid PostId { get; set; }
    public Post? Post { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

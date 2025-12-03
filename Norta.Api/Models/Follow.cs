namespace Norta.Api.Models;

public class Follow
{
    public string FollowerId { get; set; } = null!;
    public AppUser? Follower { get; set; }

    public string FollowingId { get; set; } = null!;
    public AppUser? Following { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Norta.Api.Data;
using Norta.Api.DTOs;
using Norta.Api.Models;
using System.Security.Claims;

namespace Norta.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly UserManager<AppUser> _userManager;

    public UsersController(AppDbContext db, UserManager<AppUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    private string GetCurrentUserId() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var userId = GetCurrentUserId();
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        var followersCount = await _db.Follows.CountAsync(f => f.FollowingId == userId);
        var followingCount = await _db.Follows.CountAsync(f => f.FollowerId == userId);
        var postsCount = await _db.Posts.CountAsync(p => p.UserId == userId);

        return Ok(new UserDto(
            user.Id,
            user.Email!,
            user.DisplayName,
            user.Bio,
            user.AvatarUrl,
            followersCount,
            followingCount,
            postsCount,
            false
        ));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(string id)
    {
        var currentUserId = GetCurrentUserId();
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var followersCount = await _db.Follows.CountAsync(f => f.FollowingId == id);
        var followingCount = await _db.Follows.CountAsync(f => f.FollowerId == id);
        var postsCount = await _db.Posts.CountAsync(p => p.UserId == id);
        var isFollowing = await _db.Follows.AnyAsync(f => f.FollowerId == currentUserId && f.FollowingId == id);

        return Ok(new UserDto(
            user.Id,
            user.Email!,
            user.DisplayName,
            user.Bio,
            user.AvatarUrl,
            followersCount,
            followingCount,
            postsCount,
            isFollowing
        ));
    }

    [HttpPut("me")]
    public async Task<ActionResult<UserDto>> UpdateProfile(UpdateProfileRequest request)
    {
        var userId = GetCurrentUserId();
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(request.DisplayName))
            user.DisplayName = request.DisplayName;

        if (request.Bio != null)
            user.Bio = request.Bio;

        if (request.AvatarUrl != null)
            user.AvatarUrl = request.AvatarUrl;

        await _userManager.UpdateAsync(user);

        return await GetCurrentUser();
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<UserSearchDto>>> SearchUsers([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return Ok(new List<UserSearchDto>());

        var currentUserId = GetCurrentUserId();
        var users = await _db.Users
            .Where(u => u.DisplayName.Contains(q) || u.Email!.Contains(q))
            .Take(20)
            .ToListAsync();

        var followingIds = await _db.Follows
            .Where(f => f.FollowerId == currentUserId)
            .Select(f => f.FollowingId)
            .ToListAsync();

        return Ok(users.Select(u => new UserSearchDto(
            u.Id,
            u.DisplayName,
            u.AvatarUrl,
            followingIds.Contains(u.Id)
        )).ToList());
    }

    [HttpPost("{id}/follow")]
    public async Task<IActionResult> FollowUser(string id)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == id)
            return BadRequest(new { message = "Cannot follow yourself" });

        var existingFollow = await _db.Follows
            .FirstOrDefaultAsync(f => f.FollowerId == currentUserId && f.FollowingId == id);

        if (existingFollow != null)
            return BadRequest(new { message = "Already following" });

        _db.Follows.Add(new Follow
        {
            FollowerId = currentUserId,
            FollowingId = id
        });

        await _db.SaveChangesAsync();

        // Create notification
        _db.Notifications.Add(new Notification
        {
            UserId = id,
            Type = NotificationType.Follow,
            ActorId = currentUserId,
            Message = "started following you"
        });
        await _db.SaveChangesAsync();

        return Ok();
    }

    [HttpDelete("{id}/follow")]
    public async Task<IActionResult> UnfollowUser(string id)
    {
        var currentUserId = GetCurrentUserId();
        var follow = await _db.Follows
            .FirstOrDefaultAsync(f => f.FollowerId == currentUserId && f.FollowingId == id);

        if (follow == null)
            return NotFound();

        _db.Follows.Remove(follow);
        await _db.SaveChangesAsync();

        return Ok();
    }

    [HttpGet("{id}/followers")]
    public async Task<ActionResult<List<UserSearchDto>>> GetFollowers(string id)
    {
        var currentUserId = GetCurrentUserId();
        var followers = await _db.Follows
            .Where(f => f.FollowingId == id)
            .Include(f => f.Follower)
            .Select(f => f.Follower!)
            .ToListAsync();

        var followingIds = await _db.Follows
            .Where(f => f.FollowerId == currentUserId)
            .Select(f => f.FollowingId)
            .ToListAsync();

        return Ok(followers.Select(u => new UserSearchDto(
            u.Id,
            u.DisplayName,
            u.AvatarUrl,
            followingIds.Contains(u.Id)
        )).ToList());
    }

    [HttpGet("{id}/following")]
    public async Task<ActionResult<List<UserSearchDto>>> GetFollowing(string id)
    {
        var currentUserId = GetCurrentUserId();
        var following = await _db.Follows
            .Where(f => f.FollowerId == id)
            .Include(f => f.Following)
            .Select(f => f.Following!)
            .ToListAsync();

        var followingIds = await _db.Follows
            .Where(f => f.FollowerId == currentUserId)
            .Select(f => f.FollowingId)
            .ToListAsync();

        return Ok(following.Select(u => new UserSearchDto(
            u.Id,
            u.DisplayName,
            u.AvatarUrl,
            followingIds.Contains(u.Id)
        )).ToList());
    }
}

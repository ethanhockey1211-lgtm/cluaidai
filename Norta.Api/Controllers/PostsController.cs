using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Norta.Api.Data;
using Norta.Api.DTOs;
using Norta.Api.Models;
using Norta.Api.Services;
using System.Security.Claims;

namespace Norta.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PostsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notificationService;

    public PostsController(AppDbContext db, INotificationService notificationService)
    {
        _db = db;
        _notificationService = notificationService;
    }

    private string GetCurrentUserId() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

    [HttpGet("feed")]
    public async Task<ActionResult<List<PostDto>>> GetFeed([FromQuery] int page = 0, [FromQuery] int size = 10)
    {
        var userId = GetCurrentUserId();

        // Get posts from followed users + own posts
        var followingIds = await _db.Follows
            .Where(f => f.FollowerId == userId)
            .Select(f => f.FollowingId)
            .ToListAsync();

        followingIds.Add(userId); // Include own posts

        var posts = await _db.Posts
            .Where(p => followingIds.Contains(p.UserId))
            .Include(p => p.User)
            .Include(p => p.Likes)
            .Include(p => p.Comments)
            .OrderByDescending(p => p.CreatedAt)
            .Skip(page * size)
            .Take(size)
            .ToListAsync();

        var postDtos = posts.Select(p => new PostDto(
            p.Id,
            p.UserId,
            new UserSummaryDto(p.User!.Id, p.User.DisplayName, p.User.AvatarUrl),
            p.Caption,
            p.ImageUrl,
            p.Likes.Count,
            p.Comments.Count,
            p.Likes.Any(l => l.UserId == userId),
            p.CreatedAt
        )).ToList();

        return Ok(postDtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PostDto>> GetPost(Guid id)
    {
        var userId = GetCurrentUserId();
        var post = await _db.Posts
            .Include(p => p.User)
            .Include(p => p.Likes)
            .Include(p => p.Comments)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null) return NotFound();

        return Ok(new PostDto(
            post.Id,
            post.UserId,
            new UserSummaryDto(post.User!.Id, post.User.DisplayName, post.User.AvatarUrl),
            post.Caption,
            post.ImageUrl,
            post.Likes.Count,
            post.Comments.Count,
            post.Likes.Any(l => l.UserId == userId),
            post.CreatedAt
        ));
    }

    [HttpPost]
    public async Task<ActionResult<PostDto>> CreatePost(CreatePostRequest request)
    {
        var userId = GetCurrentUserId();
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return Unauthorized();

        var post = new Post
        {
            UserId = userId,
            Caption = request.Caption,
            ImageUrl = request.ImageUrl
        };

        _db.Posts.Add(post);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPost), new { id = post.Id }, new PostDto(
            post.Id,
            post.UserId,
            new UserSummaryDto(user.Id, user.DisplayName, user.AvatarUrl),
            post.Caption,
            post.ImageUrl,
            0,
            0,
            false,
            post.CreatedAt
        ));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePost(Guid id)
    {
        var userId = GetCurrentUserId();
        var post = await _db.Posts.FindAsync(id);

        if (post == null) return NotFound();
        if (post.UserId != userId) return Forbid();

        _db.Posts.Remove(post);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id}/like")]
    public async Task<IActionResult> LikePost(Guid id)
    {
        var userId = GetCurrentUserId();
        var post = await _db.Posts.FindAsync(id);
        if (post == null) return NotFound();

        var existingLike = await _db.Likes
            .FirstOrDefaultAsync(l => l.PostId == id && l.UserId == userId);

        if (existingLike != null)
            return BadRequest(new { message = "Already liked" });

        _db.Likes.Add(new Like
        {
            PostId = id,
            UserId = userId
        });

        await _db.SaveChangesAsync();

        // Create notification (don't notify if liking own post)
        if (post.UserId != userId)
        {
            await _notificationService.CreateNotificationAsync(
                post.UserId,
                NotificationType.Like,
                userId,
                id
            );
        }

        return Ok();
    }

    [HttpDelete("{id}/like")]
    public async Task<IActionResult> UnlikePost(Guid id)
    {
        var userId = GetCurrentUserId();
        var like = await _db.Likes
            .FirstOrDefaultAsync(l => l.PostId == id && l.UserId == userId);

        if (like == null) return NotFound();

        _db.Likes.Remove(like);
        await _db.SaveChangesAsync();

        return Ok();
    }

    [HttpGet("{id}/comments")]
    public async Task<ActionResult<List<CommentDto>>> GetComments(Guid id)
    {
        var comments = await _db.Comments
            .Where(c => c.PostId == id)
            .Include(c => c.User)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return Ok(comments.Select(c => new CommentDto(
            c.Id,
            c.PostId,
            new UserSummaryDto(c.User!.Id, c.User.DisplayName, c.User.AvatarUrl),
            c.Text,
            c.CreatedAt
        )).ToList());
    }

    [HttpPost("{id}/comments")]
    public async Task<ActionResult<CommentDto>> CreateComment(Guid id, CreateCommentRequest request)
    {
        var userId = GetCurrentUserId();
        var post = await _db.Posts.FindAsync(id);
        if (post == null) return NotFound();

        var user = await _db.Users.FindAsync(userId);
        if (user == null) return Unauthorized();

        var comment = new Comment
        {
            PostId = id,
            UserId = userId,
            Text = request.Text
        };

        _db.Comments.Add(comment);
        await _db.SaveChangesAsync();

        // Create notification (don't notify if commenting on own post)
        if (post.UserId != userId)
        {
            await _notificationService.CreateNotificationAsync(
                post.UserId,
                NotificationType.Comment,
                userId,
                id
            );
        }

        return CreatedAtAction(nameof(GetComments), new { id = post.Id }, new CommentDto(
            comment.Id,
            comment.PostId,
            new UserSummaryDto(user.Id, user.DisplayName, user.AvatarUrl),
            comment.Text,
            comment.CreatedAt
        ));
    }

    [HttpDelete("comments/{commentId}")]
    public async Task<IActionResult> DeleteComment(Guid commentId)
    {
        var userId = GetCurrentUserId();
        var comment = await _db.Comments
            .Include(c => c.Post)
            .FirstOrDefaultAsync(c => c.Id == commentId);

        if (comment == null) return NotFound();

        // Can delete if owner of comment or owner of post
        if (comment.UserId != userId && comment.Post?.UserId != userId)
            return Forbid();

        _db.Comments.Remove(comment);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<List<PostDto>>> GetUserPosts(string userId, [FromQuery] int page = 0, [FromQuery] int size = 10)
    {
        var currentUserId = GetCurrentUserId();
        var posts = await _db.Posts
            .Where(p => p.UserId == userId)
            .Include(p => p.User)
            .Include(p => p.Likes)
            .Include(p => p.Comments)
            .OrderByDescending(p => p.CreatedAt)
            .Skip(page * size)
            .Take(size)
            .ToListAsync();

        return Ok(posts.Select(p => new PostDto(
            p.Id,
            p.UserId,
            new UserSummaryDto(p.User!.Id, p.User.DisplayName, p.User.AvatarUrl),
            p.Caption,
            p.ImageUrl,
            p.Likes.Count,
            p.Comments.Count,
            p.Likes.Any(l => l.UserId == currentUserId),
            p.CreatedAt
        )).ToList());
    }
}

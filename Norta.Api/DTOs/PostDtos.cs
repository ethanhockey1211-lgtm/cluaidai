using System.ComponentModel.DataAnnotations;

namespace Norta.Api.DTOs;

public record CreatePostRequest(
    [MaxLength(1000)] string Caption,
    string? ImageUrl
);

public record PostDto(
    Guid Id,
    string UserId,
    UserSummaryDto User,
    string Caption,
    string? ImageUrl,
    int LikesCount,
    int CommentsCount,
    bool IsLiked,
    DateTime CreatedAt
);

public record UserSummaryDto(
    string Id,
    string DisplayName,
    string? AvatarUrl
);

public record CommentDto(
    Guid Id,
    Guid PostId,
    UserSummaryDto User,
    string Text,
    DateTime CreatedAt
);

public record CreateCommentRequest(
    [Required, MaxLength(500)] string Text
);

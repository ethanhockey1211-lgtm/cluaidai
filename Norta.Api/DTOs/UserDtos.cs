using System.ComponentModel.DataAnnotations;

namespace Norta.Api.DTOs;

public record UserDto(
    string Id,
    string Email,
    string DisplayName,
    string Bio,
    string? AvatarUrl,
    int FollowersCount,
    int FollowingCount,
    int PostsCount,
    bool IsFollowing
);

public record UpdateProfileRequest(
    [MinLength(2)] string? DisplayName,
    [MaxLength(500)] string? Bio,
    string? AvatarUrl
);

public record UserSearchDto(
    string Id,
    string DisplayName,
    string? AvatarUrl,
    bool IsFollowing
);

using System.ComponentModel.DataAnnotations;

namespace Norta.Api.DTOs;

public record MessageDto(
    Guid Id,
    string FromUserId,
    string ToUserId,
    UserSummaryDto FromUser,
    string Text,
    bool Delivered,
    bool Seen,
    DateTime SentAt,
    DateTime? SeenAt
);

public record SendMessageRequest(
    [Required] string ToUserId,
    [Required, MaxLength(1000)] string Text
);

public record ConversationDto(
    UserSummaryDto OtherUser,
    MessageDto? LastMessage,
    int UnreadCount
);

using Norta.Api.Models;

namespace Norta.Api.DTOs;

public record NotificationDto(
    Guid Id,
    NotificationType Type,
    UserSummaryDto? Actor,
    Guid? PostId,
    string Message,
    bool IsRead,
    DateTime CreatedAt
);

using Sqordia.Domain.Enums;

namespace Sqordia.Application.Common.Models;

public record CreateNotificationCommand(
    Guid UserId,
    NotificationType Type,
    NotificationCategory Category,
    string TitleFr,
    string TitleEn,
    string MessageFr,
    string MessageEn,
    string? ActionUrl = null,
    string? MetadataJson = null,
    Guid? RelatedEntityId = null,
    NotificationPriority Priority = NotificationPriority.Normal,
    string? GroupKey = null);

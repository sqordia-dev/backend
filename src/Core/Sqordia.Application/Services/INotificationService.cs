using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Responses.Notification;
using Sqordia.Domain.Enums;

namespace Sqordia.Application.Services;

public interface INotificationService
{
    Task<Result<NotificationListResponse>> GetNotificationsAsync(
        int pageNumber = 1,
        int pageSize = 20,
        bool? isRead = null,
        string? category = null,
        CancellationToken cancellationToken = default);

    Task<Result<UnreadCountResponse>> GetUnreadCountAsync(
        CancellationToken cancellationToken = default);

    Task<Result<NotificationResponse>> GetByIdAsync(
        Guid notificationId,
        CancellationToken cancellationToken = default);

    Task<Result> MarkAsReadAsync(
        Guid notificationId,
        CancellationToken cancellationToken = default);

    Task<Result> MarkAllAsReadAsync(
        CancellationToken cancellationToken = default);

    Task<Result> DeleteNotificationAsync(
        Guid notificationId,
        CancellationToken cancellationToken = default);

    Task<Result<NotificationResponse>> CreateNotificationAsync(
        Guid userId,
        NotificationType type,
        NotificationCategory category,
        string titleFr,
        string titleEn,
        string messageFr,
        string messageEn,
        string? actionUrl = null,
        string? metadataJson = null,
        Guid? relatedEntityId = null,
        NotificationPriority priority = NotificationPriority.Normal,
        string? groupKey = null,
        CancellationToken cancellationToken = default);

    Task<Result> CreateBulkNotificationsAsync(
        IEnumerable<Guid> userIds,
        NotificationType type,
        NotificationCategory category,
        string titleFr,
        string titleEn,
        string messageFr,
        string messageEn,
        string? actionUrl = null,
        string? metadataJson = null,
        Guid? relatedEntityId = null,
        NotificationPriority priority = NotificationPriority.Normal,
        string? groupKey = null,
        CancellationToken cancellationToken = default);

    Task<Result> CreateSystemAnnouncementAsync(
        string titleFr,
        string titleEn,
        string messageFr,
        string messageEn,
        NotificationPriority priority = NotificationPriority.Normal,
        string? actionUrl = null,
        CancellationToken cancellationToken = default);
}

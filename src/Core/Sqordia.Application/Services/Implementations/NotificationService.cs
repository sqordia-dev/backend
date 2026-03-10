using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Responses.Notification;
using Sqordia.Domain.Entities;
using Sqordia.Domain.Enums;

namespace Sqordia.Application.Services.Implementations;

public class NotificationService : INotificationService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<NotificationService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<NotificationListResponse>> GetNotificationsAsync(
        int pageNumber = 1,
        int pageSize = 20,
        bool? isRead = null,
        string? category = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _currentUserService.GetUserIdAsGuid();
            if (userId == null)
                return Result.Failure<NotificationListResponse>(
                    Error.Unauthorized("Notification.Unauthorized", "Authentication required"));

            var query = _context.Notifications
                .Where(n => n.UserId == userId.Value)
                .AsNoTracking();

            if (isRead.HasValue)
                query = query.Where(n => n.IsRead == isRead.Value);

            if (!string.IsNullOrEmpty(category) &&
                Enum.TryParse<NotificationCategory>(category, true, out var parsedCategory))
                query = query.Where(n => n.Category == parsedCategory);

            var totalCount = await query.CountAsync(cancellationToken);
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var notifications = await query
                .OrderByDescending(n => n.Created)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var response = new NotificationListResponse
            {
                Items = notifications.Select(MapToResponse).ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                TotalPages = totalPages,
                HasNextPage = pageNumber < totalPages
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notifications");
            return Result.Failure<NotificationListResponse>(
                Error.Failure("Notification.Error.GetFailed", "Failed to retrieve notifications"));
        }
    }

    public async Task<Result<UnreadCountResponse>> GetUnreadCountAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _currentUserService.GetUserIdAsGuid();
            if (userId == null)
                return Result.Failure<UnreadCountResponse>(
                    Error.Unauthorized("Notification.Unauthorized", "Authentication required"));

            var count = await _context.Notifications
                .Where(n => n.UserId == userId.Value && !n.IsRead)
                .CountAsync(cancellationToken);

            return Result.Success(new UnreadCountResponse { Count = count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread notification count");
            return Result.Failure<UnreadCountResponse>(
                Error.Failure("Notification.Error.CountFailed", "Failed to retrieve unread count"));
        }
    }

    public async Task<Result<NotificationResponse>> GetByIdAsync(
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _currentUserService.GetUserIdAsGuid();
            if (userId == null)
                return Result.Failure<NotificationResponse>(
                    Error.Unauthorized("Notification.Unauthorized", "Authentication required"));

            var notification = await _context.Notifications
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.Id == notificationId, cancellationToken);

            if (notification == null)
                return Result.Failure<NotificationResponse>(
                    Error.NotFound("Notification.NotFound", "Notification not found"));

            if (notification.UserId != userId.Value)
                return Result.Failure<NotificationResponse>(
                    Error.Forbidden("Notification.Forbidden", "You do not have access to this notification"));

            return Result.Success(MapToResponse(notification));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notification {NotificationId}", notificationId);
            return Result.Failure<NotificationResponse>(
                Error.Failure("Notification.Error.GetFailed", "Failed to retrieve notification"));
        }
    }

    public async Task<Result> MarkAsReadAsync(
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _currentUserService.GetUserIdAsGuid();
            if (userId == null)
                return Result.Failure(Error.Unauthorized("Notification.Unauthorized", "Authentication required"));

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId, cancellationToken);

            if (notification == null)
                return Result.Failure(Error.NotFound("Notification.NotFound", "Notification not found"));

            if (notification.UserId != userId.Value)
                return Result.Failure(Error.Forbidden("Notification.Forbidden", "You do not have access to this notification"));

            notification.MarkAsRead();
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification {NotificationId} as read", notificationId);
            return Result.Failure(Error.Failure("Notification.Error.MarkReadFailed", "Failed to mark notification as read"));
        }
    }

    public async Task<Result> MarkAllAsReadAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _currentUserService.GetUserIdAsGuid();
            if (userId == null)
                return Result.Failure(Error.Unauthorized("Notification.Unauthorized", "Authentication required"));

            await _context.Notifications
                .Where(n => n.UserId == userId.Value && !n.IsRead)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(n => n.IsRead, true)
                        .SetProperty(n => n.ReadAt, DateTime.UtcNow),
                    cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read");
            return Result.Failure(Error.Failure("Notification.Error.MarkAllReadFailed", "Failed to mark all notifications as read"));
        }
    }

    public async Task<Result> DeleteNotificationAsync(
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _currentUserService.GetUserIdAsGuid();
            if (userId == null)
                return Result.Failure(Error.Unauthorized("Notification.Unauthorized", "Authentication required"));

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId, cancellationToken);

            if (notification == null)
                return Result.Failure(Error.NotFound("Notification.NotFound", "Notification not found"));

            if (notification.UserId != userId.Value)
                return Result.Failure(Error.Forbidden("Notification.Forbidden", "You do not have access to this notification"));

            notification.SoftDelete();
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting notification {NotificationId}", notificationId);
            return Result.Failure(Error.Failure("Notification.Error.DeleteFailed", "Failed to delete notification"));
        }
    }

    public async Task<Result<NotificationResponse>> CreateNotificationAsync(
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
        CancellationToken cancellationToken = default)
    {
        try
        {
            var notification = new Notification(
                userId, type, category,
                titleFr, titleEn,
                messageFr, messageEn,
                actionUrl, metadataJson, relatedEntityId);

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Notification created: {Type} for user {UserId}",
                type, userId);

            return Result.Success(MapToResponse(notification));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating notification for user {UserId}", userId);
            return Result.Failure<NotificationResponse>(
                Error.Failure("Notification.Error.CreateFailed", "Failed to create notification"));
        }
    }

    public async Task<Result> CreateBulkNotificationsAsync(
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
        CancellationToken cancellationToken = default)
    {
        try
        {
            var notifications = userIds.Select(uid => new Notification(
                uid, type, category,
                titleFr, titleEn,
                messageFr, messageEn,
                actionUrl, metadataJson, relatedEntityId));

            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Bulk notifications created: {Type} for {Count} users",
                type, userIds.Count());

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating bulk notifications");
            return Result.Failure(
                Error.Failure("Notification.Error.BulkCreateFailed", "Failed to create bulk notifications"));
        }
    }

    private static NotificationResponse MapToResponse(Notification notification)
    {
        return new NotificationResponse
        {
            Id = notification.Id,
            Type = notification.Type.ToString(),
            Category = notification.Category.ToString(),
            TitleFr = notification.TitleFr,
            TitleEn = notification.TitleEn,
            MessageFr = notification.MessageFr,
            MessageEn = notification.MessageEn,
            IsRead = notification.IsRead,
            ReadAt = notification.ReadAt,
            ActionUrl = notification.ActionUrl,
            MetadataJson = notification.MetadataJson,
            RelatedEntityId = notification.RelatedEntityId,
            CreatedAt = notification.Created
        };
    }
}

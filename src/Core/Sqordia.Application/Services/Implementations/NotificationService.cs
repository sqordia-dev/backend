using System.Net;
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
    private readonly INotificationHubService _hubService;
    private readonly INotificationPreferenceService _preferenceService;
    private readonly IEmailService _emailService;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        INotificationHubService hubService,
        INotificationPreferenceService preferenceService,
        IEmailService emailService,
        ILogger<NotificationService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _hubService = hubService;
        _preferenceService = preferenceService;
        _emailService = emailService;
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
                .OrderByDescending(n => n.Priority)
                .ThenByDescending(n => n.Created)
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

            // Push updated count via SignalR
            await PushUnreadCountAsync(userId.Value, cancellationToken);

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

            // Push zero count via SignalR
            _ = _hubService.SendUnreadCountAsync(userId.Value, 0, cancellationToken);

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

            var wasUnread = !notification.IsRead;
            notification.SoftDelete();
            await _context.SaveChangesAsync(cancellationToken);

            if (wasUnread)
            {
                await PushUnreadCountAsync(userId.Value, cancellationToken);
            }

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
        NotificationPriority priority = NotificationPriority.Normal,
        string? groupKey = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check user preferences for in-app
            var shouldSendInApp = await _preferenceService.ShouldSendInAppAsync(userId, type, cancellationToken);
            if (!shouldSendInApp)
            {
                _logger.LogDebug("In-app notification skipped for user {UserId}, type {Type} (disabled by preference)", userId, type);
                // Still check email even if in-app is disabled
                _ = TrySendEmailNotificationAsync(userId, type, titleFr, titleEn, messageFr, messageEn, cancellationToken);
                return Result.Success(new NotificationResponse()); // Return empty response
            }

            // Smart batching: check for recent similar notifications
            if (!string.IsNullOrEmpty(groupKey))
            {
                var batchWindow = DateTime.UtcNow.AddMinutes(-2);
                var recentSimilar = await _context.Notifications
                    .Where(n => n.UserId == userId && n.GroupKey == groupKey && n.Created >= batchWindow && !n.IsRead)
                    .OrderByDescending(n => n.Created)
                    .FirstOrDefaultAsync(cancellationToken);

                if (recentSimilar != null)
                {
                    _logger.LogDebug("Notification batched with group key {GroupKey} for user {UserId}", groupKey, userId);
                    return Result.Success(MapToResponse(recentSimilar));
                }
            }

            var notification = new Notification(
                userId, type, category,
                titleFr, titleEn,
                messageFr, messageEn,
                actionUrl, metadataJson, relatedEntityId,
                priority, groupKey);

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync(cancellationToken);

            var response = MapToResponse(notification);

            _logger.LogInformation(
                "Notification created: {Type} ({Priority}) for user {UserId}",
                type, priority, userId);

            // Push via SignalR (fire-and-forget, use CancellationToken.None — request token may cancel)
            var unreadCount = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .CountAsync(cancellationToken);

            _ = Task.Run(async () =>
            {
                try
                {
                    await _hubService.SendNotificationAsync(userId, response, CancellationToken.None);
                    await _hubService.SendUnreadCountAsync(userId, unreadCount, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to push notification via SignalR for user {UserId}", userId);
                }
            }, CancellationToken.None);

            // Send email notification if applicable (fire-and-forget)
            _ = TrySendEmailNotificationAsync(userId, type, titleFr, titleEn, messageFr, messageEn, CancellationToken.None);

            return Result.Success(response);
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
        NotificationPriority priority = NotificationPriority.Normal,
        string? groupKey = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userIdList = userIds.ToList();

            // Batch preference check — single query instead of N+1
            var disabledUsers = await _preferenceService.GetUsersWithInAppDisabledAsync(
                userIdList, type, cancellationToken);
            var filteredUserIds = userIdList.Where(uid => !disabledUsers.Contains(uid)).ToList();

            if (filteredUserIds.Count == 0)
            {
                _logger.LogDebug("Bulk notification skipped: all {Count} users disabled type {Type}", userIdList.Count, type);
                return Result.Success();
            }

            var notifications = filteredUserIds.Select(uid => new Notification(
                uid, type, category,
                titleFr, titleEn,
                messageFr, messageEn,
                actionUrl, metadataJson, relatedEntityId,
                priority, groupKey));

            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Bulk notifications created: {Type} ({Priority}) for {Count}/{Total} users (after preference filter)",
                type, priority, filteredUserIds.Count, userIdList.Count);

            // Push via SignalR for each user (fire-and-forget)
            var response = new NotificationResponse
            {
                Type = type.ToString(),
                Category = category.ToString(),
                Priority = priority.ToString(),
                TitleFr = titleFr,
                TitleEn = titleEn,
                MessageFr = messageFr,
                MessageEn = messageEn,
                ActionUrl = actionUrl,
                GroupKey = groupKey,
                CreatedAt = DateTime.UtcNow
            };

            _ = Task.Run(async () =>
            {
                try
                {
                    await _hubService.SendBulkNotificationsAsync(filteredUserIds, response, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to push bulk notifications via SignalR");
                }
            }, CancellationToken.None);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating bulk notifications");
            return Result.Failure(
                Error.Failure("Notification.Error.BulkCreateFailed", "Failed to create bulk notifications"));
        }
    }

    public async Task<Result> CreateSystemAnnouncementAsync(
        string titleFr,
        string titleEn,
        string messageFr,
        string messageEn,
        NotificationPriority priority = NotificationPriority.Normal,
        string? actionUrl = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get all active user IDs
            var userIds = await _context.Users
                .Where(u => !u.IsDeleted)
                .Select(u => u.Id)
                .ToListAsync(cancellationToken);

            if (userIds.Count == 0)
                return Result.Success();

            return await CreateBulkNotificationsAsync(
                userIds,
                NotificationType.SystemAnnouncement,
                NotificationCategory.System,
                titleFr, titleEn,
                messageFr, messageEn,
                actionUrl,
                priority: priority,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating system announcement");
            return Result.Failure(
                Error.Failure("Notification.Error.AnnouncementFailed", "Failed to create system announcement"));
        }
    }

    private async Task PushUnreadCountAsync(Guid userId, CancellationToken ct)
    {
        try
        {
            var count = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .CountAsync(ct);

            await _hubService.SendUnreadCountAsync(userId, count, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to push unread count for user {UserId}", userId);
        }
    }

    private async Task TrySendEmailNotificationAsync(
        Guid userId, NotificationType type,
        string titleFr, string titleEn,
        string messageFr, string messageEn,
        CancellationToken ct)
    {
        try
        {
            var shouldEmail = await _preferenceService.ShouldSendEmailAsync(userId, type, ct);
            if (!shouldEmail) return;

            var email = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.Email)
                .FirstOrDefaultAsync(ct);

            if (email == null) return;

            var subject = $"[Sqordia] {titleEn}";
            var body = $"<h2>{WebUtility.HtmlEncode(titleEn)}</h2><p>{WebUtility.HtmlEncode(messageEn)}</p>" +
                        $"<hr/><h2>{WebUtility.HtmlEncode(titleFr)}</h2><p>{WebUtility.HtmlEncode(messageFr)}</p>";

            await _emailService.SendHtmlEmailAsync(email, subject, body);

            _logger.LogDebug("Email notification sent to {Email} for type {Type}", email, type);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send email notification to user {UserId}", userId);
        }
    }

    private static NotificationResponse MapToResponse(Notification notification)
    {
        return new NotificationResponse
        {
            Id = notification.Id,
            Type = notification.Type.ToString(),
            Category = notification.Category.ToString(),
            Priority = notification.Priority.ToString(),
            TitleFr = notification.TitleFr,
            TitleEn = notification.TitleEn,
            MessageFr = notification.MessageFr,
            MessageEn = notification.MessageEn,
            IsRead = notification.IsRead,
            ReadAt = notification.ReadAt,
            ActionUrl = notification.ActionUrl,
            MetadataJson = notification.MetadataJson,
            RelatedEntityId = notification.RelatedEntityId,
            GroupKey = notification.GroupKey,
            CreatedAt = notification.Created
        };
    }
}

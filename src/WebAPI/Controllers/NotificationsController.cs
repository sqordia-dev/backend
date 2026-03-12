using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services;
using Sqordia.Contracts.Requests.Notification;
using Sqordia.Contracts.Responses.Notification;
using Sqordia.Domain.Enums;

namespace WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/notifications")]
[Authorize]
public class NotificationsController : BaseApiController
{
    private readonly INotificationService _notificationService;
    private readonly INotificationPreferenceService _preferenceService;
    private readonly INotificationAnalyticsService _analyticsService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        INotificationService notificationService,
        INotificationPreferenceService preferenceService,
        INotificationAnalyticsService analyticsService,
        ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService;
        _preferenceService = preferenceService;
        _analyticsService = analyticsService;
        _logger = logger;
    }

    /// <summary>
    /// Get notifications for the current user with optional filtering
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(NotificationListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool? isRead = null,
        [FromQuery] string? category = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _notificationService.GetNotificationsAsync(
            pageNumber, pageSize, isRead, category, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get the unread notification count for the current user
    /// </summary>
    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(UnreadCountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken = default)
    {
        var result = await _notificationService.GetUnreadCountAsync(cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get a specific notification by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetNotification(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _notificationService.GetByIdAsync(id, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Mark a notification as read
    /// </summary>
    [HttpPatch("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _notificationService.MarkAsReadAsync(id, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Mark all notifications as read for the current user
    /// </summary>
    [HttpPatch("mark-all-read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken = default)
    {
        var result = await _notificationService.MarkAllAsReadAsync(cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Delete a notification (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteNotification(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _notificationService.DeleteNotificationAsync(id, cancellationToken);
        return HandleResult(result);
    }

    // ─── Preferences ───────────────────────────────────────────────

    /// <summary>
    /// Get notification preferences for the current user
    /// </summary>
    [HttpGet("preferences")]
    [ProducesResponseType(typeof(NotificationPreferencesListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPreferences(CancellationToken cancellationToken = default)
    {
        var result = await _preferenceService.GetPreferencesAsync(cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Update notification preferences for the current user (bulk)
    /// </summary>
    [HttpPut("preferences")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdatePreferences(
        [FromBody] UpdateNotificationPreferencesBulkRequest request,
        CancellationToken cancellationToken = default)
    {
        var preferences = request.Preferences
            .Where(p => Enum.TryParse<NotificationType>(p.NotificationType, true, out _))
            .Select(p => (
                Type: Enum.Parse<NotificationType>(p.NotificationType, true),
                p.InAppEnabled,
                p.EmailEnabled,
                EmailFrequency: Enum.TryParse<NotificationFrequency>(p.EmailFrequency, true, out var freq)
                    ? freq : NotificationFrequency.Instant,
                p.SoundEnabled
            ));

        var result = await _preferenceService.UpdatePreferencesBulkAsync(preferences, cancellationToken);
        return HandleResult(result);
    }

    // ─── Admin Endpoints ───────────────────────────────────────────

    /// <summary>
    /// Get notification analytics (admin only)
    /// </summary>
    [HttpGet("admin/analytics")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(NotificationAnalyticsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAnalytics(
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default)
    {
        var result = await _analyticsService.GetAnalyticsAsync(days, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a system-wide announcement (admin only)
    /// </summary>
    [HttpPost("admin/broadcast")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateBroadcast(
        [FromBody] CreateSystemAnnouncementRequest request,
        CancellationToken cancellationToken = default)
    {
        var priority = Enum.TryParse<NotificationPriority>(request.Priority, true, out var p)
            ? p : NotificationPriority.Normal;

        var result = await _notificationService.CreateSystemAnnouncementAsync(
            request.TitleFr, request.TitleEn,
            request.MessageFr, request.MessageEn,
            priority, request.ActionUrl,
            cancellationToken);

        return HandleResult(result);
    }
}

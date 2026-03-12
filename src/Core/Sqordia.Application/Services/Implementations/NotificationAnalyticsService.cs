using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Responses.Notification;

namespace Sqordia.Application.Services.Implementations;

public class NotificationAnalyticsService : INotificationAnalyticsService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<NotificationAnalyticsService> _logger;

    public NotificationAnalyticsService(
        IApplicationDbContext context,
        ILogger<NotificationAnalyticsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<NotificationAnalyticsResponse>> GetAnalyticsAsync(
        int days = 30,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var since = DateTime.UtcNow.AddDays(-days);

            var notifications = await _context.Notifications
                .IgnoreQueryFilters() // Include soft-deleted for accurate analytics
                .Where(n => n.Created >= since)
                .Select(n => new
                {
                    n.Type,
                    n.Priority,
                    n.IsRead,
                    n.Created,
                    n.ReadAt,
                    n.UserId
                })
                .ToListAsync(cancellationToken);

            var totalSent = notifications.Count;
            var totalRead = notifications.Count(n => n.IsRead);
            var readRate = totalSent > 0 ? Math.Round((double)totalRead / totalSent * 100, 1) : 0;

            var readNotifications = notifications.Where(n => n.IsRead && n.ReadAt.HasValue).ToList();
            var avgTimeToRead = readNotifications.Count > 0
                ? Math.Round(readNotifications.Average(n => (n.ReadAt!.Value - n.Created).TotalMinutes), 1)
                : 0;

            var activeUsers = notifications.Select(n => n.UserId).Distinct().Count();

            // Stats by type
            var byType = notifications
                .GroupBy(n => n.Type.ToString())
                .Select(g => new NotificationTypeStats
                {
                    Type = g.Key,
                    Sent = g.Count(),
                    Read = g.Count(n => n.IsRead),
                    ReadRate = g.Count() > 0
                        ? Math.Round((double)g.Count(n => n.IsRead) / g.Count() * 100, 1)
                        : 0
                })
                .OrderByDescending(s => s.Sent)
                .ToList();

            // Stats by priority
            var byPriority = notifications
                .GroupBy(n => n.Priority.ToString())
                .Select(g => new NotificationPriorityStats
                {
                    Priority = g.Key,
                    Sent = g.Count(),
                    Read = g.Count(n => n.IsRead),
                    ReadRate = g.Count() > 0
                        ? Math.Round((double)g.Count(n => n.IsRead) / g.Count() * 100, 1)
                        : 0
                })
                .OrderByDescending(s => s.Sent)
                .ToList();

            // Daily trend
            var dailyTrend = notifications
                .GroupBy(n => n.Created.Date)
                .Select(g => new NotificationDailyStats
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Sent = g.Count(),
                    Read = g.Count(n => n.IsRead)
                })
                .OrderBy(s => s.Date)
                .ToList();

            var response = new NotificationAnalyticsResponse
            {
                TotalSent = totalSent,
                TotalRead = totalRead,
                ReadRate = readRate,
                AverageTimeToReadMinutes = avgTimeToRead,
                ActiveUsersWithNotifications = activeUsers,
                ByType = byType,
                ByPriority = byPriority,
                DailyTrend = dailyTrend
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error computing notification analytics");
            return Result.Failure<NotificationAnalyticsResponse>(
                Error.Failure("Notification.Error.AnalyticsFailed", "Failed to compute notification analytics"));
        }
    }
}

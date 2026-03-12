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

            var baseQuery = _context.Notifications
                .IgnoreQueryFilters()
                .Where(n => n.Created >= since);

            // Aggregate totals in SQL
            var totals = await baseQuery
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    TotalSent = g.Count(),
                    TotalRead = g.Count(n => n.IsRead),
                    ActiveUsers = g.Select(n => n.UserId).Distinct().Count()
                })
                .FirstOrDefaultAsync(cancellationToken);

            var totalSent = totals?.TotalSent ?? 0;
            var totalRead = totals?.TotalRead ?? 0;
            var readRate = totalSent > 0 ? Math.Round((double)totalRead / totalSent * 100, 1) : 0;

            // Average time-to-read — project timestamps and compute in-memory
            // (EF/PostgreSQL doesn't support DateDiff directly in aggregate)
            var readTimestamps = await baseQuery
                .Where(n => n.IsRead && n.ReadAt != null)
                .Select(n => new { n.Created, ReadAt = n.ReadAt!.Value })
                .ToListAsync(cancellationToken);

            var avgTimeToRead = readTimestamps.Count > 0
                ? readTimestamps.Average(n => (n.ReadAt - n.Created).TotalMinutes)
                : 0;

            // Stats by type in SQL
            var byType = await baseQuery
                .GroupBy(n => n.Type)
                .Select(g => new NotificationTypeStats
                {
                    Type = g.Key.ToString(),
                    Sent = g.Count(),
                    Read = g.Count(n => n.IsRead),
                    ReadRate = g.Count() > 0
                        ? Math.Round((double)g.Count(n => n.IsRead) / g.Count() * 100, 1)
                        : 0
                })
                .OrderByDescending(s => s.Sent)
                .ToListAsync(cancellationToken);

            // Stats by priority in SQL
            var byPriority = await baseQuery
                .GroupBy(n => n.Priority)
                .Select(g => new NotificationPriorityStats
                {
                    Priority = g.Key.ToString(),
                    Sent = g.Count(),
                    Read = g.Count(n => n.IsRead),
                    ReadRate = g.Count() > 0
                        ? Math.Round((double)g.Count(n => n.IsRead) / g.Count() * 100, 1)
                        : 0
                })
                .OrderByDescending(s => s.Sent)
                .ToListAsync(cancellationToken);

            // Daily trend in SQL
            var dailyTrend = await baseQuery
                .GroupBy(n => n.Created.Date)
                .Select(g => new NotificationDailyStats
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Sent = g.Count(),
                    Read = g.Count(n => n.IsRead)
                })
                .OrderBy(s => s.Date)
                .ToListAsync(cancellationToken);

            var response = new NotificationAnalyticsResponse
            {
                TotalSent = totalSent,
                TotalRead = totalRead,
                ReadRate = readRate,
                AverageTimeToReadMinutes = Math.Round(avgTimeToRead, 1),
                ActiveUsersWithNotifications = totals?.ActiveUsers ?? 0,
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

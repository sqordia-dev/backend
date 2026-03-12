using System.Net;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Domain.Entities;
using Sqordia.Domain.Enums;

namespace WebAPI.BackgroundServices;

public class NotificationDigestService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NotificationDigestService> _logger;

    // Run at 8:00 AM UTC daily — check interval is 1 hour to catch the window
    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(1);
    private static readonly int DigestHourUtc = 8;

    public NotificationDigestService(
        IServiceScopeFactory scopeFactory,
        ILogger<NotificationDigestService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromMinutes(3), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow;

                // Daily digest: run once per day at DigestHourUtc (narrow window avoids re-runs)
                if (now.Hour == DigestHourUtc && now.Minute < 5)
                {
                    await SendDigestsAsync(NotificationFrequency.DailyDigest, TimeSpan.FromDays(1), stoppingToken);
                }

                // Weekly digest: run on Mondays at DigestHourUtc
                if (now.DayOfWeek == DayOfWeek.Monday && now.Hour == DigestHourUtc && now.Minute < 5)
                {
                    await SendDigestsAsync(NotificationFrequency.WeeklyDigest, TimeSpan.FromDays(7), stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing notification digests");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }

    private async Task SendDigestsAsync(NotificationFrequency frequency, TimeSpan lookbackWindow, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        var since = DateTime.UtcNow - lookbackWindow;

        // Find users who have at least one preference with this digest frequency
        var userPrefs = await context.NotificationPreferences
            .Where(p => p.EmailEnabled && p.EmailFrequency == frequency)
            .GroupBy(p => p.UserId)
            .Select(g => new { UserId = g.Key, Types = g.Select(p => p.NotificationType).ToList() })
            .ToListAsync(ct);

        _logger.LogInformation(
            "Processing {Frequency} digest for {UserCount} users",
            frequency, userPrefs.Count);

        // Batch load all user emails to avoid N+1
        var userIds = userPrefs.Select(p => p.UserId).ToList();
        var userEmails = await context.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id) && u.Email != null)
            .Select(u => new { u.Id, u.Email })
            .ToDictionaryAsync(u => u.Id, u => u.Email, ct);

        foreach (var userPref in userPrefs)
        {
            try
            {
                if (!userEmails.TryGetValue(userPref.UserId, out var email) || email == null)
                    continue;

                // Get unread notifications of those types within the lookback window
                var notifications = await context.Notifications
                    .Where(n => n.UserId == userPref.UserId
                        && !n.IsRead
                        && userPref.Types.Contains(n.Type)
                        && n.Created >= since)
                    .OrderByDescending(n => n.Created)
                    .Take(50)
                    .ToListAsync(ct);

                if (notifications.Count == 0) continue;

                var html = BuildDigestHtml(notifications, frequency);
                var frequencyLabel = frequency == NotificationFrequency.DailyDigest
                    ? "Daily" : "Weekly";

                await emailService.SendHtmlEmailAsync(
                    email,
                    $"[Sqordia] Your {frequencyLabel} Notification Digest",
                    html);

                _logger.LogDebug(
                    "Sent {Frequency} digest with {Count} notifications to {Email}",
                    frequency, notifications.Count, email);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send digest for user {UserId}", userPref.UserId);
            }
        }
    }

    private static string BuildDigestHtml(
        List<Notification> notifications,
        NotificationFrequency frequency)
    {
        var sb = new StringBuilder();
        var periodLabel = frequency == NotificationFrequency.DailyDigest
            ? "last 24 hours" : "last 7 days";

        sb.AppendLine($"""
            <div style="font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; max-width: 600px; margin: 0 auto;">
                <div style="background: linear-gradient(135deg, #3b82f6, #6366f1); padding: 24px; border-radius: 12px 12px 0 0;">
                    <h1 style="color: white; margin: 0; font-size: 20px;">Sqordia Notifications</h1>
                    <p style="color: rgba(255,255,255,0.8); margin: 4px 0 0; font-size: 14px;">
                        {notifications.Count} notification(s) from the {periodLabel}
                    </p>
                </div>
                <div style="border: 1px solid #e5e7eb; border-top: none; border-radius: 0 0 12px 12px; padding: 16px;">
            """);

        foreach (var n in notifications)
        {
            var priorityColor = n.Priority switch
            {
                NotificationPriority.Urgent => "#ef4444",
                NotificationPriority.High => "#f59e0b",
                _ => "#6b7280"
            };

            sb.AppendLine($"""
                    <div style="padding: 12px 0; border-bottom: 1px solid #f3f4f6;">
                        <div style="display: flex; align-items: center; gap: 8px;">
                            <span style="color: {priorityColor}; font-weight: 600; font-size: 14px;">
                                {WebUtility.HtmlEncode(n.TitleEn)}
                            </span>
                        </div>
                        <p style="color: #6b7280; font-size: 13px; margin: 4px 0 0;">
                            {WebUtility.HtmlEncode(n.MessageEn)}
                        </p>
                        <p style="color: #9ca3af; font-size: 11px; margin: 4px 0 0;">
                            {n.Created:MMM dd, yyyy HH:mm} UTC
                        </p>
                    </div>
                """);
        }

        sb.AppendLine("""
                    <div style="text-align: center; padding: 16px 0 8px;">
                        <a href="https://app.sqordia.com/notifications"
                           style="display: inline-block; padding: 10px 24px; background: #3b82f6; color: white; text-decoration: none; border-radius: 8px; font-size: 14px; font-weight: 500;">
                            View All Notifications
                        </a>
                    </div>
                </div>
            </div>
            """);

        return sb.ToString();
    }
}

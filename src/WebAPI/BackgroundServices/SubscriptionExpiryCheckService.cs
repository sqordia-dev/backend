using Microsoft.EntityFrameworkCore;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Services;
using Sqordia.Domain.Entities;
using Sqordia.Domain.Enums;

namespace WebAPI.BackgroundServices;

public class SubscriptionExpiryCheckService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SubscriptionExpiryCheckService> _logger;
    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(6);

    public SubscriptionExpiryCheckService(
        IServiceScopeFactory scopeFactory,
        ILogger<SubscriptionExpiryCheckService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait 5 minutes after startup to let the app stabilize
        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckExpiringSubscriptionsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking expiring subscriptions");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }

    private async Task CheckExpiringSubscriptionsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var warningDate = DateTime.UtcNow.AddDays(7);
        var today = DateTime.UtcNow.Date;

        // Find subscriptions expiring within 7 days that haven't been notified
        var expiringSubscriptions = await context.Subscriptions
            .Include(s => s.Plan)
            .Where(s => s.Status == SubscriptionStatus.Active
                && s.EndDate >= today
                && s.EndDate <= warningDate
                && s.Plan!.PlanType != SubscriptionPlanType.Free)
            .ToListAsync(ct);

        foreach (var sub in expiringSubscriptions)
        {
            // Check if we already sent a notification for this subscription's expiry
            var alreadyNotified = await context.Notifications
                .AnyAsync(n =>
                    n.UserId == sub.UserId
                    && n.Type == NotificationType.SubscriptionExpiring
                    && n.RelatedEntityId == sub.Id
                    && n.Created >= today.AddDays(-7),
                    ct);

            if (alreadyNotified) continue;

            var daysLeft = (sub.EndDate - DateTime.UtcNow).Days;
            var planName = sub.Plan?.Name ?? "votre plan";
            var priority = daysLeft <= 2 ? NotificationPriority.High : NotificationPriority.Normal;

            await notificationService.CreateNotificationAsync(
                new CreateNotificationCommand(
                    sub.UserId,
                    NotificationType.SubscriptionExpiring,
                    NotificationCategory.Subscription,
                    $"Votre abonnement {planName} expire dans {daysLeft} jour(s)",
                    $"Your {planName} subscription expires in {daysLeft} day(s)",
                    $"Renouvelez votre abonnement pour continuer à profiter de toutes les fonctionnalités.",
                    $"Renew your subscription to continue enjoying all features.",
                    ActionUrl: "/settings/subscription",
                    RelatedEntityId: sub.Id,
                    Priority: priority),
                ct);

            _logger.LogInformation(
                "Sent subscription expiry notification to user {UserId}, {DaysLeft} days remaining",
                sub.UserId, daysLeft);
        }
    }
}

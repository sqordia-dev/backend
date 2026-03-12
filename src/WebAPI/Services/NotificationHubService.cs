using Microsoft.AspNetCore.SignalR;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Contracts.Responses.Notification;
using Sqordia.WebAPI.Hubs;

namespace Sqordia.WebAPI.Services;

public class NotificationHubService : INotificationHubService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationHubService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendNotificationAsync(Guid userId, NotificationResponse notification, CancellationToken ct = default)
    {
        await _hubContext.Clients.Group($"user_{userId}")
            .SendAsync("ReceiveNotification", notification, ct);
    }

    public async Task SendUnreadCountAsync(Guid userId, int count, CancellationToken ct = default)
    {
        await _hubContext.Clients.Group($"user_{userId}")
            .SendAsync("UnreadCountUpdated", count, ct);
    }

    public async Task SendBulkNotificationsAsync(IEnumerable<Guid> userIds, NotificationResponse notification, CancellationToken ct = default)
    {
        var tasks = userIds.Select(userId =>
            _hubContext.Clients.Group($"user_{userId}")
                .SendAsync("ReceiveNotification", notification, ct));

        await Task.WhenAll(tasks);
    }
}

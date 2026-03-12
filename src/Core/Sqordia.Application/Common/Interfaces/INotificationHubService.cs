using Sqordia.Contracts.Responses.Notification;

namespace Sqordia.Application.Common.Interfaces;

public interface INotificationHubService
{
    Task SendNotificationAsync(Guid userId, NotificationResponse notification, CancellationToken ct = default);
    Task SendUnreadCountAsync(Guid userId, int count, CancellationToken ct = default);
    Task SendBulkNotificationsAsync(IEnumerable<Guid> userIds, NotificationResponse notification, CancellationToken ct = default);
}

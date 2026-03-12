using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Responses.Notification;

namespace Sqordia.Application.Services;

public interface INotificationAnalyticsService
{
    Task<Result<NotificationAnalyticsResponse>> GetAnalyticsAsync(
        int days = 30,
        CancellationToken cancellationToken = default);
}

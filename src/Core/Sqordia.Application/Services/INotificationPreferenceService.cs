using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Responses.Notification;
using Sqordia.Domain.Enums;

namespace Sqordia.Application.Services;

public interface INotificationPreferenceService
{
    Task<Result<NotificationPreferencesListResponse>> GetPreferencesAsync(
        CancellationToken cancellationToken = default);

    Task<Result> UpdatePreferenceAsync(
        NotificationType type,
        bool inAppEnabled,
        bool emailEnabled,
        NotificationFrequency emailFrequency,
        bool soundEnabled,
        CancellationToken cancellationToken = default);

    Task<Result> UpdatePreferencesBulkAsync(
        IEnumerable<(NotificationType Type, bool InAppEnabled, bool EmailEnabled, NotificationFrequency EmailFrequency, bool SoundEnabled)> preferences,
        CancellationToken cancellationToken = default);

    Task<bool> ShouldSendInAppAsync(Guid userId, NotificationType type, CancellationToken cancellationToken = default);
    Task<bool> ShouldSendEmailAsync(Guid userId, NotificationType type, CancellationToken cancellationToken = default);
    Task<bool> ShouldPlaySoundAsync(Guid userId, NotificationType type, CancellationToken cancellationToken = default);
}

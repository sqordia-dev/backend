using Sqordia.Domain.Enums;

namespace Sqordia.Contracts.Responses.Notification;

public class NotificationPreferenceResponse
{
    public Guid Id { get; set; }
    public NotificationType NotificationType { get; set; }
    public bool InAppEnabled { get; set; }
    public bool EmailEnabled { get; set; }
    public NotificationFrequency EmailFrequency { get; set; } = NotificationFrequency.Instant;
    public bool SoundEnabled { get; set; }
}

public class NotificationPreferencesListResponse
{
    public List<NotificationPreferenceResponse> Preferences { get; set; } = new();
}

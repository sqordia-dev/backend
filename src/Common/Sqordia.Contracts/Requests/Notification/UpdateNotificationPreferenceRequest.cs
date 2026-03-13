using Sqordia.Domain.Enums;

namespace Sqordia.Contracts.Requests.Notification;

public class UpdateNotificationPreferenceRequest
{
    public NotificationType NotificationType { get; set; }
    public bool InAppEnabled { get; set; } = true;
    public bool EmailEnabled { get; set; } = false;
    public NotificationFrequency EmailFrequency { get; set; } = NotificationFrequency.Instant;
    public bool SoundEnabled { get; set; } = true;
}

public class UpdateNotificationPreferencesBulkRequest
{
    public List<UpdateNotificationPreferenceRequest> Preferences { get; set; } = new();
}

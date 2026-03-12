namespace Sqordia.Contracts.Responses.Notification;

public class NotificationPreferenceResponse
{
    public Guid Id { get; set; }
    public string NotificationType { get; set; } = string.Empty;
    public bool InAppEnabled { get; set; }
    public bool EmailEnabled { get; set; }
    public string EmailFrequency { get; set; } = "Instant";
    public bool SoundEnabled { get; set; }
}

public class NotificationPreferencesListResponse
{
    public List<NotificationPreferenceResponse> Preferences { get; set; } = new();
}

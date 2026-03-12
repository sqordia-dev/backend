namespace Sqordia.Contracts.Requests.Notification;

public class UpdateNotificationPreferenceRequest
{
    public string NotificationType { get; set; } = string.Empty;
    public bool InAppEnabled { get; set; } = true;
    public bool EmailEnabled { get; set; } = false;
    public string EmailFrequency { get; set; } = "Instant";
    public bool SoundEnabled { get; set; } = true;
}

public class UpdateNotificationPreferencesBulkRequest
{
    public List<UpdateNotificationPreferenceRequest> Preferences { get; set; } = new();
}

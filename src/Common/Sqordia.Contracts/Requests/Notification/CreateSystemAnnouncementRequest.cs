using Sqordia.Domain.Enums;

namespace Sqordia.Contracts.Requests.Notification;

public class CreateSystemAnnouncementRequest
{
    public string TitleFr { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string MessageFr { get; set; } = string.Empty;
    public string MessageEn { get; set; } = string.Empty;
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
    public string? ActionUrl { get; set; }
}

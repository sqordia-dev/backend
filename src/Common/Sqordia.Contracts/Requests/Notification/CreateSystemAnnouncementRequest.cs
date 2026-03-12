namespace Sqordia.Contracts.Requests.Notification;

public class CreateSystemAnnouncementRequest
{
    public string TitleFr { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string MessageFr { get; set; } = string.Empty;
    public string MessageEn { get; set; } = string.Empty;
    public string Priority { get; set; } = "Normal";
    public string? ActionUrl { get; set; }
}

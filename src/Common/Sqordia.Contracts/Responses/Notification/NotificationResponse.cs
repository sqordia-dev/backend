namespace Sqordia.Contracts.Responses.Notification;

public class NotificationResponse
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Priority { get; set; } = "Normal";
    public string TitleFr { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string MessageFr { get; set; } = string.Empty;
    public string MessageEn { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? ActionUrl { get; set; }
    public string? MetadataJson { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public string? GroupKey { get; set; }
    public DateTime CreatedAt { get; set; }
}

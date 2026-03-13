using Sqordia.Domain.Common;
using Sqordia.Domain.Enums;

namespace Sqordia.Domain.Entities;

public class Notification : BaseAuditableEntity
{
    public Guid UserId { get; private set; }
    public NotificationType Type { get; private set; }
    public NotificationCategory Category { get; private set; }
    public NotificationPriority Priority { get; private set; }
    public string TitleFr { get; private set; } = null!;
    public string TitleEn { get; private set; } = null!;
    public string MessageFr { get; private set; } = null!;
    public string MessageEn { get; private set; } = null!;
    public bool IsRead { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public string? ActionUrl { get; private set; }
    public string? MetadataJson { get; private set; }
    public Guid? RelatedEntityId { get; private set; }
    public string? GroupKey { get; private set; }

    private Notification() { } // EF Core constructor

    public Notification(
        Guid userId,
        NotificationType type,
        NotificationCategory category,
        string titleFr,
        string titleEn,
        string messageFr,
        string messageEn,
        string? actionUrl = null,
        string? metadataJson = null,
        Guid? relatedEntityId = null,
        NotificationPriority priority = NotificationPriority.Normal,
        string? groupKey = null)
    {
        UserId = userId;
        Type = type;
        Category = category;
        Priority = priority;
        TitleFr = titleFr ?? throw new ArgumentNullException(nameof(titleFr));
        TitleEn = titleEn ?? throw new ArgumentNullException(nameof(titleEn));
        MessageFr = messageFr ?? throw new ArgumentNullException(nameof(messageFr));
        MessageEn = messageEn ?? throw new ArgumentNullException(nameof(messageEn));
        IsRead = false;
        ActionUrl = actionUrl;
        MetadataJson = metadataJson;
        RelatedEntityId = relatedEntityId;
        GroupKey = groupKey;
        Created = DateTime.UtcNow;
    }

    public void MarkAsRead()
    {
        IsRead = true;
        ReadAt = DateTime.UtcNow;
    }

    public void MarkAsUnread()
    {
        IsRead = false;
        ReadAt = null;
    }
}

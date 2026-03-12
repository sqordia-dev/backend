using Sqordia.Domain.Common;
using Sqordia.Domain.Enums;

namespace Sqordia.Domain.Entities;

public class NotificationPreference : BaseAuditableEntity
{
    public Guid UserId { get; private set; }
    public NotificationType NotificationType { get; private set; }
    public bool InAppEnabled { get; private set; }
    public bool EmailEnabled { get; private set; }
    public NotificationFrequency EmailFrequency { get; private set; }
    public bool SoundEnabled { get; private set; }

    private NotificationPreference() { } // EF Core constructor

    public NotificationPreference(
        Guid userId,
        NotificationType notificationType,
        bool inAppEnabled = true,
        bool emailEnabled = true,
        NotificationFrequency emailFrequency = NotificationFrequency.Instant,
        bool soundEnabled = true)
    {
        UserId = userId;
        NotificationType = notificationType;
        InAppEnabled = inAppEnabled;
        EmailEnabled = emailEnabled;
        EmailFrequency = emailFrequency;
        SoundEnabled = soundEnabled;
        Created = DateTime.UtcNow;
    }

    public void Update(
        bool inAppEnabled,
        bool emailEnabled,
        NotificationFrequency emailFrequency,
        bool soundEnabled)
    {
        InAppEnabled = inAppEnabled;
        EmailEnabled = emailEnabled;
        EmailFrequency = emailFrequency;
        SoundEnabled = soundEnabled;
        LastModified = DateTime.UtcNow;
    }

    public static NotificationPreference CreateDefault(Guid userId, NotificationType type)
    {
        var emailEnabled = type switch
        {
            NotificationType.OrganizationInvitation => true,
            NotificationType.BusinessPlanShared => true,
            NotificationType.SubscriptionExpiring => true,
            _ => false
        };

        return new NotificationPreference(userId, type, inAppEnabled: true, emailEnabled: emailEnabled);
    }
}

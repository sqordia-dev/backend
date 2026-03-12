using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities;

public class NewsletterSubscriber : BaseAuditableEntity
{
    public string Email { get; private set; } = null!;
    public bool IsActive { get; private set; } = true;
    public string Language { get; private set; } = "fr";
    public DateTime SubscribedAt { get; private set; }
    public DateTime? UnsubscribedAt { get; private set; }

    private NewsletterSubscriber() { } // EF Core

    public NewsletterSubscriber(string email, string language = "fr")
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        Email = email.Trim().ToLowerInvariant();
        Language = language;
        SubscribedAt = DateTime.UtcNow;
        IsActive = true;
    }

    public void Unsubscribe()
    {
        IsActive = false;
        UnsubscribedAt = DateTime.UtcNow;
    }

    public void Resubscribe()
    {
        IsActive = true;
        UnsubscribedAt = null;
    }
}

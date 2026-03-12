namespace Sqordia.Contracts.Responses.Newsletter;

public class NewsletterSubscriberResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string Language { get; set; } = "fr";
    public DateTime SubscribedAt { get; set; }
    public DateTime? UnsubscribedAt { get; set; }
}

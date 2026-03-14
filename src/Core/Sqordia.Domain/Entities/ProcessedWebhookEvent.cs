namespace Sqordia.Domain.Entities;

/// <summary>
/// Tracks processed Stripe webhook events for idempotency.
/// Prevents duplicate event processing when Stripe retries delivery.
/// </summary>
public class ProcessedWebhookEvent
{
    public string EventId { get; private set; } = null!;
    public string EventType { get; private set; } = null!;
    public DateTime ProcessedAt { get; private set; }

    private ProcessedWebhookEvent() { }

    public ProcessedWebhookEvent(string eventId, string eventType)
    {
        EventId = eventId;
        EventType = eventType;
        ProcessedAt = DateTime.UtcNow;
    }
}

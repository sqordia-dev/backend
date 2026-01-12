namespace Sqordia.Contracts.Responses.ActivityLog;

public class ActivityLogResponse
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public required string Action { get; set; }

    public string? EntityType { get; set; }

    public string? EntityId { get; set; }

    public DateTime Timestamp { get; set; }

    public bool Success { get; set; }
}

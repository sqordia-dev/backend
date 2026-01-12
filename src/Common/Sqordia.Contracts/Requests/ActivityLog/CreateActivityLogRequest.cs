using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.ActivityLog;

public class CreateActivityLogRequest
{
    [Required]
    public required string Action { get; set; }

    public string? EntityType { get; set; }

    public string? EntityId { get; set; }

    [Required]
    public required string Description { get; set; }

    public Dictionary<string, object>? Metadata { get; set; }

    public string? UserAgent { get; set; }
}

using Sqordia.Domain.Enums;

namespace Sqordia.Contracts.Responses.Admin.PromptRegistry;

/// <summary>
/// Version history entry for a prompt template
/// </summary>
public class PromptVersionHistoryDto
{
    public Guid Id { get; set; }
    public int Version { get; set; }
    public bool IsActive { get; set; }
    public PromptAlias? Alias { get; set; }
    public string? AliasName { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;

    // Brief performance summary
    public int TotalUsageCount { get; set; }
    public double AverageRating { get; set; }
    public double AcceptanceRate { get; set; }

    // Change summary (computed by comparing with previous version)
    public bool HasSystemPromptChanges { get; set; }
    public bool HasUserPromptChanges { get; set; }
    public bool HasOutputFormatChanges { get; set; }
}

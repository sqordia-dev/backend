using Sqordia.Domain.Enums;

namespace Sqordia.Contracts.Responses.Admin.PromptRegistry;

/// <summary>
/// Full prompt template details with performance summary
/// </summary>
public class PromptTemplateDto
{
    public Guid Id { get; set; }
    public SectionType SectionType { get; set; }
    public string SectionTypeName { get; set; } = string.Empty;
    public BusinessPlanType PlanType { get; set; }
    public string PlanTypeName { get; set; } = string.Empty;
    public string? IndustryCategory { get; set; }
    public int Version { get; set; }
    public bool IsActive { get; set; }
    public PromptAlias? Alias { get; set; }
    public string? AliasName { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SystemPrompt { get; set; } = string.Empty;
    public string UserPromptTemplate { get; set; } = string.Empty;
    public OutputFormat OutputFormat { get; set; }
    public string OutputFormatName { get; set; } = string.Empty;
    public string? VisualElementsJson { get; set; }
    public string? ExampleOutput { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;

    // Performance summary
    public int TotalUsageCount { get; set; }
    public double AverageRating { get; set; }
    public double AcceptanceRate { get; set; }
    public double EditRate { get; set; }
    public double RegenerateRate { get; set; }
    public int RatingCount { get; set; }
}

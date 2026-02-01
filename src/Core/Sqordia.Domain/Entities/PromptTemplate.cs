using Sqordia.Domain.Common;
using Sqordia.Domain.Enums;

namespace Sqordia.Domain.Entities;

/// <summary>
/// Entity for storing AI prompt templates with versioning and metadata
/// Supports industry-specific customization and A/B testing
/// </summary>
public class PromptTemplate : BaseEntity
{
    public SectionType SectionType { get; private set; }
    public BusinessPlanType PlanType { get; private set; }
    public string? IndustryCategory { get; private set; } // NAICS code or null for generic
    public int Version { get; private set; }
    public bool IsActive { get; private set; }
    public PromptAlias? Alias { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string SystemPrompt { get; private set; } = string.Empty;
    public string UserPromptTemplate { get; private set; } = string.Empty;
    public OutputFormat OutputFormat { get; private set; }
    public string? VisualElementsJson { get; private set; } // JSON for chart/table specs
    public string? ExampleOutput { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;

    // Navigation properties
    public virtual ICollection<PromptPerformance> PerformanceMetrics { get; private set; } = new List<PromptPerformance>();

    // Required for EF Core
    protected PromptTemplate() { }

    public PromptTemplate(
        SectionType sectionType,
        BusinessPlanType planType,
        string name,
        string systemPrompt,
        string userPromptTemplate,
        OutputFormat outputFormat,
        string createdBy,
        string? industryCategory = null,
        string? description = null,
        string? visualElementsJson = null,
        string? exampleOutput = null)
    {
        SectionType = sectionType;
        PlanType = planType;
        IndustryCategory = industryCategory;
        Version = 1;
        IsActive = false;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? string.Empty;
        SystemPrompt = systemPrompt ?? throw new ArgumentNullException(nameof(systemPrompt));
        UserPromptTemplate = userPromptTemplate ?? throw new ArgumentNullException(nameof(userPromptTemplate));
        OutputFormat = outputFormat;
        VisualElementsJson = visualElementsJson;
        ExampleOutput = exampleOutput;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        CreatedBy = createdBy ?? throw new ArgumentNullException(nameof(createdBy));
    }

    /// <summary>
    /// Activates this prompt template
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates this prompt template
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets the deployment alias for this prompt
    /// </summary>
    public void SetAlias(PromptAlias? alias)
    {
        Alias = alias;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the prompt content
    /// </summary>
    public void UpdateContent(
        string systemPrompt,
        string userPromptTemplate,
        string? description = null,
        string? visualElementsJson = null,
        string? exampleOutput = null)
    {
        SystemPrompt = systemPrompt ?? throw new ArgumentNullException(nameof(systemPrompt));
        UserPromptTemplate = userPromptTemplate ?? throw new ArgumentNullException(nameof(userPromptTemplate));

        if (description != null)
            Description = description;

        VisualElementsJson = visualElementsJson;
        ExampleOutput = exampleOutput;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the output format
    /// </summary>
    public void UpdateOutputFormat(OutputFormat outputFormat)
    {
        OutputFormat = outputFormat;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the name
    /// </summary>
    public void UpdateName(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Increments the version number
    /// </summary>
    public void IncrementVersion()
    {
        Version++;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets the industry category
    /// </summary>
    public void SetIndustryCategory(string? industryCategory)
    {
        IndustryCategory = industryCategory;
        UpdatedAt = DateTime.UtcNow;
    }
}

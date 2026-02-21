using System.ComponentModel.DataAnnotations;
using Sqordia.Domain.Enums;

namespace Sqordia.Contracts.Requests.Admin.PromptRegistry;

/// <summary>
/// Request to create a new prompt template
/// </summary>
public class CreatePromptTemplateRequest
{
    /// <summary>
    /// Type of section this prompt is for
    /// </summary>
    [Required]
    public SectionType SectionType { get; set; }

    /// <summary>
    /// Type of business plan this prompt is for
    /// </summary>
    [Required]
    public BusinessPlanType PlanType { get; set; }

    /// <summary>
    /// Optional industry category (NAICS code) for industry-specific prompts
    /// </summary>
    [StringLength(50)]
    public string? IndustryCategory { get; set; }

    /// <summary>
    /// Name of the prompt template
    /// </summary>
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public required string Name { get; set; }

    /// <summary>
    /// Description of what this prompt is used for
    /// </summary>
    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// The system prompt that defines the AI's role and behavior
    /// </summary>
    [Required]
    [StringLength(10000, MinimumLength = 50)]
    public required string SystemPrompt { get; set; }

    /// <summary>
    /// Template for the user prompt with placeholders for variables
    /// </summary>
    [Required]
    [StringLength(10000, MinimumLength = 20)]
    public required string UserPromptTemplate { get; set; }

    /// <summary>
    /// Output format for the generated content
    /// </summary>
    public OutputFormat OutputFormat { get; set; } = OutputFormat.Prose;

    /// <summary>
    /// JSON specification for visual elements (charts, tables)
    /// </summary>
    public string? VisualElementsJson { get; set; }

    /// <summary>
    /// Example output to guide the AI
    /// </summary>
    public string? ExampleOutput { get; set; }
}

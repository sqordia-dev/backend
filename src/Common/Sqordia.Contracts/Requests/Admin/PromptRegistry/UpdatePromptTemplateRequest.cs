using System.ComponentModel.DataAnnotations;
using Sqordia.Domain.Enums;

namespace Sqordia.Contracts.Requests.Admin.PromptRegistry;

/// <summary>
/// Request to update an existing prompt template
/// All fields are optional - only provided fields will be updated
/// </summary>
public class UpdatePromptTemplateRequest
{
    /// <summary>
    /// Name of the prompt template
    /// </summary>
    [StringLength(200, MinimumLength = 3)]
    public string? Name { get; set; }

    /// <summary>
    /// Description of what this prompt is used for
    /// </summary>
    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// The system prompt that defines the AI's role and behavior
    /// </summary>
    [StringLength(10000, MinimumLength = 50)]
    public string? SystemPrompt { get; set; }

    /// <summary>
    /// Template for the user prompt with placeholders for variables
    /// </summary>
    [StringLength(10000, MinimumLength = 20)]
    public string? UserPromptTemplate { get; set; }

    /// <summary>
    /// Output format for the generated content
    /// </summary>
    public OutputFormat? OutputFormat { get; set; }

    /// <summary>
    /// JSON specification for visual elements (charts, tables)
    /// </summary>
    public string? VisualElementsJson { get; set; }

    /// <summary>
    /// Example output to guide the AI
    /// </summary>
    public string? ExampleOutput { get; set; }

    /// <summary>
    /// Optional industry category (NAICS code) for industry-specific prompts
    /// </summary>
    [StringLength(50)]
    public string? IndustryCategory { get; set; }
}

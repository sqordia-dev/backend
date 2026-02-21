using System.ComponentModel.DataAnnotations;
using Sqordia.Domain.Enums;

namespace Sqordia.Contracts.Requests.Admin.PromptRegistry;

/// <summary>
/// Request to test a draft prompt (before saving) with sample data
/// </summary>
public class TestDraftPromptRequest
{
    /// <summary>
    /// Type of section this prompt is for (for context)
    /// </summary>
    [Required]
    public SectionType SectionType { get; set; }

    /// <summary>
    /// Type of business plan this prompt is for (for context)
    /// </summary>
    [Required]
    public BusinessPlanType PlanType { get; set; }

    /// <summary>
    /// The system prompt to test
    /// </summary>
    [Required]
    [StringLength(10000, MinimumLength = 50)]
    public required string SystemPrompt { get; set; }

    /// <summary>
    /// The user prompt template to test
    /// </summary>
    [Required]
    [StringLength(10000, MinimumLength = 20)]
    public required string UserPromptTemplate { get; set; }

    /// <summary>
    /// JSON object containing sample variable values for template substitution
    /// </summary>
    [Required]
    public required string SampleVariables { get; set; }

    /// <summary>
    /// Optional AI provider to use for testing (OpenAI, Claude, Gemini)
    /// If not specified, uses the active provider from settings
    /// </summary>
    [StringLength(50)]
    public string? Provider { get; set; }

    /// <summary>
    /// Maximum tokens for the response (100-4000)
    /// </summary>
    [Range(100, 4000)]
    public int MaxTokens { get; set; } = 1000;

    /// <summary>
    /// Temperature for response generation (0.0-1.0)
    /// </summary>
    [Range(0.0, 1.0)]
    public double Temperature { get; set; } = 0.7;
}

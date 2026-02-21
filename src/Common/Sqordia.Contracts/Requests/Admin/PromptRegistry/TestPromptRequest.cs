using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.Admin.PromptRegistry;

/// <summary>
/// Request to test an existing prompt template with sample data
/// </summary>
public class TestPromptRequest
{
    /// <summary>
    /// JSON object containing sample variable values for template substitution
    /// Example: {"businessName": "Acme Corp", "industry": "Technology"}
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
    /// Lower values are more deterministic, higher values more creative
    /// </summary>
    [Range(0.0, 1.0)]
    public double Temperature { get; set; } = 0.7;
}

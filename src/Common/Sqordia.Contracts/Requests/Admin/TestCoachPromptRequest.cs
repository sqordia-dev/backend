using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.Admin;

/// <summary>
/// Request to test a question's coach prompt with a sample answer
/// </summary>
public class TestCoachPromptRequest
{
    /// <summary>
    /// The user's answer to test the coach prompt with
    /// </summary>
    [Required]
    [MinLength(1)]
    public required string Answer { get; set; }

    /// <summary>
    /// Language for the coach prompt (fr or en)
    /// </summary>
    [Required]
    [RegularExpression("^(fr|en)$", ErrorMessage = "Language must be 'fr' or 'en'")]
    public required string Language { get; set; } = "fr";

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

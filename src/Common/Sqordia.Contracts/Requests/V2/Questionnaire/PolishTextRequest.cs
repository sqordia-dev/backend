using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.V2.Questionnaire;

/// <summary>
/// Request to polish/enhance text using AI
/// </summary>
public class PolishTextRequest
{
    /// <summary>
    /// The raw text to polish
    /// </summary>
    [Required]
    [MinLength(10, ErrorMessage = "Text must be at least 10 characters")]
    public required string Text { get; set; }

    /// <summary>
    /// Language for the polished output (fr or en)
    /// </summary>
    [Required]
    public required string Language { get; set; }

    /// <summary>
    /// Optional context about the text (e.g., section name, business type)
    /// </summary>
    public string? Context { get; set; }

    /// <summary>
    /// Desired tone: professional, casual, formal, persuasive
    /// </summary>
    public string Tone { get; set; } = "professional";

    /// <summary>
    /// Target audience for the text
    /// </summary>
    public string? TargetAudience { get; set; }
}

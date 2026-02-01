using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.Sections;

/// <summary>
/// Request for AI-assisted content modification
/// </summary>
public class AiAssistRequest
{
    /// <summary>
    /// The type of AI assistance requested: "improve", "expand", or "shorten"
    /// </summary>
    [Required]
    [RegularExpression("^(improve|expand|shorten)$", ErrorMessage = "Action must be 'improve', 'expand', or 'shorten'")]
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// The current content to be modified
    /// </summary>
    [Required]
    [StringLength(50000, MinimumLength = 1, ErrorMessage = "Content must be between 1 and 50000 characters")]
    public string CurrentContent { get; set; } = string.Empty;

    /// <summary>
    /// Optional additional instructions for the AI
    /// </summary>
    [StringLength(1000)]
    public string? Instructions { get; set; }

    /// <summary>
    /// Language for the output (fr or en). Defaults to fr.
    /// </summary>
    [StringLength(2, MinimumLength = 2)]
    public string Language { get; set; } = "fr";
}

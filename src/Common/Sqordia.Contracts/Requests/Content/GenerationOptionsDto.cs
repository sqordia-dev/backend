namespace Sqordia.Contracts.Requests.Content;

/// <summary>
/// Options for AI content generation
/// </summary>
public class GenerationOptionsDto
{
    /// <summary>
    /// Language for content generation (e.g., "fr", "en")
    /// </summary>
    public string Language { get; set; } = "fr";

    /// <summary>
    /// Preferred prompt alias to use (e.g., "production", "staging", "experimental")
    /// </summary>
    public string? PreferredAlias { get; set; }

    /// <summary>
    /// Additional template variables to pass to the prompt
    /// </summary>
    public Dictionary<string, string> AdditionalVariables { get; set; } = new();

    /// <summary>
    /// Whether to include visual elements (charts, tables, metrics) in the generated content
    /// </summary>
    public bool IncludeVisualElements { get; set; } = true;
}

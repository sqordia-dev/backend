namespace Sqordia.Contracts.Responses.Sections;

/// <summary>
/// Response from AI-assisted content modification
/// </summary>
public class AiAssistResponse
{
    /// <summary>
    /// The AI-generated or modified content
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// The action that was performed
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Original content word count
    /// </summary>
    public int OriginalWordCount { get; set; }

    /// <summary>
    /// New content word count
    /// </summary>
    public int NewWordCount { get; set; }

    /// <summary>
    /// Processing time in milliseconds
    /// </summary>
    public long ProcessingTimeMs { get; set; }

    /// <summary>
    /// The AI model used for generation
    /// </summary>
    public string? Model { get; set; }
}

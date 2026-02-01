namespace Sqordia.Contracts.Responses.V2.Questionnaire;

/// <summary>
/// Response with AI-polished text
/// </summary>
public class PolishedTextResponse
{
    /// <summary>
    /// The original input text
    /// </summary>
    public required string OriginalText { get; set; }

    /// <summary>
    /// The polished/enhanced text
    /// </summary>
    public required string PolishedText { get; set; }

    /// <summary>
    /// Confidence score for the enhancement (0-1)
    /// </summary>
    public decimal Confidence { get; set; }

    /// <summary>
    /// List of improvements made
    /// </summary>
    public required List<string> Improvements { get; set; }

    /// <summary>
    /// Alternative suggestions if available
    /// </summary>
    public List<string>? Alternatives { get; set; }

    public DateTime GeneratedAt { get; set; }
    public string? Model { get; set; }
}

namespace Sqordia.Contracts.Responses.AI;

/// <summary>
/// Response from polishing an answer
/// </summary>
public class PolishAnswerResponse
{
    /// <summary>
    /// The polished/enhanced text
    /// </summary>
    public string? PolishedText { get; set; }

    /// <summary>
    /// Strength score (0-100)
    /// </summary>
    public int StrengthScore { get; set; }

    /// <summary>
    /// Original text for comparison
    /// </summary>
    public string? OriginalText { get; set; }

    /// <summary>
    /// List of improvements made
    /// </summary>
    public List<string> Improvements { get; set; } = new();
}

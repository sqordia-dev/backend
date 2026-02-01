namespace Sqordia.Contracts.Responses.AI;

/// <summary>
/// Response from analyzing an answer (polish + gaps)
/// </summary>
public class AnalyzeAnswerResponse
{
    /// <summary>
    /// The polished/enhanced text (if requested)
    /// </summary>
    public string? PolishedText { get; set; }

    /// <summary>
    /// Strength score (0-100)
    /// </summary>
    public int StrengthScore { get; set; }

    /// <summary>
    /// Identified gaps in the answer
    /// </summary>
    public List<AnswerGap> Gaps { get; set; } = new();

    /// <summary>
    /// List of improvements made
    /// </summary>
    public List<string> Improvements { get; set; } = new();
}

/// <summary>
/// A gap identified in an answer
/// </summary>
public class AnswerGap
{
    /// <summary>
    /// Category: Financial, Strategic, Legal, QuebecCompliance
    /// </summary>
    public string Category { get; set; } = "Strategic";

    /// <summary>
    /// Priority: high, medium, low
    /// </summary>
    public string Priority { get; set; } = "medium";

    /// <summary>
    /// Description of the gap
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Suggested improvement
    /// </summary>
    public string Suggestion { get; set; } = string.Empty;

    /// <summary>
    /// Optional follow-up question prompt
    /// </summary>
    public string? QuestionPrompt { get; set; }
}

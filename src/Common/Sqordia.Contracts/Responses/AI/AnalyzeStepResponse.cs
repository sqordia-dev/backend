namespace Sqordia.Contracts.Responses.AI;

/// <summary>
/// Response from analyzing a questionnaire step
/// </summary>
public class AnalyzeStepResponse
{
    /// <summary>
    /// Overall score for the step (0-100)
    /// </summary>
    public int OverallScore { get; set; }

    /// <summary>
    /// Analysis for each question in the step
    /// </summary>
    public List<QuestionAnalysis> Questions { get; set; } = new();

    /// <summary>
    /// Summary of improvements needed
    /// </summary>
    public string? Summary { get; set; }
}

/// <summary>
/// Analysis for a single question
/// </summary>
public class QuestionAnalysis
{
    /// <summary>
    /// The question ID
    /// </summary>
    public string QuestionId { get; set; } = string.Empty;

    /// <summary>
    /// Score for this answer (0-100)
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// Gaps identified in this answer
    /// </summary>
    public List<AnswerGap> Gaps { get; set; } = new();
}

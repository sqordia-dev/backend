namespace Sqordia.Contracts.Responses.Questionnaire;

/// <summary>
/// Represents the progress of a business plan questionnaire
/// </summary>
public class QuestionnaireProgressDto
{
    /// <summary>
    /// The business plan ID
    /// </summary>
    public required Guid BusinessPlanId { get; set; }
    
    /// <summary>
    /// The business plan title
    /// </summary>
    public required string BusinessPlanTitle { get; set; }
    
    /// <summary>
    /// Total number of questions in the questionnaire
    /// </summary>
    public required int TotalQuestions { get; set; }
    
    /// <summary>
    /// Number of questions that have been answered
    /// </summary>
    public required int CompletedQuestions { get; set; }
    
    /// <summary>
    /// Number of questions that remain unanswered
    /// </summary>
    public required int RemainingQuestions { get; set; }
    
    /// <summary>
    /// Completion percentage (0-100)
    /// </summary>
    public required decimal CompletionPercentage { get; set; }
    
    /// <summary>
    /// Current status of the questionnaire
    /// </summary>
    public required string Status { get; set; }
    
    /// <summary>
    /// Whether the questionnaire is complete
    /// </summary>
    public required bool IsComplete { get; set; }
    
    /// <summary>
    /// When the questionnaire was started (first answer)
    /// </summary>
    public DateTime? StartedAt { get; set; }
    
    /// <summary>
    /// When the questionnaire was completed (last answer)
    /// </summary>
    public DateTime? CompletedAt { get; set; }
    
    /// <summary>
    /// Estimated time remaining to complete (in minutes)
    /// </summary>
    public int? EstimatedTimeRemaining { get; set; }
    
    /// <summary>
    /// Average time per question (in minutes)
    /// </summary>
    public decimal? AverageTimePerQuestion { get; set; }
    
    /// <summary>
    /// List of unanswered question IDs
    /// </summary>
    public List<string> UnansweredQuestionIds { get; set; } = new();
    
    /// <summary>
    /// List of recently answered questions
    /// </summary>
    public List<RecentAnswerDto> RecentAnswers { get; set; } = new();
    
    /// <summary>
    /// Progress milestones achieved
    /// </summary>
    public List<string> MilestonesAchieved { get; set; } = new();
    
    /// <summary>
    /// Next milestone to achieve
    /// </summary>
    public string? NextMilestone { get; set; }
    
    /// <summary>
    /// Questions remaining to reach next milestone
    /// </summary>
    public int? QuestionsToNextMilestone { get; set; }
}

/// <summary>
/// Represents a recently answered question
/// </summary>
public class RecentAnswerDto
{
    /// <summary>
    /// The question ID
    /// </summary>
    public required string QuestionId { get; set; }
    
    /// <summary>
    /// The question text
    /// </summary>
    public required string QuestionText { get; set; }
    
    /// <summary>
    /// When the question was answered
    /// </summary>
    public required DateTime AnsweredAt { get; set; }
    
    /// <summary>
    /// The answer provided (truncated if too long)
    /// </summary>
    public required string Answer { get; set; }
    
    /// <summary>
    /// Whether the answer was provided by AI suggestion
    /// </summary>
    public required bool IsAISuggested { get; set; }
}

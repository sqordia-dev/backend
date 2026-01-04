namespace Sqordia.Contracts.Responses.Questionnaire;

/// <summary>
/// Response containing AI-generated suggestions for a questionnaire question
/// </summary>
public class QuestionSuggestionResponse
{
    /// <summary>
    /// The original question text
    /// </summary>
    public required string QuestionText { get; set; }
    
    /// <summary>
    /// The business plan type used for context
    /// </summary>
    public required string PlanType { get; set; }
    
    /// <summary>
    /// List of AI-generated suggestions
    /// </summary>
    public required List<QuestionSuggestion> Suggestions { get; set; }
    
    /// <summary>
    /// Timestamp when suggestions were generated
    /// </summary>
    public required DateTime GeneratedAt { get; set; }
    
    /// <summary>
    /// AI model used for generation
    /// </summary>
    public required string Model { get; set; }
    
    /// <summary>
    /// Language of the generated suggestions
    /// </summary>
    public required string Language { get; set; }
}

/// <summary>
/// Individual AI-generated suggestion
/// </summary>
public class QuestionSuggestion
{
    /// <summary>
    /// The suggested answer text
    /// </summary>
    public required string Answer { get; set; }
    
    /// <summary>
    /// Confidence score for this suggestion (0.0 - 1.0)
    /// </summary>
    public required double Confidence { get; set; }
    
    /// <summary>
    /// Brief explanation of why this suggestion is relevant
    /// </summary>
    public required string Reasoning { get; set; }
    
    /// <summary>
    /// Suggestion type (e.g., "Detailed", "Concise", "Professional")
    /// </summary>
    public required string SuggestionType { get; set; }
}

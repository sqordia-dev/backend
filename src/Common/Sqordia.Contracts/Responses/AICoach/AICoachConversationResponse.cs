namespace Sqordia.Contracts.Responses.AICoach;

/// <summary>
/// Response containing AI Coach conversation data
/// </summary>
public class AICoachConversationResponse
{
    /// <summary>
    /// The conversation ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The business plan this conversation belongs to
    /// </summary>
    public Guid BusinessPlanId { get; set; }

    /// <summary>
    /// The question ID this conversation is about
    /// </summary>
    public string QuestionId { get; set; } = string.Empty;

    /// <summary>
    /// The question number in the questionnaire flow
    /// </summary>
    public int? QuestionNumber { get; set; }

    /// <summary>
    /// The question text
    /// </summary>
    public string? QuestionText { get; set; }

    /// <summary>
    /// Language of the conversation
    /// </summary>
    public string Language { get; set; } = "en";

    /// <summary>
    /// User persona
    /// </summary>
    public string? Persona { get; set; }

    /// <summary>
    /// Total tokens used in this conversation
    /// </summary>
    public int TotalTokensUsed { get; set; }

    /// <summary>
    /// Timestamp of the last message
    /// </summary>
    public DateTime? LastMessageAt { get; set; }

    /// <summary>
    /// Whether the conversation is still active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Messages in this conversation
    /// </summary>
    public List<AICoachMessageResponse> Messages { get; set; } = new();

    /// <summary>
    /// When the conversation was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

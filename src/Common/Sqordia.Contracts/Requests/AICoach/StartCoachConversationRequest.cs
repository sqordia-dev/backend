using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.AICoach;

/// <summary>
/// Request to start a new AI Coach conversation for a specific question
/// </summary>
public class StartCoachConversationRequest
{
    /// <summary>
    /// The business plan ID this conversation relates to
    /// </summary>
    [Required]
    public Guid BusinessPlanId { get; set; }

    /// <summary>
    /// The question ID this conversation is about
    /// </summary>
    [Required]
    [MaxLength(100)]
    public required string QuestionId { get; set; }

    /// <summary>
    /// The question number in the questionnaire flow
    /// </summary>
    public int? QuestionNumber { get; set; }

    /// <summary>
    /// The question text being asked
    /// </summary>
    [MaxLength(2000)]
    public string? QuestionText { get; set; }

    /// <summary>
    /// The user's current answer (if any) to provide context
    /// </summary>
    public string? CurrentAnswer { get; set; }

    /// <summary>
    /// Language for the conversation: en, fr
    /// </summary>
    [MaxLength(10)]
    public string Language { get; set; } = "en";

    /// <summary>
    /// User persona: Entrepreneur, Consultant, OBNL
    /// </summary>
    [MaxLength(50)]
    public string? Persona { get; set; }

    /// <summary>
    /// Initial message from the user to start the conversation
    /// </summary>
    [Required]
    [MinLength(1)]
    public required string InitialMessage { get; set; }
}

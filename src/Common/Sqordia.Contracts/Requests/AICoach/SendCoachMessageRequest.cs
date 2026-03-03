using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.AICoach;

/// <summary>
/// Request to send a message in an existing AI Coach conversation
/// </summary>
public class SendCoachMessageRequest
{
    /// <summary>
    /// The conversation ID to send the message to
    /// </summary>
    [Required]
    public Guid ConversationId { get; set; }

    /// <summary>
    /// The message content from the user
    /// </summary>
    [Required]
    [MinLength(1)]
    public required string Message { get; set; }
}

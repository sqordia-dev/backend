namespace Sqordia.Contracts.Responses.AICoach;

/// <summary>
/// Response containing a single AI Coach message
/// </summary>
public class AICoachMessageResponse
{
    /// <summary>
    /// The message ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The role of the message sender: "user" or "assistant"
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// The message content
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Number of tokens used by this message
    /// </summary>
    public int TokenCount { get; set; }

    /// <summary>
    /// The sequence number of this message in the conversation
    /// </summary>
    public int Sequence { get; set; }

    /// <summary>
    /// When the message was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

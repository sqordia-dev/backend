using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities.AICoach;

/// <summary>
/// Represents a single message in an AI Coach conversation.
/// Messages can be from the user or the assistant (AI).
/// </summary>
public class AICoachMessage : BaseAuditableEntity
{
    public Guid ConversationId { get; private set; }
    public string Role { get; private set; } = null!; // "user" or "assistant"
    public string Content { get; private set; } = null!;
    public int TokenCount { get; private set; }
    public int Sequence { get; private set; }

    // Navigation properties
    public virtual AICoachConversation Conversation { get; private set; } = null!;

    private AICoachMessage() { } // EF Core constructor

    public AICoachMessage(
        Guid conversationId,
        string role,
        string content,
        int tokenCount,
        int sequence)
    {
        ConversationId = conversationId;
        Role = role ?? throw new ArgumentNullException(nameof(role));
        Content = content ?? throw new ArgumentNullException(nameof(content));
        TokenCount = tokenCount;
        Sequence = sequence;
        Created = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a user message
    /// </summary>
    public static AICoachMessage CreateUserMessage(Guid conversationId, string content, int tokenCount, int sequence)
    {
        return new AICoachMessage(conversationId, "user", content, tokenCount, sequence);
    }

    /// <summary>
    /// Creates an assistant (AI) message
    /// </summary>
    public static AICoachMessage CreateAssistantMessage(Guid conversationId, string content, int tokenCount, int sequence)
    {
        return new AICoachMessage(conversationId, "assistant", content, tokenCount, sequence);
    }

    public bool IsUserMessage => Role == "user";
    public bool IsAssistantMessage => Role == "assistant";
}

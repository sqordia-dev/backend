using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities.AICoach;

/// <summary>
/// Represents an AI Coach conversation for a specific question in a business plan.
/// Each question can have its own conversation thread that persists across sessions.
/// </summary>
public class AICoachConversation : BaseAuditableEntity
{
    public Guid UserId { get; private set; }
    public Guid BusinessPlanId { get; private set; }
    public string QuestionId { get; private set; } = null!;
    public int? QuestionNumber { get; private set; }
    public string? QuestionText { get; private set; }
    public string Language { get; private set; } = "en";
    public string? Persona { get; private set; }
    public int TotalTokensUsed { get; private set; }
    public DateTime? LastMessageAt { get; private set; }
    public bool IsActive { get; private set; }

    // Navigation properties
    public virtual ICollection<AICoachMessage> Messages { get; private set; } = new List<AICoachMessage>();

    private AICoachConversation() { } // EF Core constructor

    public AICoachConversation(
        Guid userId,
        Guid businessPlanId,
        string questionId,
        int? questionNumber = null,
        string? questionText = null,
        string language = "en",
        string? persona = null)
    {
        UserId = userId;
        BusinessPlanId = businessPlanId;
        QuestionId = questionId ?? throw new ArgumentNullException(nameof(questionId));
        QuestionNumber = questionNumber;
        QuestionText = questionText;
        Language = language;
        Persona = persona;
        TotalTokensUsed = 0;
        IsActive = true;
        Created = DateTime.UtcNow;
    }

    public void AddMessage(AICoachMessage message)
    {
        Messages.Add(message);
        TotalTokensUsed += message.TokenCount;
        LastMessageAt = DateTime.UtcNow;
    }

    public void IncrementTokenUsage(int tokens)
    {
        TotalTokensUsed += tokens;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Reactivate()
    {
        IsActive = true;
    }

    public void UpdateContext(string? questionText, string language, string? persona)
    {
        QuestionText = questionText;
        Language = language;
        Persona = persona;
    }
}

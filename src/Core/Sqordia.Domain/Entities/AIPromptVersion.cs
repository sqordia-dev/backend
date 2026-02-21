using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities;

/// <summary>
/// Represents a historical version of an AI prompt for version tracking and rollback
/// </summary>
public class AIPromptVersion : BaseEntity
{
    public Guid AIPromptId { get; private set; }
    public int Version { get; private set; }
    public string SystemPrompt { get; private set; } = null!;
    public string UserPromptTemplate { get; private set; } = null!;
    public string? Variables { get; private set; }
    public string? Notes { get; private set; }
    public string? ChangedBy { get; private set; }
    public DateTime ChangedAt { get; private set; }

    // Navigation property
    public AIPrompt AIPrompt { get; private set; } = null!;

    private AIPromptVersion() { } // EF Core constructor

    public AIPromptVersion(
        Guid aiPromptId,
        int version,
        string systemPrompt,
        string userPromptTemplate,
        string? variables = null,
        string? notes = null,
        string? changedBy = null)
    {
        AIPromptId = aiPromptId;
        Version = version;
        SystemPrompt = systemPrompt ?? throw new ArgumentNullException(nameof(systemPrompt));
        UserPromptTemplate = userPromptTemplate ?? throw new ArgumentNullException(nameof(userPromptTemplate));
        Variables = variables;
        Notes = notes;
        ChangedBy = changedBy;
        ChangedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a version snapshot from an existing AIPrompt
    /// </summary>
    public static AIPromptVersion CreateFromPrompt(AIPrompt prompt, string? changedBy = null, string? notes = null)
    {
        return new AIPromptVersion(
            prompt.Id,
            prompt.Version,
            prompt.SystemPrompt,
            prompt.UserPromptTemplate,
            prompt.Variables,
            notes ?? prompt.Notes,
            changedBy);
    }
}

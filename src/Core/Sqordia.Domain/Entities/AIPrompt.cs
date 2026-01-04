using Sqordia.Domain.Common;
using Sqordia.Domain.Enums;

namespace Sqordia.Domain.Entities;

/// <summary>
/// Represents an AI prompt template for generating content
/// </summary>
public class AIPrompt : BaseAuditableEntity
{
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public string Category { get; private set; } = null!; // e.g., "QuestionSuggestions", "SectionImprovement", "ContentGeneration"
    public string PlanType { get; private set; } = null!; // e.g., "BusinessPlan", "StrategicPlan", "LeanCanvas"
    public string Language { get; private set; } = null!; // "fr" or "en"
    public string SystemPrompt { get; private set; } = null!;
    public string UserPromptTemplate { get; private set; } = null!;
    public string Variables { get; private set; } = string.Empty; // JSON string of available variables
    public bool IsActive { get; private set; }
    public int Version { get; private set; }
    public string? ParentPromptId { get; private set; } // For versioning
    public string? Notes { get; private set; }
    public int UsageCount { get; private set; }
    public DateTime? LastUsedAt { get; private set; }
    public double AverageRating { get; private set; }
    public int RatingCount { get; private set; }

    private AIPrompt() { } // EF Core constructor

    public AIPrompt(
        string name,
        string description,
        string category,
        string planType,
        string language,
        string systemPrompt,
        string userPromptTemplate,
        string? variables = null,
        string? notes = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Category = category ?? throw new ArgumentNullException(nameof(category));
        PlanType = planType ?? throw new ArgumentNullException(nameof(planType));
        Language = language ?? throw new ArgumentNullException(nameof(language));
        SystemPrompt = systemPrompt ?? throw new ArgumentNullException(nameof(systemPrompt));
        UserPromptTemplate = userPromptTemplate ?? throw new ArgumentNullException(nameof(userPromptTemplate));
        Variables = variables ?? string.Empty;
        Notes = notes;
        IsActive = true;
        Version = 1;
        UsageCount = 0;
        AverageRating = 0.0;
        RatingCount = 0;
    }

    public void UpdateContent(string systemPrompt, string userPromptTemplate, string? notes = null)
    {
        SystemPrompt = systemPrompt ?? throw new ArgumentNullException(nameof(systemPrompt));
        UserPromptTemplate = userPromptTemplate ?? throw new ArgumentNullException(nameof(userPromptTemplate));
        Notes = notes;
        IncrementVersion();
    }

    public void UpdateMetadata(string name, string description, string? notes = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Notes = notes;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void RecordUsage()
    {
        UsageCount++;
        LastUsedAt = DateTime.UtcNow;
    }

    public void AddRating(double rating)
    {
        if (rating < 0 || rating > 5)
            throw new ArgumentOutOfRangeException(nameof(rating), "Rating must be between 0 and 5");

        var totalRating = AverageRating * RatingCount + rating;
        RatingCount++;
        AverageRating = totalRating / RatingCount;
    }

    public void IncrementVersion()
    {
        Version++;
    }

    public void SetParentPrompt(string parentPromptId)
    {
        ParentPromptId = parentPromptId;
    }
}

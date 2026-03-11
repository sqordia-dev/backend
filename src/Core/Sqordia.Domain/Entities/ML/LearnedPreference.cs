using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities.ML;

/// <summary>
/// Stores preferences learned from user edit patterns.
/// Updated by the ML service's preference extraction pipeline.
/// Consumed by the generation pipeline to enhance rubrics.
/// </summary>
public class LearnedPreference : BaseEntity
{
    public string SectionType { get; private set; } = null!;
    public string? Industry { get; private set; }
    public string? PlanType { get; private set; }
    public string Language { get; private set; } = "fr";

    /// <summary>Category: "tone", "content", "structure", "length", "data_density".</summary>
    public string PreferenceType { get; private set; } = null!;

    /// <summary>
    /// JSON payload with the learned preference details.
    /// Example: {"preferred_tone":"data-driven","avoid":["vague claims"],"add":["regulatory context"]}
    /// </summary>
    public string PreferenceJson { get; private set; } = null!;

    /// <summary>Number of edits this preference was derived from.</summary>
    public int SampleCount { get; private set; }

    /// <summary>Confidence score (0-1) based on consistency of the pattern.</summary>
    public double Confidence { get; private set; }

    public bool IsActive { get; private set; }
    public DateTime LearnedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }

    private LearnedPreference() { } // EF Core

    public LearnedPreference(
        string sectionType,
        string preferenceType,
        string preferenceJson,
        int sampleCount,
        double confidence,
        string language = "fr",
        string? industry = null,
        string? planType = null,
        DateTime? expiresAt = null)
    {
        SectionType = sectionType;
        PreferenceType = preferenceType;
        PreferenceJson = preferenceJson;
        SampleCount = sampleCount;
        Confidence = confidence;
        Language = language;
        Industry = industry;
        PlanType = planType;
        IsActive = true;
        LearnedAt = DateTime.UtcNow;
        ExpiresAt = expiresAt;
    }

    public void Update(string preferenceJson, int sampleCount, double confidence)
    {
        PreferenceJson = preferenceJson;
        SampleCount = sampleCount;
        Confidence = confidence;
        LearnedAt = DateTime.UtcNow;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}

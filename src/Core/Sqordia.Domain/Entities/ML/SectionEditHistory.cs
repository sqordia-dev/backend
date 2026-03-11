using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities.ML;

/// <summary>
/// Tracks what users change after AI generates content.
/// The diff between AI output and user-edited output is the most valuable ML training signal.
/// </summary>
public class SectionEditHistory : BaseEntity
{
    public Guid BusinessPlanId { get; private set; }
    public string SectionType { get; private set; } = null!;
    public string AiGeneratedContent { get; private set; } = null!;
    public string UserEditedContent { get; private set; } = null!;

    /// <summary>Word-level edit distance between AI and user content.</summary>
    public int EditDistance { get; private set; }

    /// <summary>Ratio of changed words (0 = identical, 1 = fully rewritten).</summary>
    public double EditRatio { get; private set; }

    public Guid? PromptTemplateId { get; private set; }
    public string? Industry { get; private set; }
    public string? PlanType { get; private set; }
    public string Language { get; private set; } = "fr";

    public DateTime CreatedAt { get; private set; }

    private SectionEditHistory() { } // EF Core

    public SectionEditHistory(
        Guid businessPlanId,
        string sectionType,
        string aiGeneratedContent,
        string userEditedContent,
        int editDistance,
        double editRatio,
        string language,
        Guid? promptTemplateId = null,
        string? industry = null,
        string? planType = null)
    {
        BusinessPlanId = businessPlanId;
        SectionType = sectionType;
        AiGeneratedContent = aiGeneratedContent;
        UserEditedContent = userEditedContent;
        EditDistance = editDistance;
        EditRatio = editRatio;
        Language = language;
        PromptTemplateId = promptTemplateId;
        Industry = industry;
        PlanType = planType;
        CreatedAt = DateTime.UtcNow;
    }
}

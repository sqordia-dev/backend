using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities.ML;

/// <summary>
/// Persisted telemetry for every AI generation call.
/// Feeds ML quality prediction and cost optimization models.
/// </summary>
public class AICallTelemetryRecord : BaseEntity
{
    public Guid? BusinessPlanId { get; private set; }
    public Guid? PromptTemplateId { get; private set; }
    public string Provider { get; private set; } = null!;
    public string ModelUsed { get; private set; } = null!;
    public string? SectionType { get; private set; }
    public string PipelinePass { get; private set; } = null!;
    public int InputTokens { get; private set; }
    public int OutputTokens { get; private set; }
    public long LatencyMs { get; private set; }
    public string Language { get; private set; } = null!;
    public float Temperature { get; private set; }

    // Outcome fields — filled asynchronously when the user acts on the content
    public decimal? QualityScore { get; private set; }
    public bool? WasAccepted { get; private set; }
    public bool? WasRegenerated { get; private set; }
    public bool? WasEdited { get; private set; }
    public double? EditRatio { get; private set; }

    public DateTime CreatedAt { get; private set; }

    private AICallTelemetryRecord() { } // EF Core

    public AICallTelemetryRecord(
        string provider,
        string modelUsed,
        string? sectionType,
        string pipelinePass,
        int inputTokens,
        int outputTokens,
        long latencyMs,
        string language,
        float temperature,
        Guid? businessPlanId = null,
        Guid? promptTemplateId = null)
    {
        Provider = provider;
        ModelUsed = modelUsed;
        SectionType = sectionType;
        PipelinePass = pipelinePass;
        InputTokens = inputTokens;
        OutputTokens = outputTokens;
        LatencyMs = latencyMs;
        Language = language;
        Temperature = temperature;
        BusinessPlanId = businessPlanId;
        PromptTemplateId = promptTemplateId;
        CreatedAt = DateTime.UtcNow;
    }

    public void RecordOutcome(bool? wasAccepted, bool? wasRegenerated, bool? wasEdited, double? editRatio, decimal? qualityScore)
    {
        WasAccepted = wasAccepted;
        WasRegenerated = wasRegenerated;
        WasEdited = wasEdited;
        EditRatio = editRatio;
        QualityScore = qualityScore;
    }

    public int TotalTokens => InputTokens + OutputTokens;
}

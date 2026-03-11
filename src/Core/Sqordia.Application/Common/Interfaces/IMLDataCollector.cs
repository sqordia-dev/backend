using Sqordia.Domain.Entities.ML;

namespace Sqordia.Application.Common.Interfaces;

/// <summary>
/// Collects ML training signals: AI call telemetry and section edit diffs.
/// </summary>
public interface IMLDataCollector
{
    /// <summary>Persists an AI call telemetry record to the database.</summary>
    Task<Guid> RecordAICallAsync(
        AICallTelemetryRecord record,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a section edit by computing word-level diff between AI-generated and user-edited content.
    /// </summary>
    Task RecordSectionEditAsync(
        Guid businessPlanId,
        string sectionType,
        string aiGeneratedContent,
        string userEditedContent,
        string language,
        Guid? promptTemplateId = null,
        string? industry = null,
        string? planType = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the outcome of a previously recorded AI call (accepted, regenerated, edited, quality score).
    /// </summary>
    Task UpdateCallOutcomeAsync(
        Guid telemetryRecordId,
        bool? wasAccepted,
        bool? wasRegenerated,
        bool? wasEdited,
        double? editRatio,
        decimal? qualityScore,
        CancellationToken cancellationToken = default);
}

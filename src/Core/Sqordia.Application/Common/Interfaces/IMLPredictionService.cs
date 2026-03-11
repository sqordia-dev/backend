namespace Sqordia.Application.Common.Interfaces;

/// <summary>
/// Calls the Python ML service for quality predictions, prompt recommendations,
/// learned preferences, and quality drift monitoring.
/// </summary>
public interface IMLPredictionService
{
    /// <summary>Predicts quality score for a generated section before showing it to the user.</summary>
    Task<QualityPrediction> PredictQualityAsync(
        QualityPredictionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Recommends the best prompt variant for a given context using a multi-armed bandit.</summary>
    Task<PromptRecommendation> RecommendPromptAsync(
        PromptRecommendationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Gets learned preferences for a section type and optional industry filter.</summary>
    Task<List<LearnedPreferenceDto>> GetLearnedPreferencesAsync(
        string sectionType,
        string? industry = null,
        string language = "fr",
        CancellationToken cancellationToken = default);

    /// <summary>Checks for quality drift across sections and models.</summary>
    Task<QualityDriftReport> CheckQualityDriftAsync(
        CancellationToken cancellationToken = default);

    /// <summary>Triggers a training run for the ML models.</summary>
    Task<TrainingResult> TriggerTrainingAsync(
        CancellationToken cancellationToken = default);
}

// --- Request/Response DTOs ---

public record QualityPredictionRequest(
    string SectionType,
    string? Industry,
    string PlanType,
    string Language,
    int WordCount,
    float Temperature,
    string Provider,
    string Model,
    int InputTokens,
    int OutputTokens,
    double QuestionnaireCompleteness,
    bool HasBusinessBrief);

public record QualityPrediction(
    double PredictedScore,
    double Confidence,
    bool ShouldRegenerate,
    string? Reason);

public record PromptRecommendationRequest(
    string SectionType,
    string? Industry,
    string PlanType,
    string Language);

public record PromptRecommendation(
    Guid? RecommendedPromptTemplateId,
    double ExpectedQuality,
    string Strategy); // "bandit", "fallback", "insufficient_data"

public record LearnedPreferenceDto(
    string SectionType,
    string PreferenceType,
    string PreferenceJson,
    int SampleCount,
    double Confidence);

public record QualityDriftReport(
    bool HasDrift,
    List<DriftAlert> Alerts,
    DateTime CheckedAt);

public record DriftAlert(
    string SectionType,
    string Metric,
    double CurrentValue,
    double BaselineValue,
    double DeltaPercent,
    string Severity); // "warning", "critical"

public record TrainingResult(
    bool Success,
    string? ModelVersion,
    int TrainingSamples,
    Dictionary<string, double> Metrics,
    string? Error);

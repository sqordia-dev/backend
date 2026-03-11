namespace Sqordia.Application.Common.Models;

/// <summary>
/// Result from an AI generation call, including content and metadata.
/// </summary>
public record AICallResult(
    string Content,
    int InputTokens,
    int OutputTokens,
    long LatencyMs,
    string ModelUsed);

/// <summary>
/// Telemetry data for a single AI call, used for observability and optimization.
/// </summary>
public record AICallTelemetry(
    Guid? PromptTemplateId,
    string Provider,
    string ModelUsed,
    int InputTokens,
    int OutputTokens,
    long LatencyMs,
    string? SectionType,
    string Language,
    string PipelinePass,
    DateTime Timestamp,
    float Temperature = 0.6f);

using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Domain.Entities.ML;

namespace Sqordia.Infrastructure.Services;

/// <summary>
/// Logs AI call telemetry as structured log entries AND persists to the database
/// for ML training and quality drift monitoring.
/// </summary>
public class AITelemetryService : IAITelemetryService
{
    private readonly IMLDataCollector _dataCollector;
    private readonly ILogger<AITelemetryService> _logger;

    public AITelemetryService(IMLDataCollector dataCollector, ILogger<AITelemetryService> logger)
    {
        _dataCollector = dataCollector;
        _logger = logger;
    }

    public async Task LogCallAsync(AICallTelemetry telemetry, CancellationToken cancellationToken = default)
    {
        // Structured log (always)
        _logger.LogInformation(
            "AI Call: Provider={Provider} Model={Model} Section={Section} Pass={Pass} " +
            "InputTokens={InputTokens} OutputTokens={OutputTokens} LatencyMs={LatencyMs} " +
            "Language={Language} PromptId={PromptId}",
            telemetry.Provider,
            telemetry.ModelUsed,
            telemetry.SectionType ?? "N/A",
            telemetry.PipelinePass,
            telemetry.InputTokens,
            telemetry.OutputTokens,
            telemetry.LatencyMs,
            telemetry.Language,
            telemetry.PromptTemplateId?.ToString() ?? "fallback");

        // Persist to DB for ML training
        try
        {
            var record = new AICallTelemetryRecord(
                provider: telemetry.Provider,
                modelUsed: telemetry.ModelUsed,
                sectionType: telemetry.SectionType,
                pipelinePass: telemetry.PipelinePass,
                inputTokens: telemetry.InputTokens,
                outputTokens: telemetry.OutputTokens,
                latencyMs: telemetry.LatencyMs,
                language: telemetry.Language,
                temperature: telemetry.Temperature,
                promptTemplateId: telemetry.PromptTemplateId);

            await _dataCollector.RecordAICallAsync(record, cancellationToken);
        }
        catch (Exception ex)
        {
            // Telemetry persistence should never break the main flow
            _logger.LogWarning(ex, "Failed to persist AI call telemetry to database");
        }
    }
}

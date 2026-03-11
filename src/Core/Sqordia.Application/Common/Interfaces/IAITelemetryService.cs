using Sqordia.Application.Common.Models;

namespace Sqordia.Application.Common.Interfaces;

/// <summary>
/// Service for logging AI call telemetry data for observability and optimization.
/// </summary>
public interface IAITelemetryService
{
    /// <summary>
    /// Logs telemetry data for a single AI call.
    /// </summary>
    Task LogCallAsync(AICallTelemetry telemetry, CancellationToken cancellationToken = default);
}

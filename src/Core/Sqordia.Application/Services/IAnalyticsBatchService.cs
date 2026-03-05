using Sqordia.Application.Common.Models;

namespace Sqordia.Application.Services;

/// <summary>
/// Service for batch AI analytics processing
/// </summary>
public interface IAnalyticsBatchService
{
    /// <summary>
    /// Run batch analysis for all insight types
    /// </summary>
    Task<Result> RunBatchAnalysisAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the latest insights (one per type)
    /// </summary>
    Task<Result<List<AnalyticsInsightDto>>> GetLatestInsightsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get insight history for a specific type
    /// </summary>
    Task<Result<List<AnalyticsInsightDto>>> GetInsightHistoryAsync(string insightType, int count = 10, CancellationToken cancellationToken = default);
}

public class AnalyticsInsightDto
{
    public Guid Id { get; set; }
    public string InsightType { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
    public string ModelUsed { get; set; } = string.Empty;
    public int TokensUsed { get; set; }
    public bool IsLatest { get; set; }
    public DateTime GeneratedAt { get; set; }
}

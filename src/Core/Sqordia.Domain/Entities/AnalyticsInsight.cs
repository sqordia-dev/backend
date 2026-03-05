using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities;

/// <summary>
/// AI-generated analytics insight from batch processing
/// </summary>
public class AnalyticsInsight : BaseAuditableEntity
{
    public string InsightType { get; set; } = string.Empty; // user_signups, ai_usage, feature_flags, anomalies
    public string Content { get; set; } = string.Empty; // Markdown content
    public string Period { get; set; } = string.Empty; // e.g. "2026-03-05"
    public string? MetadataJson { get; set; }
    public string ModelUsed { get; set; } = string.Empty;
    public int TokensUsed { get; set; }
    public bool IsLatest { get; set; }
}

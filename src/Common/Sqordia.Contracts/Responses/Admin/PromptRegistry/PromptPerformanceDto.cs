namespace Sqordia.Contracts.Responses.Admin.PromptRegistry;

/// <summary>
/// Detailed performance metrics for a single prompt template
/// </summary>
public class PromptPerformanceDto
{
    public Guid PromptTemplateId { get; set; }
    public string PromptName { get; set; } = string.Empty;

    // Aggregate totals
    public int TotalUsageCount { get; set; }
    public int TotalEditCount { get; set; }
    public int TotalRegenerateCount { get; set; }
    public int TotalAcceptCount { get; set; }
    public int TotalRatingCount { get; set; }
    public double AverageRating { get; set; }

    // Calculated rates
    public double EditRate { get; set; }
    public double RegenerateRate { get; set; }
    public double AcceptanceRate { get; set; }

    // Time-series data for charts
    public List<PerformancePeriodDto> Periods { get; set; } = new();
}

/// <summary>
/// Performance metrics for a specific time period
/// </summary>
public class PerformancePeriodDto
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public int UsageCount { get; set; }
    public int EditCount { get; set; }
    public int RegenerateCount { get; set; }
    public int AcceptCount { get; set; }
    public int RatingCount { get; set; }
    public double AverageRating { get; set; }
    public double EditRate { get; set; }
    public double RegenerateRate { get; set; }
    public double AcceptanceRate { get; set; }
}

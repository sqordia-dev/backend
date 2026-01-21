namespace Sqordia.Contracts.Responses.V2.StrategyMap;

/// <summary>
/// Response containing strategy map data
/// </summary>
public class StrategyMapResponse
{
    public Guid BusinessPlanId { get; set; }
    public string? StrategyMapJson { get; set; }
    public decimal? ReadinessScore { get; set; }
    public FinancialHealthMetricsResponse? HealthMetrics { get; set; }
    public DateTime? LastUpdated { get; set; }
}

/// <summary>
/// Financial health metrics for the strategy map
/// </summary>
public class FinancialHealthMetricsResponse
{
    public int? PivotPointMonth { get; set; }
    public int? RunwayMonths { get; set; }
    public decimal? MonthlyBurnRate { get; set; }
    public decimal? TargetCAC { get; set; }
}

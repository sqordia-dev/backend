namespace Sqordia.Domain.ValueObjects;

/// <summary>
/// Financial health metrics for business plan readiness assessment
/// </summary>
public record FinancialHealthMetrics
{
    /// <summary>
    /// Month when business reaches break-even (pivot point)
    /// </summary>
    public int? PivotPointMonth { get; init; }

    /// <summary>
    /// Number of months the business can operate with current funding
    /// </summary>
    public int? RunwayMonths { get; init; }

    /// <summary>
    /// Monthly cash burn rate
    /// </summary>
    public decimal? MonthlyBurnRate { get; init; }

    /// <summary>
    /// Target Customer Acquisition Cost
    /// </summary>
    public decimal? TargetCAC { get; init; }

    /// <summary>
    /// Default constructor for EF Core
    /// </summary>
    public FinancialHealthMetrics() { }

    /// <summary>
    /// Creates a new FinancialHealthMetrics instance
    /// </summary>
    public FinancialHealthMetrics(int? pivotPointMonth, int? runwayMonths, decimal? monthlyBurnRate, decimal? targetCAC)
    {
        PivotPointMonth = pivotPointMonth;
        RunwayMonths = runwayMonths;
        MonthlyBurnRate = monthlyBurnRate;
        TargetCAC = targetCAC;
    }

    /// <summary>
    /// Returns an empty FinancialHealthMetrics instance
    /// </summary>
    public static FinancialHealthMetrics Empty => new();

    /// <summary>
    /// Checks if any metrics are populated
    /// </summary>
    public bool HasData => PivotPointMonth.HasValue || RunwayMonths.HasValue ||
                           MonthlyBurnRate.HasValue || TargetCAC.HasValue;
}

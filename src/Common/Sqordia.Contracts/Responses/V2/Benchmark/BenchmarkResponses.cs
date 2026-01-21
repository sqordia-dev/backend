namespace Sqordia.Contracts.Responses.V2.Benchmark;

/// <summary>
/// Comparison of business plan metrics against industry benchmarks
/// </summary>
public class BenchmarkComparisonResponse
{
    public Guid BusinessPlanId { get; set; }
    public required string IndustryCode { get; set; }
    public required string IndustryName { get; set; }
    public required List<MetricComparison> Comparisons { get; set; }
    public decimal OverallPerformanceScore { get; set; }
    public required string PerformanceLevel { get; set; }
    public required List<string> Insights { get; set; }
    public required List<string> Recommendations { get; set; }
    public DateTime GeneratedAt { get; set; }
}

/// <summary>
/// Comparison of a single metric
/// </summary>
public class MetricComparison
{
    public required string MetricName { get; set; }
    public required string MetricNameFR { get; set; }
    public decimal? ActualValue { get; set; }
    public decimal BenchmarkLow { get; set; }
    public decimal BenchmarkMedian { get; set; }
    public decimal BenchmarkHigh { get; set; }
    public required string Unit { get; set; }
    public required string Performance { get; set; }
    public decimal PercentileRank { get; set; }
}

/// <summary>
/// Industry benchmark data
/// </summary>
public class IndustryBenchmarkResponse
{
    public required string IndustryCode { get; set; }
    public required string IndustryName { get; set; }
    public required string IndustryNameFR { get; set; }
    public required string Description { get; set; }
    public required string DescriptionFR { get; set; }
    public required List<BenchmarkMetric> Metrics { get; set; }
    public string? DataSource { get; set; }
    public int? DataYear { get; set; }
}

/// <summary>
/// A single benchmark metric definition
/// </summary>
public class BenchmarkMetric
{
    public required string MetricName { get; set; }
    public required string MetricNameFR { get; set; }
    public required string Category { get; set; }
    public decimal Low { get; set; }
    public decimal Median { get; set; }
    public decimal High { get; set; }
    public required string Unit { get; set; }
    public string? Description { get; set; }
    public string? DescriptionFR { get; set; }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Services.V2;
using Sqordia.Contracts.Responses.V2.Benchmark;
using System.Reflection;
using System.Text.Json;

namespace Sqordia.Infrastructure.Services;

/// <summary>
/// Financial benchmark comparison service
/// Compares business plan metrics against industry standards loaded from embedded JSON
/// </summary>
public class FinancialBenchmarkService : IFinancialBenchmarkService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<FinancialBenchmarkService> _logger;

    // Static cache for benchmarks - loaded once per application lifetime
    private static readonly Lazy<List<IndustryBenchmarkResponse>> _benchmarksCache =
        new Lazy<List<IndustryBenchmarkResponse>>(LoadBenchmarksStatic);

    public FinancialBenchmarkService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<FinancialBenchmarkService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public Task<Result<BenchmarkComparisonResponse>> CompareToBenchmarksAsync(
        Guid businessPlanId,
        string? industryCode = null,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(async () =>
        {
            try
            {
                _logger.LogInformation("Comparing business plan {PlanId} to benchmarks for industry {Industry}",
                    businessPlanId, industryCode ?? "auto-detect");

                var businessPlan = await GetBusinessPlanWithAccessCheckAsync(businessPlanId, cancellationToken);
                if (businessPlan == null)
                {
                    return Result.Failure<BenchmarkComparisonResponse>(
                        Error.NotFound("BusinessPlan.NotFound", "Business plan not found or access denied."));
                }

                // Auto-detect industry if not provided
                var industry = industryCode ?? DetectIndustry(businessPlan);
                var benchmark = _benchmarksCache.Value.FirstOrDefault(b =>
                    b.IndustryCode.Equals(industry, StringComparison.OrdinalIgnoreCase));

                if (benchmark == null)
                {
                    // Default to consulting if industry not found
                    benchmark = _benchmarksCache.Value.First(b => b.IndustryCode == "CONSULTING");
                    industry = "CONSULTING";
                }

                // Extract metrics from business plan and compare
                var comparisons = CompareMetrics(businessPlan, benchmark);
                var overallScore = CalculateOverallScore(comparisons);
                var insights = GenerateInsights(comparisons, benchmark);
                var recommendations = GenerateRecommendations(comparisons, benchmark);

                var response = new BenchmarkComparisonResponse
                {
                    BusinessPlanId = businessPlanId,
                    IndustryCode = benchmark.IndustryCode,
                    IndustryName = benchmark.IndustryName,
                    Comparisons = comparisons,
                    OverallPerformanceScore = overallScore,
                    PerformanceLevel = GetPerformanceLevel(overallScore),
                    Insights = insights,
                    Recommendations = recommendations,
                    GeneratedAt = DateTime.UtcNow
                };

                _logger.LogInformation("Benchmark comparison completed with score {Score}", overallScore);

                return Result.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error comparing business plan {PlanId} to benchmarks", businessPlanId);
                return Result.Failure<BenchmarkComparisonResponse>(
                    Error.InternalServerError("Benchmark.ComparisonError", "An error occurred during benchmark comparison."));
            }
        }, cancellationToken);
    }

    public Task<Result<IEnumerable<IndustryBenchmarkResponse>>> GetAvailableBenchmarksAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting available industry benchmarks");
            return Task.FromResult(Result.Success(_benchmarksCache.Value.AsEnumerable()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available benchmarks");
            return Task.FromResult(Result.Failure<IEnumerable<IndustryBenchmarkResponse>>(
                Error.InternalServerError("Benchmark.LoadError", "An error occurred loading benchmarks.")));
        }
    }

    public Task<Result<IndustryBenchmarkResponse>> GetBenchmarkByIndustryAsync(
        string industryCode,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting benchmark for industry {Industry}", industryCode);

            var benchmark = _benchmarksCache.Value.FirstOrDefault(b =>
                b.IndustryCode.Equals(industryCode, StringComparison.OrdinalIgnoreCase));

            if (benchmark == null)
            {
                return Task.FromResult(Result.Failure<IndustryBenchmarkResponse>(
                    Error.NotFound("Benchmark.NotFound", $"No benchmark found for industry '{industryCode}'.")));
            }

            return Task.FromResult(Result.Success(benchmark));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting benchmark for industry {Industry}", industryCode);
            return Task.FromResult(Result.Failure<IndustryBenchmarkResponse>(
                Error.InternalServerError("Benchmark.GetError", "An error occurred getting the benchmark.")));
        }
    }

    private static List<IndustryBenchmarkResponse> LoadBenchmarksStatic()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourcePath = "Sqordia.Infrastructure.Data.IndustryBenchmarks.json";

            using var stream = assembly.GetManifestResourceStream(resourcePath);

            if (stream == null)
            {
                // Fallback: try to load from file
                var filePath = Path.Combine(
                    Path.GetDirectoryName(assembly.Location) ?? "",
                    "Data",
                    "IndustryBenchmarks.json");

                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    return ParseBenchmarks(json);
                }

                // Could not find IndustryBenchmarks.json, using default benchmarks
                return GetDefaultBenchmarks();
            }

            using var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();
            return ParseBenchmarks(content);
        }
        catch (Exception)
        {
            // Error loading benchmarks from JSON file, using defaults
            return GetDefaultBenchmarks();
        }
    }

    private static List<IndustryBenchmarkResponse> ParseBenchmarks(string json)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var root = JsonSerializer.Deserialize<BenchmarkRoot>(json, options);
        return root?.Benchmarks ?? new List<IndustryBenchmarkResponse>();
    }

    private static List<IndustryBenchmarkResponse> GetDefaultBenchmarks()
    {
        return new List<IndustryBenchmarkResponse>
        {
            new()
            {
                IndustryCode = "CONSULTING",
                IndustryName = "Business Consulting",
                IndustryNameFR = "Consultation d'affaires",
                Description = "Professional services and advisory firms",
                DescriptionFR = "Services professionnels et cabinets de conseil",
                Metrics = new List<BenchmarkMetric>
                {
                    new()
                    {
                        MetricName = "Gross Margin",
                        MetricNameFR = "Marge brute",
                        Category = "Profitability",
                        Low = 35,
                        Median = 50,
                        High = 65,
                        Unit = "%"
                    }
                }
            }
        };
    }

    private async Task<Domain.Entities.BusinessPlan.BusinessPlan?> GetBusinessPlanWithAccessCheckAsync(
        Guid businessPlanId,
        CancellationToken cancellationToken)
    {
        var currentUserIdString = _currentUserService.UserId;
        if (string.IsNullOrEmpty(currentUserIdString) || !Guid.TryParse(currentUserIdString, out var currentUserId))
        {
            return null;
        }

        var businessPlan = await _context.BusinessPlans
            .FirstOrDefaultAsync(bp => bp.Id == businessPlanId && !bp.IsDeleted, cancellationToken);

        if (businessPlan == null)
        {
            return null;
        }

        var isMember = await _context.OrganizationMembers
            .AnyAsync(om => om.OrganizationId == businessPlan.OrganizationId &&
                           om.UserId == currentUserId &&
                           om.IsActive, cancellationToken);

        return isMember ? businessPlan : null;
    }

    private static string DetectIndustry(Domain.Entities.BusinessPlan.BusinessPlan plan)
    {
        var content = string.Join(" ",
            plan.ExecutiveSummary ?? "",
            plan.MarketAnalysis ?? "",
            plan.BusinessModel ?? "").ToLower();

        if (content.Contains("saas") || content.Contains("software") || content.Contains("logiciel"))
            return "SAAS";
        if (content.Contains("restaurant") || content.Contains("food") || content.Contains("cuisine"))
            return "RESTAURANT";
        if (content.Contains("retail") || content.Contains("store") || content.Contains("magasin"))
            return "RETAIL";
        if (content.Contains("nonprofit") || content.Contains("obnl") || content.Contains("charity"))
            return "NONPROFIT";
        if (content.Contains("manufacturing") || content.Contains("production") || content.Contains("fabrication"))
            return "MANUFACTURING";

        return "CONSULTING"; // Default
    }

    private static List<MetricComparison> CompareMetrics(
        Domain.Entities.BusinessPlan.BusinessPlan plan,
        IndustryBenchmarkResponse benchmark)
    {
        var comparisons = new List<MetricComparison>();

        // Extract what metrics we can from the business plan
        // This is a simplified implementation - in production, you'd parse financial projections
        foreach (var metric in benchmark.Metrics.Take(3)) // Compare top 3 metrics
        {
            var actualValue = ExtractMetricValue(plan, metric.MetricName);

            var performance = actualValue.HasValue
                ? DeterminePerformance(actualValue.Value, metric.Low, metric.Median, metric.High)
                : "Unknown";

            var percentile = actualValue.HasValue
                ? CalculatePercentile(actualValue.Value, metric.Low, metric.Median, metric.High)
                : 50m;

            comparisons.Add(new MetricComparison
            {
                MetricName = metric.MetricName,
                MetricNameFR = metric.MetricNameFR,
                ActualValue = actualValue,
                BenchmarkLow = metric.Low,
                BenchmarkMedian = metric.Median,
                BenchmarkHigh = metric.High,
                Unit = metric.Unit,
                Performance = performance,
                PercentileRank = percentile
            });
        }

        return comparisons;
    }

    private static decimal? ExtractMetricValue(Domain.Entities.BusinessPlan.BusinessPlan plan, string metricName)
    {
        // This is a simplified extraction - in production, you'd parse financial data
        // For now, return null to indicate the metric couldn't be extracted
        if (plan.HealthMetrics != null)
        {
            if (metricName.Contains("CAC", StringComparison.OrdinalIgnoreCase))
                return plan.HealthMetrics.TargetCAC;
            if (metricName.Contains("Burn", StringComparison.OrdinalIgnoreCase))
                return plan.HealthMetrics.MonthlyBurnRate;
        }

        return null;
    }

    private static string DeterminePerformance(decimal value, decimal low, decimal median, decimal high)
    {
        if (value >= high) return "Excellent";
        if (value >= median) return "Good";
        if (value >= low) return "Below Average";
        return "Needs Improvement";
    }

    private static decimal CalculatePercentile(decimal value, decimal low, decimal median, decimal high)
    {
        if (value <= low) return 25;
        if (value <= median) return 25 + ((value - low) / (median - low) * 25);
        if (value <= high) return 50 + ((value - median) / (high - median) * 25);
        return 90;
    }

    private static decimal CalculateOverallScore(List<MetricComparison> comparisons)
    {
        var validComparisons = comparisons.Where(c => c.ActualValue.HasValue).ToList();
        if (!validComparisons.Any())
            return 50; // Default to median if no data

        return validComparisons.Average(c => c.PercentileRank);
    }

    private static string GetPerformanceLevel(decimal score)
    {
        return score switch
        {
            >= 75 => "Above Average",
            >= 50 => "Average",
            >= 25 => "Below Average",
            _ => "Needs Improvement"
        };
    }

    private static List<string> GenerateInsights(
        List<MetricComparison> comparisons,
        IndustryBenchmarkResponse benchmark)
    {
        var insights = new List<string>();

        var excellentMetrics = comparisons.Where(c => c.Performance == "Excellent").ToList();
        if (excellentMetrics.Any())
        {
            insights.Add($"Strong performance in: {string.Join(", ", excellentMetrics.Select(m => m.MetricName))}");
        }

        var needsImprovement = comparisons.Where(c => c.Performance == "Needs Improvement").ToList();
        if (needsImprovement.Any())
        {
            insights.Add($"Areas needing attention: {string.Join(", ", needsImprovement.Select(m => m.MetricName))}");
        }

        if (!comparisons.Any(c => c.ActualValue.HasValue))
        {
            insights.Add("Add financial projections to enable detailed benchmark comparisons");
        }

        insights.Add($"Compared against {benchmark.IndustryName} industry benchmarks ({benchmark.DataYear})");

        return insights;
    }

    private static List<string> GenerateRecommendations(
        List<MetricComparison> comparisons,
        IndustryBenchmarkResponse benchmark)
    {
        var recommendations = new List<string>();

        foreach (var comparison in comparisons.Where(c => c.Performance == "Needs Improvement" || c.Performance == "Below Average"))
        {
            recommendations.Add($"Improve {comparison.MetricName} - target: {comparison.BenchmarkMedian}{comparison.Unit} (industry median)");
        }

        if (!recommendations.Any())
        {
            recommendations.Add("Continue monitoring key metrics against industry benchmarks");
            recommendations.Add($"Consider expanding to achieve top quartile ({benchmark.Metrics.First().High}{benchmark.Metrics.First().Unit})");
        }

        return recommendations.Take(5).ToList();
    }

    private class BenchmarkRoot
    {
        public List<IndustryBenchmarkResponse> Benchmarks { get; set; } = new();
    }
}

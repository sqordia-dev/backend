using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Responses.Financial.Previsio;

namespace Sqordia.Application.Financial.Services.Previsio;

/// <summary>
/// AI-powered financial forecasting service.
/// Analyzes questionnaire answers and business context to generate
/// intelligent financial projections as starting points for the Previsio module.
/// </summary>
public interface IFinancialForecastingService
{
    /// <summary>
    /// Generates AI-powered financial projections from business plan context.
    /// Returns suggested products, expenses, payroll, and growth assumptions.
    /// </summary>
    Task<Result<FinancialForecastResponse>> GenerateForecastAsync(
        Guid businessPlanId,
        string language = "fr",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates AI-suggested sales volume projections for a specific product.
    /// Uses industry benchmarks, seasonality patterns, and business context.
    /// </summary>
    Task<Result<SalesVolumeForecastResponse>> ForecastSalesVolumeAsync(
        Guid businessPlanId,
        Guid salesProductId,
        string language = "fr",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Provides AI analysis and recommendations on existing financial statements.
    /// Identifies weaknesses, suggests improvements, and benchmarks against industry.
    /// </summary>
    Task<Result<FinancialAnalysisResponse>> AnalyzeFinancialsAsync(
        Guid businessPlanId,
        string language = "fr",
        CancellationToken cancellationToken = default);
}

// --- Response DTOs ---

public class FinancialForecastResponse
{
    public List<SuggestedProduct> SuggestedProducts { get; set; } = new();
    public List<SuggestedExpense> SuggestedExpenses { get; set; } = new();
    public List<SuggestedPayroll> SuggestedPayroll { get; set; } = new();
    public SuggestedGrowthAssumptions GrowthAssumptions { get; set; } = new();
    public string Reasoning { get; set; } = null!;
    public float Confidence { get; set; }
}

public class SuggestedProduct
{
    public string Name { get; set; } = null!;
    public decimal SuggestedPrice { get; set; }
    public decimal SuggestedMonthlyVolume { get; set; }
    public string PricingRationale { get; set; } = null!;
}

public class SuggestedExpense
{
    public string Category { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal MonthlyAmount { get; set; }
    public string Rationale { get; set; } = null!;
}

public class SuggestedPayroll
{
    public string JobTitle { get; set; } = null!;
    public string PayrollType { get; set; } = null!;
    public decimal AnnualSalary { get; set; }
    public int HeadCount { get; set; }
    public string Rationale { get; set; } = null!;
}

public class SuggestedGrowthAssumptions
{
    public decimal Year1GrowthRate { get; set; }
    public decimal Year2GrowthRate { get; set; }
    public decimal Year3GrowthRate { get; set; }
    public decimal PriceIndexation { get; set; }
    public decimal ExpenseIndexation { get; set; }
    public string Rationale { get; set; } = null!;
}

public class SalesVolumeForecastResponse
{
    public string ProductName { get; set; } = null!;
    public decimal[] MonthlyVolumes { get; set; } = new decimal[12];
    public string SeasonalityPattern { get; set; } = null!;
    public string Rationale { get; set; } = null!;
    public float Confidence { get; set; }
}

public class FinancialAnalysisResponse
{
    public decimal OverallHealthScore { get; set; }
    public List<FinancialInsight> Strengths { get; set; } = new();
    public List<FinancialInsight> Weaknesses { get; set; } = new();
    public List<FinancialInsight> Recommendations { get; set; } = new();
    public IndustryBenchmark? Benchmark { get; set; }
}

public class FinancialInsight
{
    public string Category { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Impact { get; set; } = null!;
}

public class IndustryBenchmark
{
    public string Industry { get; set; } = null!;
    public decimal TypicalGrossMargin { get; set; }
    public decimal TypicalNetMargin { get; set; }
    public int TypicalBreakEvenMonths { get; set; }
    public float DataConfidence { get; set; }
}

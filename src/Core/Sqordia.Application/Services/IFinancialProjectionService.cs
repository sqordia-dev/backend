using Sqordia.Application.Common.Models;
using Sqordia.Domain.Entities.BusinessPlan;

namespace Sqordia.Application.Services;

/// <summary>
/// Service for managing financial projections and calculations
/// </summary>
public interface IFinancialProjectionService
{
    /// <summary>
    /// Create financial projections for a business plan
    /// </summary>
    /// <param name="businessPlanId">Business plan ID</param>
    /// <param name="request">Financial projection creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created financial projection</returns>
    Task<Result<FinancialProjection>> CreateProjectionAsync(Guid businessPlanId, CreateFinancialProjectionRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing financial projection
    /// </summary>
    /// <param name="projectionId">Financial projection ID</param>
    /// <param name="request">Update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated financial projection</returns>
    Task<Result<FinancialProjection>> UpdateProjectionAsync(Guid projectionId, UpdateFinancialProjectionRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get financial projections for a business plan
    /// </summary>
    /// <param name="businessPlanId">Business plan ID</param>
    /// <param name="year">Optional year filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of financial projections</returns>
    Task<Result<List<FinancialProjection>>> GetProjectionsAsync(Guid businessPlanId, int? year = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a financial projection
    /// </summary>
    /// <param name="projectionId">Financial projection ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success result</returns>
    Task<Result> DeleteProjectionAsync(Guid projectionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate comprehensive financial projections based on input parameters
    /// </summary>
    /// <param name="businessPlanId">Business plan ID</param>
    /// <param name="request">Financial scenario request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Generated financial projections for multiple years</returns>
    Task<Result<List<FinancialProjection>>> GenerateProjectionScenarioAsync(Guid businessPlanId, GenerateProjectionScenarioRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate key financial metrics and ratios
    /// </summary>
    /// <param name="businessPlanId">Business plan ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Financial metrics summary</returns>
    Task<Result<FinancialMetrics>> GetFinancialMetricsAsync(Guid businessPlanId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Export financial projections to different formats
    /// </summary>
    /// <param name="businessPlanId">Business plan ID</param>
    /// <param name="format">Export format (excel, csv, json)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Exported financial data</returns>
    Task<Result<FinancialExportResult>> ExportProjectionsAsync(Guid businessPlanId, string format, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get available financial projection templates
    /// </summary>
    /// <returns>List of available templates</returns>
    Task<Result<List<FinancialProjectionTemplate>>> GetAvailableTemplatesAsync();

    /// <summary>
    /// Apply a financial template to generate projections
    /// </summary>
    /// <param name="businessPlanId">Business plan ID</param>
    /// <param name="templateId">Template ID</param>
    /// <param name="parameters">Template parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Generated projections from template</returns>
    Task<Result<List<FinancialProjection>>> ApplyTemplateAsync(Guid businessPlanId, string templateId, Dictionary<string, object> parameters, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate financial projection consistency and logic
    /// </summary>
    /// <param name="projection">Financial projection to validate</param>
    /// <returns>Validation results</returns>
    Task<Result<FinancialValidationResult>> ValidateProjectionAsync(FinancialProjection projection);
}

/// <summary>
/// Request model for creating financial projections
/// </summary>
public class CreateFinancialProjectionRequest
{
    public int Year { get; set; }
    public int? Month { get; set; }
    public int? Quarter { get; set; }
    public decimal? Revenue { get; set; }
    public decimal? RevenueGrowthRate { get; set; }
    public decimal? CostOfGoodsSold { get; set; }
    public decimal? OperatingExpenses { get; set; }
    public decimal? MarketingExpenses { get; set; }
    public decimal? RAndDExpenses { get; set; }
    public decimal? AdministrativeExpenses { get; set; }
    public decimal? OtherExpenses { get; set; }
    public decimal? CashFlow { get; set; }
    public decimal? CashBalance { get; set; }
    public int? Employees { get; set; }
    public int? Customers { get; set; }
    public int? UnitsSold { get; set; }
    public string? Notes { get; set; }
    public string? Assumptions { get; set; }
}

/// <summary>
/// Request model for updating financial projections
/// </summary>
public class UpdateFinancialProjectionRequest
{
    public decimal? Revenue { get; set; }
    public decimal? RevenueGrowthRate { get; set; }
    public decimal? CostOfGoodsSold { get; set; }
    public decimal? OperatingExpenses { get; set; }
    public decimal? MarketingExpenses { get; set; }
    public decimal? RAndDExpenses { get; set; }
    public decimal? AdministrativeExpenses { get; set; }
    public decimal? OtherExpenses { get; set; }
    public decimal? CashFlow { get; set; }
    public decimal? CashBalance { get; set; }
    public int? Employees { get; set; }
    public int? Customers { get; set; }
    public int? UnitsSold { get; set; }
    public string? Notes { get; set; }
    public string? Assumptions { get; set; }
}

/// <summary>
/// Request model for generating financial projection scenarios
/// </summary>
public class GenerateProjectionScenarioRequest
{
    public string ScenarioName { get; set; } = string.Empty;
    public int StartYear { get; set; } = DateTime.Now.Year;
    public int ProjectionYears { get; set; } = 3;
    public string Frequency { get; set; } = "yearly"; // yearly, quarterly, monthly

    // Initial values
    public decimal InitialRevenue { get; set; }
    public decimal AnnualRevenueGrowthRate { get; set; } = 0.20m; // 20% default
    public decimal CostOfGoodsSoldPercentage { get; set; } = 0.30m; // 30% of revenue
    public decimal OperatingExpensesPercentage { get; set; } = 0.25m; // 25% of revenue
    public decimal MarketingBudgetPercentage { get; set; } = 0.10m; // 10% of revenue

    // Business model assumptions
    public decimal PricePerUnit { get; set; }
    public int InitialCustomers { get; set; }
    public decimal CustomerGrowthRate { get; set; } = 0.15m; // 15% monthly
    public decimal CustomerChurnRate { get; set; } = 0.05m; // 5% monthly
    public decimal AverageRevenuePerCustomer { get; set; }

    // Operational assumptions
    public int InitialEmployees { get; set; } = 1;
    public decimal EmployeeGrowthRate { get; set; } = 0.10m; // 10% quarterly
    public decimal AverageSalaryPerEmployee { get; set; } = 60000m; // Annual

    // Cash flow assumptions
    public decimal InitialCashBalance { get; set; }
    public decimal MonthlyBurnRate { get; set; }
    public bool IncludeSeasonality { get; set; } = false;
    public List<decimal>? SeasonalityFactors { get; set; } // 12 factors for monthly seasonality
}

/// <summary>
/// Financial metrics calculation results
/// </summary>
public class FinancialMetrics
{
    public Guid BusinessPlanId { get; set; }
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;

    // Revenue metrics
    public decimal? TotalRevenue { get; set; }
    public decimal? AverageMonthlyRevenue { get; set; }
    public decimal? RevenueGrowthRate { get; set; }
    public decimal? AverageRevenuePerCustomer { get; set; }

    // Profitability metrics
    public decimal? GrossMargin { get; set; }
    public decimal? NetMargin { get; set; }
    public decimal? EBITDA { get; set; }
    public decimal? EBITDAMargin { get; set; }

    // Cash flow metrics
    public decimal? TotalCashFlow { get; set; }
    public decimal? AverageMonthlyCashFlow { get; set; }
    public decimal? CashRunwayMonths { get; set; }
    public decimal? BreakEvenPoint { get; set; }

    // Growth metrics
    public decimal? CustomerGrowthRate { get; set; }
    public decimal? EmployeeGrowthRate { get; set; }
    public decimal? RevenuePerEmployee { get; set; }

    // Financial health indicators
    public string? FinancialHealthScore { get; set; } // A, B, C, D, F
    public List<string> Insights { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public List<string> RiskFactors { get; set; } = new();
}

/// <summary>
/// Financial export result
/// </summary>
public class FinancialExportResult
{
    public byte[] FileData { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string Format { get; set; } = string.Empty;
    public DateTime ExportedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Financial projection template definition
/// </summary>
public class FinancialProjectionTemplate
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // SaaS, E-commerce, Service, etc.
    public bool IsDefault { get; set; }
    public Dictionary<string, object> DefaultParameters { get; set; } = new();
    public List<string> RequiredParameters { get; set; } = new();
    public string? PreviewImageUrl { get; set; }
}

/// <summary>
/// Financial validation result
/// </summary>
public class FinancialValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> Suggestions { get; set; } = new();
    public decimal? HealthScore { get; set; } // 0-100
}
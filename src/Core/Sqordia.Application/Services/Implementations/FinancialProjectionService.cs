using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Domain.Entities.BusinessPlan;
using System.Text;

namespace Sqordia.Application.Services.Implementations;

/// <summary>
/// Implementation of financial projection service with comprehensive calculation engine
/// </summary>
public class FinancialProjectionService : IFinancialProjectionService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<FinancialProjectionService> _logger;

    public FinancialProjectionService(
        IApplicationDbContext context,
        ILogger<FinancialProjectionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<FinancialProjection>> CreateProjectionAsync(Guid businessPlanId, CreateFinancialProjectionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating financial projection for business plan {BusinessPlanId}", businessPlanId);

            // Validate business plan exists and user has access
            var businessPlan = await _context.BusinessPlans
                .FirstOrDefaultAsync(bp => bp.Id == businessPlanId && !bp.IsDeleted, cancellationToken);

            if (businessPlan == null)
            {
                return Result.Failure<FinancialProjection>("Business plan not found or access denied");
            }

            // Check for duplicate projection
            var existingProjection = await _context.BusinessPlanFinancialProjections
                .FirstOrDefaultAsync(fp => fp.BusinessPlanId == businessPlanId &&
                                         fp.Year == request.Year &&
                                         fp.Month == request.Month &&
                                         fp.Quarter == request.Quarter, cancellationToken);

            if (existingProjection != null)
            {
                return Result.Failure<FinancialProjection>("A financial projection already exists for this period");
            }

            // Create new projection
            var projection = new FinancialProjection(businessPlanId, request.Year, request.Month, request.Quarter);

            // Set financial data
            if (request.Revenue.HasValue)
            {
                projection.SetRevenue(request.Revenue.Value, request.RevenueGrowthRate);
            }

            projection.SetCosts(
                cogs: request.CostOfGoodsSold,
                opex: request.OperatingExpenses,
                marketing: request.MarketingExpenses,
                rnd: request.RAndDExpenses,
                admin: request.AdministrativeExpenses,
                other: request.OtherExpenses);

            if (request.CashFlow.HasValue)
            {
                projection.SetCashFlow(request.CashFlow.Value, request.CashBalance);
            }

            projection.SetMetrics(
                employees: request.Employees,
                customers: request.Customers,
                unitsSold: request.UnitsSold);

            projection.SetNotes(request.Notes, request.Assumptions);

            _context.BusinessPlanFinancialProjections.Add(projection);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Financial projection created with ID {ProjectionId}", projection.Id);
            return Result.Success(projection);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating financial projection for business plan {BusinessPlanId}", businessPlanId);
            return Result.Failure<FinancialProjection>($"Failed to create financial projection: {ex.Message}");
        }
    }

    public async Task<Result<FinancialProjection>> UpdateProjectionAsync(Guid projectionId, UpdateFinancialProjectionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating financial projection {ProjectionId}", projectionId);

            var projection = await _context.BusinessPlanFinancialProjections
                .FirstOrDefaultAsync(fp => fp.Id == projectionId, cancellationToken);

            if (projection == null)
            {
                return Result.Failure<FinancialProjection>("Financial projection not found");
            }

            // Update financial data
            if (request.Revenue.HasValue)
            {
                projection.SetRevenue(request.Revenue.Value, request.RevenueGrowthRate);
            }

            projection.SetCosts(
                cogs: request.CostOfGoodsSold,
                opex: request.OperatingExpenses,
                marketing: request.MarketingExpenses,
                rnd: request.RAndDExpenses,
                admin: request.AdministrativeExpenses,
                other: request.OtherExpenses);

            if (request.CashFlow.HasValue)
            {
                projection.SetCashFlow(request.CashFlow.Value, request.CashBalance);
            }

            projection.SetMetrics(
                employees: request.Employees,
                customers: request.Customers,
                unitsSold: request.UnitsSold);

            projection.SetNotes(request.Notes, request.Assumptions);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Financial projection {ProjectionId} updated successfully", projectionId);
            return Result.Success(projection);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating financial projection {ProjectionId}", projectionId);
            return Result.Failure<FinancialProjection>($"Failed to update financial projection: {ex.Message}");
        }
    }

    public async Task<Result<List<FinancialProjection>>> GetProjectionsAsync(Guid businessPlanId, int? year = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.BusinessPlanFinancialProjections
                .Where(fp => fp.BusinessPlanId == businessPlanId);

            if (year.HasValue)
            {
                query = query.Where(fp => fp.Year == year.Value);
            }

            var projections = await query
                .OrderBy(fp => fp.Year)
                .ThenBy(fp => fp.Quarter)
                .ThenBy(fp => fp.Month)
                .ToListAsync(cancellationToken);

            return Result.Success(projections);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving financial projections for business plan {BusinessPlanId}", businessPlanId);
            return Result.Failure<List<FinancialProjection>>($"Failed to retrieve projections: {ex.Message}");
        }
    }

    public async Task<Result> DeleteProjectionAsync(Guid projectionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var projection = await _context.BusinessPlanFinancialProjections
                .FirstOrDefaultAsync(fp => fp.Id == projectionId, cancellationToken);

            if (projection == null)
            {
                return Result.Failure("Financial projection not found");
            }

            _context.BusinessPlanFinancialProjections.Remove(projection);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Financial projection {ProjectionId} deleted successfully", projectionId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting financial projection {ProjectionId}", projectionId);
            return Result.Failure($"Failed to delete financial projection: {ex.Message}");
        }
    }

    public async Task<Result<List<FinancialProjection>>> GenerateProjectionScenarioAsync(Guid businessPlanId, GenerateProjectionScenarioRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating financial projection scenario '{ScenarioName}' for business plan {BusinessPlanId}",
                request.ScenarioName, businessPlanId);

            // Validate business plan exists
            var businessPlan = await _context.BusinessPlans
                .FirstOrDefaultAsync(bp => bp.Id == businessPlanId && !bp.IsDeleted, cancellationToken);

            if (businessPlan == null)
            {
                return Result.Failure<List<FinancialProjection>>("Business plan not found or access denied");
            }

            var projections = new List<FinancialProjection>();

            // Generate projections based on frequency
            switch (request.Frequency.ToLower())
            {
                case "yearly":
                    projections = GenerateYearlyProjections(businessPlanId, request);
                    break;
                case "quarterly":
                    projections = GenerateQuarterlyProjections(businessPlanId, request);
                    break;
                case "monthly":
                    projections = GenerateMonthlyProjections(businessPlanId, request);
                    break;
                default:
                    return Result.Failure<List<FinancialProjection>>("Invalid frequency. Must be 'yearly', 'quarterly', or 'monthly'");
            }

            // Save all projections
            _context.BusinessPlanFinancialProjections.AddRange(projections);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Generated {Count} financial projections for scenario '{ScenarioName}'",
                projections.Count, request.ScenarioName);

            return Result.Success(projections);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating financial projection scenario for business plan {BusinessPlanId}", businessPlanId);
            return Result.Failure<List<FinancialProjection>>($"Failed to generate projection scenario: {ex.Message}");
        }
    }

    public async Task<Result<FinancialMetrics>> GetFinancialMetricsAsync(Guid businessPlanId, CancellationToken cancellationToken = default)
    {
        try
        {
            var projections = await _context.BusinessPlanFinancialProjections
                .Where(fp => fp.BusinessPlanId == businessPlanId)
                .OrderBy(fp => fp.Year)
                .ThenBy(fp => fp.Month ?? fp.Quarter ?? 0)
                .ToListAsync(cancellationToken);

            if (!projections.Any())
            {
                return Result.Failure<FinancialMetrics>("No financial projections found for this business plan");
            }

            var metrics = CalculateFinancialMetrics(businessPlanId, projections);
            return Result.Success(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating financial metrics for business plan {BusinessPlanId}", businessPlanId);
            return Result.Failure<FinancialMetrics>($"Failed to calculate financial metrics: {ex.Message}");
        }
    }

    public async Task<Result<FinancialExportResult>> ExportProjectionsAsync(Guid businessPlanId, string format, CancellationToken cancellationToken = default)
    {
        try
        {
            var projections = await _context.BusinessPlanFinancialProjections
                .Where(fp => fp.BusinessPlanId == businessPlanId)
                .OrderBy(fp => fp.Year)
                .ThenBy(fp => fp.Month ?? fp.Quarter ?? 0)
                .ToListAsync(cancellationToken);

            if (!projections.Any())
            {
                return Result.Failure<FinancialExportResult>("No financial projections found for export");
            }

            var exportResult = format.ToLower() switch
            {
                "csv" => ExportToCsv(projections, businessPlanId),
                "json" => ExportToJson(projections, businessPlanId),
                "excel" => ExportToExcel(projections, businessPlanId),
                _ => throw new ArgumentException($"Unsupported export format: {format}")
            };

            return Result.Success(exportResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting financial projections for business plan {BusinessPlanId}", businessPlanId);
            return Result.Failure<FinancialExportResult>($"Failed to export financial projections: {ex.Message}");
        }
    }

    public async Task<Result<List<FinancialProjectionTemplate>>> GetAvailableTemplatesAsync()
    {
        await Task.CompletedTask; // For future database-stored templates

        var templates = new List<FinancialProjectionTemplate>
        {
            new()
            {
                Id = "saas-startup",
                Name = "SaaS Startup",
                Description = "Financial projections for Software-as-a-Service startups with recurring revenue",
                Category = "SaaS",
                IsDefault = true,
                DefaultParameters = new Dictionary<string, object>
                {
                    ["monthly_recurring_revenue"] = 10000m,
                    ["churn_rate"] = 0.05m,
                    ["customer_acquisition_cost"] = 100m,
                    ["annual_growth_rate"] = 1.0m
                },
                RequiredParameters = new List<string> { "monthly_recurring_revenue", "churn_rate" }
            },
            new()
            {
                Id = "ecommerce",
                Name = "E-commerce Business",
                Description = "Financial projections for online retail and e-commerce businesses",
                Category = "E-commerce",
                IsDefault = false,
                DefaultParameters = new Dictionary<string, object>
                {
                    ["average_order_value"] = 75m,
                    ["conversion_rate"] = 0.02m,
                    ["cost_of_goods_percentage"] = 0.40m,
                    ["marketing_budget_percentage"] = 0.15m
                },
                RequiredParameters = new List<string> { "average_order_value", "conversion_rate" }
            },
            new()
            {
                Id = "consulting",
                Name = "Consulting Services",
                Description = "Financial projections for professional services and consulting firms",
                Category = "Services",
                IsDefault = false,
                DefaultParameters = new Dictionary<string, object>
                {
                    ["hourly_rate"] = 150m,
                    ["billable_hours_per_month"] = 120,
                    ["team_size"] = 3,
                    ["overhead_percentage"] = 0.30m
                },
                RequiredParameters = new List<string> { "hourly_rate", "billable_hours_per_month" }
            },
            new()
            {
                Id = "restaurant",
                Name = "Restaurant Business",
                Description = "Financial projections for restaurants and food service businesses",
                Category = "Food & Beverage",
                IsDefault = false,
                DefaultParameters = new Dictionary<string, object>
                {
                    ["average_check_size"] = 35m,
                    ["covers_per_day"] = 80,
                    ["food_cost_percentage"] = 0.28m,
                    ["labor_cost_percentage"] = 0.30m
                },
                RequiredParameters = new List<string> { "average_check_size", "covers_per_day" }
            }
        };

        return Result.Success(templates);
    }

    public async Task<Result<List<FinancialProjection>>> ApplyTemplateAsync(Guid businessPlanId, string templateId, Dictionary<string, object> parameters, CancellationToken cancellationToken = default)
    {
        try
        {
            var templatesResult = await GetAvailableTemplatesAsync();
            if (!templatesResult.IsSuccess)
            {
                return Result.Failure<List<FinancialProjection>>("Failed to load templates");
            }

            var template = templatesResult.Value!.FirstOrDefault(t => t.Id == templateId);
            if (template == null)
            {
                return Result.Failure<List<FinancialProjection>>("Template not found");
            }

            // Validate required parameters
            foreach (var requiredParam in template.RequiredParameters)
            {
                if (!parameters.ContainsKey(requiredParam))
                {
                    return Result.Failure<List<FinancialProjection>>($"Missing required parameter: {requiredParam}");
                }
            }

            // Generate projections based on template
            var projections = GenerateProjectionsFromTemplate(businessPlanId, template, parameters);

            // Save projections
            _context.BusinessPlanFinancialProjections.AddRange(projections);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Applied template '{TemplateId}' to business plan {BusinessPlanId}, generated {Count} projections",
                templateId, businessPlanId, projections.Count);

            return Result.Success(projections);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying template {TemplateId} to business plan {BusinessPlanId}", templateId, businessPlanId);
            return Result.Failure<List<FinancialProjection>>($"Failed to apply template: {ex.Message}");
        }
    }

    public async Task<Result<FinancialValidationResult>> ValidateProjectionAsync(FinancialProjection projection)
    {
        await Task.CompletedTask; // Synchronous validation

        var result = new FinancialValidationResult { IsValid = true };

        // Validation rules
        if (projection.Revenue.HasValue && projection.Revenue < 0)
        {
            result.Errors.Add("Revenue cannot be negative");
            result.IsValid = false;
        }

        if (projection.CostOfGoodsSold.HasValue && projection.Revenue.HasValue &&
            projection.CostOfGoodsSold > projection.Revenue)
        {
            result.Warnings.Add("Cost of Goods Sold exceeds Revenue, resulting in negative gross profit");
        }

        if (projection.NetIncome.HasValue && projection.NetIncome < 0)
        {
            result.Warnings.Add("Negative net income indicates unprofitable operations");
        }

        if (projection.CashFlow.HasValue && projection.CashFlow < 0)
        {
            result.Warnings.Add("Negative cash flow may indicate cash flow problems");
        }

        // Calculate health score
        result.HealthScore = CalculateProjectionHealthScore(projection);

        // Add suggestions based on analysis
        if (result.HealthScore < 60)
        {
            result.Suggestions.Add("Consider reviewing cost structure to improve profitability");
            result.Suggestions.Add("Explore opportunities to increase revenue or reduce expenses");
        }

        return Result.Success(result);
    }

    #region Private Helper Methods

    private List<FinancialProjection> GenerateYearlyProjections(Guid businessPlanId, GenerateProjectionScenarioRequest request)
    {
        var projections = new List<FinancialProjection>();
        var currentRevenue = request.InitialRevenue;
        var currentCustomers = request.InitialCustomers;
        var currentEmployees = request.InitialEmployees;
        var currentCashBalance = request.InitialCashBalance;

        for (int year = 0; year < request.ProjectionYears; year++)
        {
            var projectionYear = request.StartYear + year;
            var projection = new FinancialProjection(businessPlanId, projectionYear);

            // Calculate revenue with growth
            if (year > 0)
            {
                currentRevenue *= (1 + request.AnnualRevenueGrowthRate);
            }

            // Calculate costs
            var cogs = currentRevenue * request.CostOfGoodsSoldPercentage;
            var opex = currentRevenue * request.OperatingExpensesPercentage;
            var marketing = currentRevenue * request.MarketingBudgetPercentage;
            var salaries = currentEmployees * request.AverageSalaryPerEmployee;

            // Set financial data
            projection.SetRevenue(currentRevenue, request.AnnualRevenueGrowthRate);
            projection.SetCosts(cogs: cogs, opex: opex, marketing: marketing, admin: salaries);

            // Calculate cash flow
            var netIncome = projection.NetIncome ?? 0;
            var cashFlow = netIncome; // Simplified
            currentCashBalance += cashFlow;
            projection.SetCashFlow(cashFlow, currentCashBalance);

            // Update metrics
            currentCustomers = (int)(currentCustomers * (1 + request.CustomerGrowthRate));
            currentEmployees = (int)(currentEmployees * (1 + request.EmployeeGrowthRate));
            projection.SetMetrics(employees: currentEmployees, customers: currentCustomers);

            projection.SetNotes($"Year {year + 1} projection",
                $"Generated using {request.ScenarioName} scenario with {request.AnnualRevenueGrowthRate:P0} annual growth");

            projections.Add(projection);
        }

        return projections;
    }

    private List<FinancialProjection> GenerateQuarterlyProjections(Guid businessPlanId, GenerateProjectionScenarioRequest request)
    {
        var projections = new List<FinancialProjection>();
        var quarterlyRevenue = request.InitialRevenue / 4; // Assume annual revenue divided by 4
        var quarterlyGrowthRate = Math.Pow(1 + (double)request.AnnualRevenueGrowthRate, 0.25) - 1; // Quarterly growth rate

        for (int year = 0; year < request.ProjectionYears; year++)
        {
            for (int quarter = 1; quarter <= 4; quarter++)
            {
                var projectionYear = request.StartYear + year;
                var projection = new FinancialProjection(businessPlanId, projectionYear, quarter: quarter);

                // Calculate quarterly metrics
                var quarterIndex = year * 4 + quarter - 1;
                var currentQuarterlyRevenue = quarterlyRevenue * (decimal)Math.Pow(1 + (double)quarterlyGrowthRate, quarterIndex);

                var cogs = currentQuarterlyRevenue * request.CostOfGoodsSoldPercentage;
                var opex = currentQuarterlyRevenue * request.OperatingExpensesPercentage / 4;
                var marketing = currentQuarterlyRevenue * request.MarketingBudgetPercentage;

                projection.SetRevenue(currentQuarterlyRevenue, (decimal)quarterlyGrowthRate);
                projection.SetCosts(cogs: cogs, opex: opex, marketing: marketing);

                projection.SetNotes($"Q{quarter} {projectionYear} projection",
                    $"Generated using {request.ScenarioName} scenario");

                projections.Add(projection);
            }
        }

        return projections;
    }

    private List<FinancialProjection> GenerateMonthlyProjections(Guid businessPlanId, GenerateProjectionScenarioRequest request)
    {
        var projections = new List<FinancialProjection>();
        var monthlyRevenue = request.InitialRevenue / 12; // Assume annual revenue divided by 12
        var monthlyGrowthRate = Math.Pow(1 + (double)request.AnnualRevenueGrowthRate, 1.0/12) - 1; // Monthly growth rate

        for (int year = 0; year < request.ProjectionYears; year++)
        {
            for (int month = 1; month <= 12; month++)
            {
                var projectionYear = request.StartYear + year;
                var projection = new FinancialProjection(businessPlanId, projectionYear, month: month);

                // Calculate monthly metrics
                var monthIndex = year * 12 + month - 1;
                var currentMonthlyRevenue = monthlyRevenue * (decimal)Math.Pow(1 + (double)monthlyGrowthRate, monthIndex);

                // Apply seasonality if specified
                if (request.IncludeSeasonality && request.SeasonalityFactors?.Count == 12)
                {
                    currentMonthlyRevenue *= request.SeasonalityFactors[month - 1];
                }

                var cogs = currentMonthlyRevenue * request.CostOfGoodsSoldPercentage;
                var opex = currentMonthlyRevenue * request.OperatingExpensesPercentage / 12;
                var marketing = currentMonthlyRevenue * request.MarketingBudgetPercentage;

                projection.SetRevenue(currentMonthlyRevenue, (decimal)monthlyGrowthRate);
                projection.SetCosts(cogs: cogs, opex: opex, marketing: marketing);

                projection.SetNotes($"{DateTime.ParseExact(month.ToString(), "M", null):MMMM} {projectionYear} projection",
                    $"Generated using {request.ScenarioName} scenario");

                projections.Add(projection);
            }
        }

        return projections;
    }

    private FinancialMetrics CalculateFinancialMetrics(Guid businessPlanId, List<FinancialProjection> projections)
    {
        var metrics = new FinancialMetrics { BusinessPlanId = businessPlanId };

        var revenueProjections = projections.Where(p => p.Revenue.HasValue).ToList();
        var profitProjections = projections.Where(p => p.NetIncome.HasValue).ToList();
        var cashFlowProjections = projections.Where(p => p.CashFlow.HasValue).ToList();

        // Revenue metrics
        if (revenueProjections.Any())
        {
            metrics.TotalRevenue = revenueProjections.Sum(p => p.Revenue!.Value);
            metrics.AverageMonthlyRevenue = metrics.TotalRevenue / revenueProjections.Count;

            if (revenueProjections.Count > 1)
            {
                var firstRevenue = revenueProjections.First().Revenue!.Value;
                var lastRevenue = revenueProjections.Last().Revenue!.Value;
                metrics.RevenueGrowthRate = firstRevenue != 0 ? (lastRevenue - firstRevenue) / firstRevenue : 0;
            }
        }

        // Profitability metrics
        if (profitProjections.Any())
        {
            var totalNetIncome = profitProjections.Sum(p => p.NetIncome!.Value);
            metrics.NetMargin = metrics.TotalRevenue > 0 ? totalNetIncome / metrics.TotalRevenue : 0;

            var grossProfitProjections = projections.Where(p => p.GrossProfit.HasValue).ToList();
            if (grossProfitProjections.Any())
            {
                var totalGrossProfit = grossProfitProjections.Sum(p => p.GrossProfit!.Value);
                metrics.GrossMargin = metrics.TotalRevenue > 0 ? totalGrossProfit / metrics.TotalRevenue : 0;
            }
        }

        // Cash flow metrics
        if (cashFlowProjections.Any())
        {
            metrics.TotalCashFlow = cashFlowProjections.Sum(p => p.CashFlow!.Value);
            metrics.AverageMonthlyCashFlow = metrics.TotalCashFlow / cashFlowProjections.Count;

            // Calculate cash runway (simplified)
            var negativeCashFlowMonths = cashFlowProjections.Where(p => p.CashFlow < 0).ToList();
            if (negativeCashFlowMonths.Any())
            {
                var averageMonthlyBurn = Math.Abs(negativeCashFlowMonths.Average(p => p.CashFlow!.Value));
                var currentCash = projections.LastOrDefault()?.CashBalance ?? 0;
                metrics.CashRunwayMonths = averageMonthlyBurn > 0 ? currentCash / averageMonthlyBurn : 0;
            }
        }

        // Financial health score (0-100)
        var healthScore = 50m; // Base score

        if (metrics.NetMargin > 0.1m) healthScore += 20; // Good profit margin
        if (metrics.GrossMargin > 0.4m) healthScore += 15; // Healthy gross margin
        if (metrics.RevenueGrowthRate > 0.2m) healthScore += 15; // Strong growth
        if (metrics.CashRunwayMonths > 12) healthScore += 10; // Good cash position

        metrics.FinancialHealthScore = healthScore switch
        {
            >= 90 => "A",
            >= 80 => "B",
            >= 70 => "C",
            >= 60 => "D",
            _ => "F"
        };

        // Generate insights and recommendations
        GenerateInsightsAndRecommendations(metrics);

        return metrics;
    }

    private void GenerateInsightsAndRecommendations(FinancialMetrics metrics)
    {
        // Insights
        if (metrics.RevenueGrowthRate > 0.5m)
        {
            metrics.Insights.Add("Exceptional revenue growth trajectory");
        }
        else if (metrics.RevenueGrowthRate > 0.2m)
        {
            metrics.Insights.Add("Strong revenue growth");
        }
        else if (metrics.RevenueGrowthRate < 0)
        {
            metrics.Insights.Add("Revenue is declining");
            metrics.RiskFactors.Add("Negative revenue growth");
        }

        if (metrics.NetMargin > 0.15m)
        {
            metrics.Insights.Add("Excellent profitability margins");
        }
        else if (metrics.NetMargin < 0)
        {
            metrics.Insights.Add("Business is currently unprofitable");
            metrics.RiskFactors.Add("Negative profit margins");
        }

        if (metrics.CashRunwayMonths < 6)
        {
            metrics.RiskFactors.Add("Short cash runway - may need funding soon");
        }

        // Recommendations
        if (metrics.NetMargin < 0.1m)
        {
            metrics.Recommendations.Add("Focus on improving profit margins through cost optimization or pricing strategies");
        }

        if (metrics.GrossMargin < 0.3m)
        {
            metrics.Recommendations.Add("Review cost of goods sold - consider supplier negotiations or process improvements");
        }

        if (metrics.CashRunwayMonths < 12)
        {
            metrics.Recommendations.Add("Consider raising additional funding or improving cash flow");
        }

        if (metrics.RevenueGrowthRate < 0.1m)
        {
            metrics.Recommendations.Add("Explore strategies to accelerate revenue growth");
        }
    }

    private decimal CalculateProjectionHealthScore(FinancialProjection projection)
    {
        var score = 50m; // Base score

        // Revenue check
        if (projection.Revenue > 0) score += 10;

        // Profitability check
        if (projection.NetIncome > 0) score += 20;
        else if (projection.NetIncome < 0) score -= 10;

        // Cash flow check
        if (projection.CashFlow > 0) score += 15;
        else if (projection.CashFlow < 0) score -= 5;

        // Gross margin check
        if (projection.GrossProfit > 0 && projection.Revenue > 0)
        {
            var grossMargin = projection.GrossProfit.Value / projection.Revenue.Value;
            if (grossMargin > 0.5m) score += 15;
            else if (grossMargin > 0.3m) score += 10;
            else if (grossMargin > 0.1m) score += 5;
        }

        return Math.Max(0, Math.Min(100, score));
    }

    private List<FinancialProjection> GenerateProjectionsFromTemplate(Guid businessPlanId, FinancialProjectionTemplate template, Dictionary<string, object> parameters)
    {
        var projections = new List<FinancialProjection>();
        var startYear = DateTime.Now.Year;

        // Generate 3 years of projections based on template type
        switch (template.Id)
        {
            case "saas-startup":
                projections = GenerateSaaSProjections(businessPlanId, startYear, parameters);
                break;
            case "ecommerce":
                projections = GenerateEcommerceProjections(businessPlanId, startYear, parameters);
                break;
            case "consulting":
                projections = GenerateConsultingProjections(businessPlanId, startYear, parameters);
                break;
            case "restaurant":
                projections = GenerateRestaurantProjections(businessPlanId, startYear, parameters);
                break;
            default:
                // Generic template
                projections = GenerateGenericProjections(businessPlanId, startYear, parameters);
                break;
        }

        return projections;
    }

    private List<FinancialProjection> GenerateSaaSProjections(Guid businessPlanId, int startYear, Dictionary<string, object> parameters)
    {
        var projections = new List<FinancialProjection>();
        var mrr = Convert.ToDecimal(parameters["monthly_recurring_revenue"]);
        var churnRate = Convert.ToDecimal(parameters["churn_rate"]);
        var cac = Convert.ToDecimal(parameters.GetValueOrDefault("customer_acquisition_cost", 100m));
        var growthRate = Convert.ToDecimal(parameters.GetValueOrDefault("annual_growth_rate", 1.0m));

        for (int year = 0; year < 3; year++)
        {
            var projectionYear = startYear + year;
            var annualRevenue = mrr * 12 * (decimal)Math.Pow(1 + (double)growthRate, year);

            var projection = new FinancialProjection(businessPlanId, projectionYear);
            projection.SetRevenue(annualRevenue, growthRate);

            // SaaS specific costs
            var cogs = annualRevenue * 0.15m; // Low COGS for SaaS
            var opex = annualRevenue * 0.30m;
            var marketing = annualRevenue * 0.25m; // High marketing for growth

            projection.SetCosts(cogs: cogs, opex: opex, marketing: marketing);
            projection.SetNotes($"SaaS projection for {projectionYear}",
                $"Based on ${mrr:N0}/month MRR with {churnRate:P0} monthly churn");

            projections.Add(projection);
        }

        return projections;
    }

    private List<FinancialProjection> GenerateEcommerceProjections(Guid businessPlanId, int startYear, Dictionary<string, object> parameters)
    {
        var projections = new List<FinancialProjection>();
        var aov = Convert.ToDecimal(parameters["average_order_value"]);
        var conversionRate = Convert.ToDecimal(parameters["conversion_rate"]);
        var cogsPercentage = Convert.ToDecimal(parameters.GetValueOrDefault("cost_of_goods_percentage", 0.40m));

        for (int year = 0; year < 3; year++)
        {
            var projectionYear = startYear + year;

            // Assume growing traffic and orders
            var monthlyOrders = 1000 * (1 + year * 0.5); // 50% year-over-year growth
            var annualRevenue = (decimal)monthlyOrders * 12 * aov;

            var projection = new FinancialProjection(businessPlanId, projectionYear);
            projection.SetRevenue(annualRevenue, 0.50m);

            var cogs = annualRevenue * cogsPercentage;
            var opex = annualRevenue * 0.20m;
            var marketing = annualRevenue * 0.15m;

            projection.SetCosts(cogs: cogs, opex: opex, marketing: marketing);
            projection.SetNotes($"E-commerce projection for {projectionYear}",
                $"Based on ${aov:N0} AOV with {conversionRate:P1} conversion rate");

            projections.Add(projection);
        }

        return projections;
    }

    private List<FinancialProjection> GenerateConsultingProjections(Guid businessPlanId, int startYear, Dictionary<string, object> parameters)
    {
        var projections = new List<FinancialProjection>();
        var hourlyRate = Convert.ToDecimal(parameters["hourly_rate"]);
        var billableHoursPerMonth = Convert.ToInt32(parameters["billable_hours_per_month"]);
        var teamSize = Convert.ToInt32(parameters.GetValueOrDefault("team_size", 3));

        for (int year = 0; year < 3; year++)
        {
            var projectionYear = startYear + year;
            var currentTeamSize = teamSize + year; // Add one person per year
            var annualRevenue = hourlyRate * billableHoursPerMonth * 12 * currentTeamSize;

            var projection = new FinancialProjection(businessPlanId, projectionYear);
            projection.SetRevenue(annualRevenue, 0.30m);

            // Service business costs
            var cogs = 0m; // No COGS for consulting
            var salaries = currentTeamSize * 80000m; // Average salary
            var opex = annualRevenue * 0.15m;
            var marketing = annualRevenue * 0.05m; // Lower marketing for consulting

            projection.SetCosts(cogs: cogs, opex: opex, marketing: marketing, admin: salaries);
            projection.SetMetrics(employees: currentTeamSize);
            projection.SetNotes($"Consulting projection for {projectionYear}",
                $"Based on ${hourlyRate:N0}/hour rate with {billableHoursPerMonth} billable hours/month");

            projections.Add(projection);
        }

        return projections;
    }

    private List<FinancialProjection> GenerateRestaurantProjections(Guid businessPlanId, int startYear, Dictionary<string, object> parameters)
    {
        var projections = new List<FinancialProjection>();
        var averageCheckSize = Convert.ToDecimal(parameters["average_check_size"]);
        var coversPerDay = Convert.ToInt32(parameters["covers_per_day"]);
        var foodCostPercentage = Convert.ToDecimal(parameters.GetValueOrDefault("food_cost_percentage", 0.28m));
        var laborCostPercentage = Convert.ToDecimal(parameters.GetValueOrDefault("labor_cost_percentage", 0.30m));

        for (int year = 0; year < 3; year++)
        {
            var projectionYear = startYear + year;
            var currentCoversPerDay = coversPerDay * (1 + year * 0.1); // 10% growth per year
            var annualRevenue = (decimal)currentCoversPerDay * 365 * averageCheckSize;

            var projection = new FinancialProjection(businessPlanId, projectionYear);
            projection.SetRevenue(annualRevenue, 0.10m);

            var cogs = annualRevenue * foodCostPercentage;
            var laborCosts = annualRevenue * laborCostPercentage;
            var opex = annualRevenue * 0.25m; // Rent, utilities, etc.
            var marketing = annualRevenue * 0.03m; // Lower marketing for restaurants

            projection.SetCosts(cogs: cogs, opex: opex, marketing: marketing, admin: laborCosts);
            projection.SetNotes($"Restaurant projection for {projectionYear}",
                $"Based on ${averageCheckSize:N0} average check with {currentCoversPerDay:N0} covers/day");

            projections.Add(projection);
        }

        return projections;
    }

    private List<FinancialProjection> GenerateGenericProjections(Guid businessPlanId, int startYear, Dictionary<string, object> parameters)
    {
        var projections = new List<FinancialProjection>();
        var initialRevenue = Convert.ToDecimal(parameters.GetValueOrDefault("initial_revenue", 100000m));
        var growthRate = Convert.ToDecimal(parameters.GetValueOrDefault("growth_rate", 0.20m));

        for (int year = 0; year < 3; year++)
        {
            var projectionYear = startYear + year;
            var annualRevenue = initialRevenue * (decimal)Math.Pow(1 + (double)growthRate, year);

            var projection = new FinancialProjection(businessPlanId, projectionYear);
            projection.SetRevenue(annualRevenue, growthRate);

            // Generic cost structure
            var cogs = annualRevenue * 0.35m;
            var opex = annualRevenue * 0.25m;
            var marketing = annualRevenue * 0.10m;

            projection.SetCosts(cogs: cogs, opex: opex, marketing: marketing);
            projection.SetNotes($"Generic projection for {projectionYear}",
                "Generated from generic business template");

            projections.Add(projection);
        }

        return projections;
    }

    private FinancialExportResult ExportToCsv(List<FinancialProjection> projections, Guid businessPlanId)
    {
        var csv = new StringBuilder();
        csv.AppendLine("Year,Month,Quarter,Revenue,COGS,OpEx,Marketing,R&D,Admin,Other,GrossProfit,NetIncome,EBITDA,CashFlow,CashBalance,Employees,Customers,UnitsSold,ARPC,Notes");

        foreach (var projection in projections)
        {
            csv.AppendLine($"{projection.Year},{projection.Month},{projection.Quarter}," +
                          $"{projection.Revenue},{projection.CostOfGoodsSold},{projection.OperatingExpenses}," +
                          $"{projection.MarketingExpenses},{projection.RAndDExpenses},{projection.AdministrativeExpenses}," +
                          $"{projection.OtherExpenses},{projection.GrossProfit},{projection.NetIncome}," +
                          $"{projection.EBITDA},{projection.CashFlow},{projection.CashBalance}," +
                          $"{projection.Employees},{projection.Customers},{projection.UnitsSold}," +
                          $"{projection.AverageRevenuePerCustomer},\"{projection.Notes}\"");
        }

        var csvBytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
        return new FinancialExportResult
        {
            FileData = csvBytes,
            FileName = $"financial_projections_{businessPlanId}_{DateTime.UtcNow:yyyyMMdd}.csv",
            ContentType = "text/csv",
            FileSizeBytes = csvBytes.Length,
            Format = "csv"
        };
    }

    private FinancialExportResult ExportToJson(List<FinancialProjection> projections, Guid businessPlanId)
    {
        var jsonData = System.Text.Json.JsonSerializer.Serialize(projections, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        });

        var jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonData);
        return new FinancialExportResult
        {
            FileData = jsonBytes,
            FileName = $"financial_projections_{businessPlanId}_{DateTime.UtcNow:yyyyMMdd}.json",
            ContentType = "application/json",
            FileSizeBytes = jsonBytes.Length,
            Format = "json"
        };
    }

    private FinancialExportResult ExportToExcel(List<FinancialProjection> projections, Guid businessPlanId)
    {
        // For a full Excel implementation, you would use a library like EPPlus or ClosedXML
        // For now, return CSV with Excel content type as a simplified implementation
        var csvResult = ExportToCsv(projections, businessPlanId);

        return new FinancialExportResult
        {
            FileData = csvResult.FileData,
            FileName = $"financial_projections_{businessPlanId}_{DateTime.UtcNow:yyyyMMdd}.xlsx",
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            FileSizeBytes = csvResult.FileSizeBytes,
            Format = "excel"
        };
    }

    #endregion
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services;

namespace WebAPI.Controllers;

/// <summary>
/// Financial projections management and calculation engine
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/business-plans/{businessPlanId}/financial-projections")]
[Authorize]
public class FinancialProjectionController : BaseApiController
{
    private readonly IFinancialProjectionService _financialProjectionService;
    private readonly ILogger<FinancialProjectionController> _logger;

    public FinancialProjectionController(
        IFinancialProjectionService financialProjectionService,
        ILogger<FinancialProjectionController> logger)
    {
        _financialProjectionService = financialProjectionService;
        _logger = logger;
    }

    /// <summary>
    /// Get all financial projections for a business plan
    /// </summary>
    /// <param name="businessPlanId">Business plan ID</param>
    /// <param name="year">Optional year filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of financial projections</returns>
    /// <remarks>
    /// Retrieves all financial projections for the specified business plan.
    /// Can be filtered by year to get projections for a specific year only.
    ///
    /// Sample request:
    ///     GET /api/v1/business-plans/3fa85f64-5717-4562-b3fc-2c963f66afa6/financial-projections
    ///     GET /api/v1/business-plans/3fa85f64-5717-4562-b3fc-2c963f66afa6/financial-projections?year=2024
    ///
    /// Sample response:
    /// [
    ///   {
    ///     "id": "123e4567-e89b-12d3-a456-426614174000",
    ///     "businessPlanId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///     "year": 2024,
    ///     "month": null,
    ///     "quarter": null,
    ///     "revenue": 1000000.00,
    ///     "costOfGoodsSold": 300000.00,
    ///     "operatingExpenses": 250000.00,
    ///     "grossProfit": 700000.00,
    ///     "netIncome": 450000.00
    ///   }
    /// ]
    /// </remarks>
    /// <response code="200">Financial projections retrieved successfully</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">Business plan not found</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProjections(
        Guid businessPlanId,
        [FromQuery] int? year = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting financial projections for business plan {BusinessPlanId}, year filter: {Year}",
            businessPlanId, year);

        var result = await _financialProjectionService.GetProjectionsAsync(businessPlanId, year, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a new financial projection
    /// </summary>
    /// <param name="businessPlanId">Business plan ID</param>
    /// <param name="request">Financial projection creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created financial projection</returns>
    /// <remarks>
    /// Creates a new financial projection for the specified business plan.
    /// Each projection represents financial data for a specific time period (year, quarter, or month).
    ///
    /// Sample request:
    ///     POST /api/v1/business-plans/3fa85f64-5717-4562-b3fc-2c963f66afa6/financial-projections
    ///     {
    ///       "year": 2024,
    ///       "month": null,
    ///       "quarter": null,
    ///       "revenue": 1000000.00,
    ///       "costOfGoodsSold": 300000.00,
    ///       "operatingExpenses": 250000.00,
    ///       "marketingExpenses": 100000.00,
    ///       "employees": 25,
    ///       "customers": 1000,
    ///       "notes": "Annual projection for 2024",
    ///       "assumptions": "Based on 20% YoY growth assumption"
    ///     }
    /// </remarks>
    /// <response code="201">Financial projection created successfully</response>
    /// <response code="400">Invalid request data or duplicate projection</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">Business plan not found</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateProjection(
        Guid businessPlanId,
        [FromBody] CreateFinancialProjectionRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating financial projection for business plan {BusinessPlanId}, year {Year}",
            businessPlanId, request.Year);

        var result = await _financialProjectionService.CreateProjectionAsync(businessPlanId, request, cancellationToken);

        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetProjections), new { businessPlanId }, result.Value);
        }

        return HandleResult(result);
    }

    /// <summary>
    /// Update an existing financial projection
    /// </summary>
    /// <param name="businessPlanId">Business plan ID</param>
    /// <param name="projectionId">Financial projection ID</param>
    /// <param name="request">Update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated financial projection</returns>
    /// <remarks>
    /// Updates an existing financial projection with new values.
    /// All financial calculations are automatically recalculated.
    ///
    /// Sample request:
    ///     PUT /api/v1/business-plans/3fa85f64-5717-4562-b3fc-2c963f66afa6/financial-projections/123e4567-e89b-12d3-a456-426614174000
    ///     {
    ///       "revenue": 1200000.00,
    ///       "costOfGoodsSold": 350000.00,
    ///       "operatingExpenses": 280000.00,
    ///       "notes": "Updated projection with Q4 actuals"
    ///     }
    /// </remarks>
    /// <response code="200">Financial projection updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">Financial projection not found</response>
    [HttpPut("{projectionId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProjection(
        Guid businessPlanId,
        Guid projectionId,
        [FromBody] UpdateFinancialProjectionRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating financial projection {ProjectionId} for business plan {BusinessPlanId}",
            projectionId, businessPlanId);

        var result = await _financialProjectionService.UpdateProjectionAsync(projectionId, request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Delete a financial projection
    /// </summary>
    /// <param name="businessPlanId">Business plan ID</param>
    /// <param name="projectionId">Financial projection ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success result</returns>
    /// <remarks>
    /// Permanently deletes a financial projection.
    /// This action cannot be undone.
    ///
    /// Sample request:
    ///     DELETE /api/v1/business-plans/3fa85f64-5717-4562-b3fc-2c963f66afa6/financial-projections/123e4567-e89b-12d3-a456-426614174000
    /// </remarks>
    /// <response code="204">Financial projection deleted successfully</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">Financial projection not found</response>
    [HttpDelete("{projectionId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProjection(
        Guid businessPlanId,
        Guid projectionId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting financial projection {ProjectionId} for business plan {BusinessPlanId}",
            projectionId, businessPlanId);

        var result = await _financialProjectionService.DeleteProjectionAsync(projectionId, cancellationToken);

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return HandleResult(result);
    }

    /// <summary>
    /// Generate comprehensive financial projection scenario
    /// </summary>
    /// <param name="businessPlanId">Business plan ID</param>
    /// <param name="request">Scenario generation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Generated financial projections</returns>
    /// <remarks>
    /// Generates multiple years of financial projections based on business model assumptions.
    /// This is an intelligent projection engine that creates realistic financial forecasts.
    ///
    /// Sample request:
    ///     POST /api/v1/business-plans/3fa85f64-5717-4562-b3fc-2c963f66afa6/financial-projections/generate-scenario
    ///     {
    ///       "scenarioName": "Conservative Growth",
    ///       "startYear": 2024,
    ///       "projectionYears": 3,
    ///       "frequency": "yearly",
    ///       "initialRevenue": 500000.00,
    ///       "annualRevenueGrowthRate": 0.25,
    ///       "costOfGoodsSoldPercentage": 0.30,
    ///       "operatingExpensesPercentage": 0.25,
    ///       "marketingBudgetPercentage": 0.10,
    ///       "initialCustomers": 500,
    ///       "customerGrowthRate": 0.15,
    ///       "initialEmployees": 5,
    ///       "averageSalaryPerEmployee": 65000.00,
    ///       "initialCashBalance": 100000.00
    ///     }
    ///
    /// The engine will generate projections for 3 years (2024-2026) with calculated:
    /// - Revenue growth based on customer acquisition and retention
    /// - Cost scaling based on business model
    /// - Cash flow projections
    /// - Employee headcount growth
    /// - Profitability analysis
    /// </remarks>
    /// <response code="201">Financial scenario generated successfully</response>
    /// <response code="400">Invalid scenario parameters</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">Business plan not found</response>
    [HttpPost("generate-scenario")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenerateProjectionScenario(
        Guid businessPlanId,
        [FromBody] GenerateProjectionScenarioRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating financial projection scenario '{ScenarioName}' for business plan {BusinessPlanId}",
            request.ScenarioName, businessPlanId);

        var result = await _financialProjectionService.GenerateProjectionScenarioAsync(businessPlanId, request, cancellationToken);

        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetProjections), new { businessPlanId }, result.Value);
        }

        return HandleResult(result);
    }

    /// <summary>
    /// Get financial metrics and analysis
    /// </summary>
    /// <param name="businessPlanId">Business plan ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Comprehensive financial metrics</returns>
    /// <remarks>
    /// Calculates and returns comprehensive financial metrics and analysis including:
    /// - Revenue metrics (total, growth rate, ARPC)
    /// - Profitability metrics (gross margin, net margin, EBITDA)
    /// - Cash flow metrics (runway, burn rate)
    /// - Growth metrics (customer, employee, revenue per employee)
    /// - Financial health score and recommendations
    ///
    /// Sample request:
    ///     GET /api/v1/business-plans/3fa85f64-5717-4562-b3fc-2c963f66afa6/financial-projections/metrics
    ///
    /// Sample response:
    /// {
    ///   "businessPlanId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "totalRevenue": 3250000.00,
    ///   "revenueGrowthRate": 0.28,
    ///   "grossMargin": 0.65,
    ///   "netMargin": 0.18,
    ///   "cashRunwayMonths": 18.5,
    ///   "financialHealthScore": "B",
    ///   "insights": ["Strong revenue growth", "Healthy profit margins"],
    ///   "recommendations": ["Consider expanding team", "Explore new markets"],
    ///   "riskFactors": []
    /// }
    /// </remarks>
    /// <response code="200">Financial metrics calculated successfully</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">Business plan not found or no projections available</response>
    [HttpGet("metrics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFinancialMetrics(
        Guid businessPlanId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Calculating financial metrics for business plan {BusinessPlanId}", businessPlanId);

        var result = await _financialProjectionService.GetFinancialMetricsAsync(businessPlanId, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Export financial projections to various formats
    /// </summary>
    /// <param name="businessPlanId">Business plan ID</param>
    /// <param name="format">Export format (csv, json, excel)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Exported financial data file</returns>
    /// <remarks>
    /// Exports all financial projections for the business plan in the specified format.
    /// Supported formats:
    /// - CSV: Comma-separated values for spreadsheet import
    /// - JSON: Structured data format for API integration
    /// - Excel: Microsoft Excel spreadsheet (future implementation)
    ///
    /// Sample request:
    ///     GET /api/v1/business-plans/3fa85f64-5717-4562-b3fc-2c963f66afa6/financial-projections/export?format=csv
    ///
    /// The response will be a file download with appropriate Content-Disposition headers.
    /// </remarks>
    /// <response code="200">Financial projections exported successfully</response>
    /// <response code="400">Invalid export format</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">Business plan not found or no projections available</response>
    [HttpGet("export")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportProjections(
        Guid businessPlanId,
        [FromQuery] string format = "csv",
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Exporting financial projections for business plan {BusinessPlanId} in {Format} format",
            businessPlanId, format);

        // Validate format
        var validFormats = new[] { "csv", "json", "excel" };
        if (!validFormats.Contains(format.ToLower()))
        {
            return BadRequest(new { error = $"Invalid format. Supported formats: {string.Join(", ", validFormats)}" });
        }

        var result = await _financialProjectionService.ExportProjectionsAsync(businessPlanId, format, cancellationToken);

        if (!result.IsSuccess)
        {
            return HandleResult(result);
        }

        var exportResult = result.Value!;

        return File(
            exportResult.FileData,
            exportResult.ContentType,
            exportResult.FileName);
    }

    /// <summary>
    /// Get available financial projection templates
    /// </summary>
    /// <returns>List of available templates</returns>
    /// <remarks>
    /// Returns predefined financial projection templates for different business models.
    /// Templates include industry-specific assumptions and parameters.
    ///
    /// Available templates:
    /// - SaaS Startup: For software-as-a-service businesses
    /// - E-commerce: For online retail businesses
    /// - Consulting: For professional services firms
    /// - Restaurant: For food service businesses
    ///
    /// Sample request:
    ///     GET /api/v1/business-plans/any-id/financial-projections/templates
    ///
    /// Sample response:
    /// [
    ///   {
    ///     "id": "saas-startup",
    ///     "name": "SaaS Startup",
    ///     "description": "Financial projections for Software-as-a-Service startups",
    ///     "category": "SaaS",
    ///     "isDefault": true,
    ///     "requiredParameters": ["monthly_recurring_revenue", "churn_rate"]
    ///   }
    /// ]
    /// </remarks>
    /// <response code="200">Templates retrieved successfully</response>
    /// <response code="401">Unauthorized - authentication required</response>
    [HttpGet("templates")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAvailableTemplates()
    {
        _logger.LogInformation("Getting available financial projection templates");

        var result = await _financialProjectionService.GetAvailableTemplatesAsync();
        return HandleResult(result);
    }

    /// <summary>
    /// Apply a financial projection template
    /// </summary>
    /// <param name="businessPlanId">Business plan ID</param>
    /// <param name="request">Template application request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Generated projections from template</returns>
    /// <remarks>
    /// Applies a predefined template to generate financial projections automatically.
    /// Templates use industry-specific assumptions and best practices.
    ///
    /// Sample request:
    ///     POST /api/v1/business-plans/3fa85f64-5717-4562-b3fc-2c963f66afa6/financial-projections/apply-template
    ///     {
    ///       "templateId": "saas-startup",
    ///       "parameters": {
    ///         "monthly_recurring_revenue": 50000.00,
    ///         "churn_rate": 0.05,
    ///         "customer_acquisition_cost": 150.00,
    ///         "annual_growth_rate": 1.2
    ///       }
    ///     }
    ///
    /// This will generate 3 years of projections using SaaS-specific formulas and assumptions.
    /// </remarks>
    /// <response code="201">Template applied successfully</response>
    /// <response code="400">Invalid template ID or missing parameters</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">Business plan not found</response>
    [HttpPost("apply-template")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApplyTemplate(
        Guid businessPlanId,
        [FromBody] ApplyTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Applying template '{TemplateId}' to business plan {BusinessPlanId}",
            request.TemplateId, businessPlanId);

        var result = await _financialProjectionService.ApplyTemplateAsync(
            businessPlanId, request.TemplateId, request.Parameters, cancellationToken);

        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetProjections), new { businessPlanId }, result.Value);
        }

        return HandleResult(result);
    }

    /// <summary>
    /// Validate a financial projection
    /// </summary>
    /// <param name="businessPlanId">Business plan ID</param>
    /// <param name="projectionId">Financial projection ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation results</returns>
    /// <remarks>
    /// Validates the logic and consistency of a financial projection.
    /// Returns errors, warnings, and suggestions for improvement.
    ///
    /// Sample request:
    ///     GET /api/v1/business-plans/3fa85f64-5717-4562-b3fc-2c963f66afa6/financial-projections/123e4567-e89b-12d3-a456-426614174000/validate
    ///
    /// Sample response:
    /// {
    ///   "isValid": true,
    ///   "errors": [],
    ///   "warnings": ["Cost of Goods Sold exceeds 50% of revenue"],
    ///   "suggestions": ["Consider renegotiating supplier contracts"],
    ///   "healthScore": 75.5
    /// }
    /// </remarks>
    /// <response code="200">Validation completed successfully</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">Financial projection not found</response>
    [HttpGet("{projectionId}/validate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ValidateProjection(
        Guid businessPlanId,
        Guid projectionId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Validating financial projection {ProjectionId} for business plan {BusinessPlanId}",
            projectionId, businessPlanId);

        var validationResult = await _financialProjectionService.ValidateProjectionAsync(projectionId, cancellationToken);
        return HandleResult(validationResult);
    }
}

/// <summary>
/// Request model for applying financial projection templates
/// </summary>
public class ApplyTemplateRequest
{
    public string TemplateId { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
}
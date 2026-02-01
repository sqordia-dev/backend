using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Financial.Commands;
using Sqordia.Application.Financial.Queries;
using Sqordia.Application.Financial.Services;
using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.Financial;
using Sqordia.Domain.Enums;

namespace WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/financial")]
[Authorize]
public class FinancialController : BaseApiController
{
    private readonly IFinancialService _financialService;

    public FinancialController(IFinancialService financialService)
    {
        _financialService = financialService;
    }

    // Financial Projections
    [HttpPost("projections")]
    public async Task<IActionResult> CreateFinancialProjection([FromBody] CreateFinancialProjectionCommand command, CancellationToken cancellationToken = default)
    {
        var result = await _financialService.CreateFinancialProjectionAsync(command);
        return HandleResult(result);
    }

    [HttpGet("projections/{id}")]
    public async Task<IActionResult> GetFinancialProjection(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _financialService.GetFinancialProjectionByIdAsync(id);
        return HandleResult(result);
    }

    [HttpGet("projections/business-plan/{businessPlanId}")]
    public async Task<IActionResult> GetFinancialProjectionsByBusinessPlan(Guid businessPlanId, CancellationToken cancellationToken = default)
    {
        var result = await _financialService.GetFinancialProjectionsByBusinessPlanAsync(businessPlanId);
        return HandleResult(result);
    }

    [HttpGet("projections/business-plan/{businessPlanId}/scenario/{scenario}")]
    public async Task<IActionResult> GetFinancialProjectionsByScenario(Guid businessPlanId, ScenarioType scenario, CancellationToken cancellationToken = default)
    {
        var result = await _financialService.GetFinancialProjectionsByScenarioAsync(businessPlanId, scenario);
        return HandleResult(result);
    }

    [HttpPut("projections/{id}")]
    public async Task<IActionResult> UpdateFinancialProjection(Guid id, [FromBody] UpdateFinancialProjectionCommand command, CancellationToken cancellationToken = default)
    {
        var result = await _financialService.UpdateFinancialProjectionAsync(command with { Id = id });
        return HandleResult(result);
    }

    [HttpDelete("projections/{id}")]
    public async Task<IActionResult> DeleteFinancialProjection(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _financialService.DeleteFinancialProjectionAsync(id);
        return HandleResult(result);
    }

    // Currency Management
    [AllowAnonymous]
    [HttpGet("currencies")]
    public async Task<IActionResult> GetAllCurrencies(CancellationToken cancellationToken = default)
    {
        var result = await _financialService.GetAllCurrenciesAsync();
        return HandleResult(result);
    }

    [AllowAnonymous]
    [HttpGet("currencies/{currencyCode}")]
    public async Task<IActionResult> GetCurrency(string currencyCode, CancellationToken cancellationToken = default)
    {
        var result = await _financialService.GetCurrencyAsync(currencyCode);
        return HandleResult(result);
    }

    [HttpGet("currencies/convert")]
    public async Task<IActionResult> ConvertCurrency(decimal amount, string fromCurrency, string toCurrency, CancellationToken cancellationToken = default)
    {
        var result = await _financialService.ConvertCurrencyAsync(amount, fromCurrency, toCurrency);
        return HandleResult(result);
    }

    [HttpGet("currencies/exchange-rate")]
    public async Task<IActionResult> GetExchangeRate(string fromCurrency, string toCurrency, CancellationToken cancellationToken = default)
    {
        var result = await _financialService.GetExchangeRateAsync(fromCurrency, toCurrency);
        return HandleResult(result);
    }

    // Tax Calculations
    [HttpPost("tax/calculate")]
    public async Task<IActionResult> CalculateTax([FromBody] TaxCalculationRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _financialService.CalculateTaxAsync(request);
        return HandleResult(result);
    }

    [HttpGet("tax/projection/{projectionId}")]
    public async Task<IActionResult> CalculateTaxesForProjection(Guid projectionId, CancellationToken cancellationToken = default)
    {
        var result = await _financialService.CalculateTaxesForProjectionAsync(projectionId);
        return HandleResult(result);
    }

    [HttpGet("tax/rules")]
    public async Task<IActionResult> GetTaxRules(string country, string region, CancellationToken cancellationToken = default)
    {
        var result = await _financialService.GetTaxRulesAsync(country, region);
        return HandleResult(result);
    }

    // Financial KPIs
    [HttpGet("kpis/business-plan/{businessPlanId}")]
    public async Task<IActionResult> CalculateKPIs(Guid businessPlanId, CancellationToken cancellationToken = default)
    {
        var result = await _financialService.CalculateKPIsAsync(businessPlanId);
        return HandleResult(result);
    }

    [HttpGet("kpis/business-plan/{businessPlanId}/name/{kpiName}")]
    public async Task<IActionResult> GetKPIByName(Guid businessPlanId, string kpiName, CancellationToken cancellationToken = default)
    {
        var result = await _financialService.GetKPIByNameAsync(businessPlanId, kpiName);
        return HandleResult(result);
    }

    [HttpGet("kpis/business-plan/{businessPlanId}/category/{category}")]
    public async Task<IActionResult> GetKPIsByCategory(Guid businessPlanId, string category, CancellationToken cancellationToken = default)
    {
        var result = await _financialService.GetKPIsByCategoryAsync(businessPlanId, category);
        return HandleResult(result);
    }

    // Investment Analysis
    [HttpPost("investment/analysis")]
    public async Task<IActionResult> CreateInvestmentAnalysis([FromBody] CreateInvestmentAnalysisCommand command, CancellationToken cancellationToken = default)
    {
        var result = await _financialService.CreateInvestmentAnalysisAsync(command);
        return HandleResult(result);
    }

    [HttpGet("investment/roi/business-plan/{businessPlanId}")]
    public async Task<IActionResult> CalculateROI(Guid businessPlanId, decimal investmentAmount, CancellationToken cancellationToken = default)
    {
        var result = await _financialService.CalculateROIAsync(businessPlanId, investmentAmount);
        return HandleResult(result);
    }

    [HttpGet("investment/npv/business-plan/{businessPlanId}")]
    public async Task<IActionResult> CalculateNPV(Guid businessPlanId, decimal discountRate, CancellationToken cancellationToken = default)
    {
        var result = await _financialService.CalculateNPVAsync(businessPlanId, discountRate);
        return HandleResult(result);
    }

    [HttpGet("investment/irr/business-plan/{businessPlanId}")]
    public async Task<IActionResult> CalculateIRR(Guid businessPlanId, CancellationToken cancellationToken = default)
    {
        var result = await _financialService.CalculateIRRAsync(businessPlanId);
        return HandleResult(result);
    }

    // Financial Reports
    [HttpGet("reports/business-plan/{businessPlanId}")]
    public async Task<IActionResult> GenerateFinancialReport(Guid businessPlanId, string reportType, CancellationToken cancellationToken = default)
    {
        var result = await _financialService.GenerateFinancialReportAsync(businessPlanId, reportType);
        return HandleResult(result);
    }

    [HttpGet("reports/cash-flow/business-plan/{businessPlanId}")]
    public async Task<IActionResult> GenerateCashFlowReport(Guid businessPlanId, CancellationToken cancellationToken = default)
    {
        var result = await _financialService.GenerateCashFlowReportAsync(businessPlanId);
        return HandleResult(result);
    }

    [HttpGet("reports/profit-loss/business-plan/{businessPlanId}")]
    public async Task<IActionResult> GenerateProfitLossReport(Guid businessPlanId, CancellationToken cancellationToken = default)
    {
        var result = await _financialService.GenerateProfitLossReportAsync(businessPlanId);
        return HandleResult(result);
    }

    [HttpGet("reports/balance-sheet/business-plan/{businessPlanId}")]
    public async Task<IActionResult> GenerateBalanceSheetReport(Guid businessPlanId, CancellationToken cancellationToken = default)
    {
        var result = await _financialService.GenerateBalanceSheetReportAsync(businessPlanId);
        return HandleResult(result);
    }

    // Scenario Analysis
    [HttpGet("analysis/scenario/business-plan/{businessPlanId}")]
    public async Task<IActionResult> PerformScenarioAnalysis(Guid businessPlanId, CancellationToken cancellationToken = default)
    {
        var result = await _financialService.PerformScenarioAnalysisAsync(businessPlanId);
        return HandleResult(result);
    }

    [HttpGet("analysis/sensitivity/business-plan/{businessPlanId}")]
    public async Task<IActionResult> PerformSensitivityAnalysis(Guid businessPlanId, string variable, CancellationToken cancellationToken = default)
    {
        var result = await _financialService.PerformSensitivityAnalysisAsync(businessPlanId, variable);
        return HandleResult(result);
    }

    [HttpGet("analysis/break-even/business-plan/{businessPlanId}")]
    public async Task<IActionResult> CalculateBreakEven(Guid businessPlanId, CancellationToken cancellationToken = default)
    {
        var result = await _financialService.CalculateBreakEvenAsync(businessPlanId);
        return HandleResult(result);
    }

    // Consultant Financial Calculations
    [HttpPost("calculate-consultant")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CalculateConsultantFinancials(
        [FromBody] CalculateConsultantFinancialsRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _financialService.CalculateConsultantFinancialsAsync(request);
        return HandleResult(result);
    }

    [HttpGet("overhead-estimates/{city}/{province}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetLocationOverheadEstimate(
        string city,
        string province,
        CancellationToken cancellationToken = default)
    {
        var result = await _financialService.GetLocationOverheadEstimateAsync(city, province);
        return HandleResult(result);
    }
}

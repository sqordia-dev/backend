using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Financial.Commands;
using Sqordia.Application.Financial.Queries;
using Sqordia.Application.Financial.Services;
using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.Financial;
using Sqordia.Domain.Enums;

namespace WebAPI.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/financial")]
public class FinancialController : ControllerBase
{
    private readonly IFinancialService _financialService;

    public FinancialController(IFinancialService financialService)
    {
        _financialService = financialService;
    }

    // Financial Projections
    [HttpPost("projections")]
    public async Task<ActionResult<Result<FinancialProjectionDto>>> CreateFinancialProjection([FromBody] CreateFinancialProjectionCommand command)
    {
        var result = await _financialService.CreateFinancialProjectionAsync(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("projections/{id}")]
    public async Task<ActionResult<Result<FinancialProjectionDto>>> GetFinancialProjection(Guid id)
    {
        var result = await _financialService.GetFinancialProjectionByIdAsync(id);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }

    [HttpGet("projections/business-plan/{businessPlanId}")]
    public async Task<ActionResult<Result<List<FinancialProjectionDto>>>> GetFinancialProjectionsByBusinessPlan(Guid businessPlanId)
    {
        var result = await _financialService.GetFinancialProjectionsByBusinessPlanAsync(businessPlanId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("projections/business-plan/{businessPlanId}/scenario/{scenario}")]
    public async Task<ActionResult<Result<List<FinancialProjectionDto>>>> GetFinancialProjectionsByScenario(Guid businessPlanId, ScenarioType scenario)
    {
        var result = await _financialService.GetFinancialProjectionsByScenarioAsync(businessPlanId, scenario);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPut("projections/{id}")]
    public async Task<ActionResult<Result<FinancialProjectionDto>>> UpdateFinancialProjection(Guid id, [FromBody] UpdateFinancialProjectionCommand command)
    {
        var result = await _financialService.UpdateFinancialProjectionAsync(command with { Id = id });
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("projections/{id}")]
    public async Task<ActionResult<Result<bool>>> DeleteFinancialProjection(Guid id)
    {
        var result = await _financialService.DeleteFinancialProjectionAsync(id);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // Currency Management
    [HttpGet("currencies")]
    public async Task<ActionResult<Result<List<CurrencyDto>>>> GetAllCurrencies()
    {
        var result = await _financialService.GetAllCurrenciesAsync();
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("currencies/{currencyCode}")]
    public async Task<ActionResult<Result<CurrencyDto>>> GetCurrency(string currencyCode)
    {
        var result = await _financialService.GetCurrencyAsync(currencyCode);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }

    [HttpGet("currencies/convert")]
    public async Task<ActionResult<Result<decimal>>> ConvertCurrency(decimal amount, string fromCurrency, string toCurrency)
    {
        var result = await _financialService.ConvertCurrencyAsync(amount, fromCurrency, toCurrency);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("currencies/exchange-rate")]
    public async Task<ActionResult<Result<ExchangeRateDto>>> GetExchangeRate(string fromCurrency, string toCurrency)
    {
        var result = await _financialService.GetExchangeRateAsync(fromCurrency, toCurrency);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }

    // Tax Calculations
    [HttpPost("tax/calculate")]
    public async Task<ActionResult<Result<TaxCalculationDto>>> CalculateTax([FromBody] TaxCalculationRequest request)
    {
        var result = await _financialService.CalculateTaxAsync(request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("tax/projection/{projectionId}")]
    public async Task<ActionResult<Result<List<TaxCalculationDto>>>> CalculateTaxesForProjection(Guid projectionId)
    {
        var result = await _financialService.CalculateTaxesForProjectionAsync(projectionId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("tax/rules")]
    public async Task<ActionResult<Result<List<TaxRuleDto>>>> GetTaxRules(string country, string region)
    {
        var result = await _financialService.GetTaxRulesAsync(country, region);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // Financial KPIs
    [HttpGet("kpis/business-plan/{businessPlanId}")]
    public async Task<ActionResult<Result<List<FinancialKPIDto>>>> CalculateKPIs(Guid businessPlanId)
    {
        var result = await _financialService.CalculateKPIsAsync(businessPlanId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("kpis/business-plan/{businessPlanId}/name/{kpiName}")]
    public async Task<ActionResult<Result<FinancialKPIDto>>> GetKPIByName(Guid businessPlanId, string kpiName)
    {
        var result = await _financialService.GetKPIByNameAsync(businessPlanId, kpiName);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }

    [HttpGet("kpis/business-plan/{businessPlanId}/category/{category}")]
    public async Task<ActionResult<Result<List<FinancialKPIDto>>>> GetKPIsByCategory(Guid businessPlanId, string category)
    {
        var result = await _financialService.GetKPIsByCategoryAsync(businessPlanId, category);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // Investment Analysis
    [HttpPost("investment/analysis")]
    public async Task<ActionResult<Result<InvestmentAnalysisDto>>> CreateInvestmentAnalysis([FromBody] CreateInvestmentAnalysisCommand command)
    {
        var result = await _financialService.CreateInvestmentAnalysisAsync(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("investment/roi/business-plan/{businessPlanId}")]
    public async Task<ActionResult<Result<InvestmentAnalysisDto>>> CalculateROI(Guid businessPlanId, decimal investmentAmount)
    {
        var result = await _financialService.CalculateROIAsync(businessPlanId, investmentAmount);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("investment/npv/business-plan/{businessPlanId}")]
    public async Task<ActionResult<Result<InvestmentAnalysisDto>>> CalculateNPV(Guid businessPlanId, decimal discountRate)
    {
        var result = await _financialService.CalculateNPVAsync(businessPlanId, discountRate);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("investment/irr/business-plan/{businessPlanId}")]
    public async Task<ActionResult<Result<InvestmentAnalysisDto>>> CalculateIRR(Guid businessPlanId)
    {
        var result = await _financialService.CalculateIRRAsync(businessPlanId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // Financial Reports
    [HttpGet("reports/business-plan/{businessPlanId}")]
    public async Task<ActionResult<Result<FinancialReportDto>>> GenerateFinancialReport(Guid businessPlanId, string reportType)
    {
        var result = await _financialService.GenerateFinancialReportAsync(businessPlanId, reportType);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("reports/cash-flow/business-plan/{businessPlanId}")]
    public async Task<ActionResult<Result<CashFlowReportDto>>> GenerateCashFlowReport(Guid businessPlanId)
    {
        var result = await _financialService.GenerateCashFlowReportAsync(businessPlanId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("reports/profit-loss/business-plan/{businessPlanId}")]
    public async Task<ActionResult<Result<ProfitLossReportDto>>> GenerateProfitLossReport(Guid businessPlanId)
    {
        var result = await _financialService.GenerateProfitLossReportAsync(businessPlanId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("reports/balance-sheet/business-plan/{businessPlanId}")]
    public async Task<ActionResult<Result<BalanceSheetReportDto>>> GenerateBalanceSheetReport(Guid businessPlanId)
    {
        var result = await _financialService.GenerateBalanceSheetReportAsync(businessPlanId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // Scenario Analysis
    [HttpGet("analysis/scenario/business-plan/{businessPlanId}")]
    public async Task<ActionResult<Result<List<ScenarioAnalysisDto>>>> PerformScenarioAnalysis(Guid businessPlanId)
    {
        var result = await _financialService.PerformScenarioAnalysisAsync(businessPlanId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("analysis/sensitivity/business-plan/{businessPlanId}")]
    public async Task<ActionResult<Result<SensitivityAnalysisDto>>> PerformSensitivityAnalysis(Guid businessPlanId, string variable)
    {
        var result = await _financialService.PerformSensitivityAnalysisAsync(businessPlanId, variable);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("analysis/break-even/business-plan/{businessPlanId}")]
    public async Task<ActionResult<Result<BreakEvenAnalysisDto>>> CalculateBreakEven(Guid businessPlanId)
    {
        var result = await _financialService.CalculateBreakEvenAsync(businessPlanId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // Consultant Financial Calculations
    [HttpPost("calculate-consultant")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result<ConsultantFinancialProjectionDto>>> CalculateConsultantFinancials(
        [FromBody] CalculateConsultantFinancialsRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _financialService.CalculateConsultantFinancialsAsync(request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("overhead-estimates/{city}/{province}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result<LocationOverheadEstimateDto>>> GetLocationOverheadEstimate(
        string city,
        string province)
    {
        var result = await _financialService.GetLocationOverheadEstimateAsync(city, province);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}

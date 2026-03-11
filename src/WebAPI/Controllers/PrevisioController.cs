using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Financial.Services.Previsio;
using Sqordia.Contracts.Requests.Financial.Previsio;

namespace WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/business-plans/{businessPlanId}/previsio")]
[Authorize]
public class PrevisioController : BaseApiController
{
    private readonly IFinancialPlanService _financialPlanService;
    private readonly ISalesModuleService _salesService;
    private readonly ICOGSModuleService _cogsService;
    private readonly IPayrollModuleService _payrollService;
    private readonly IExpenseModuleService _expenseService;
    private readonly ICapexModuleService _capexService;
    private readonly IFinancingModuleService _financingService;
    private readonly IProjectCostService _projectCostService;
    private readonly IFinancialStatementsService _statementsService;
    private readonly IFinancialForecastingService _forecastingService;

    public PrevisioController(
        IFinancialPlanService financialPlanService,
        ISalesModuleService salesService,
        ICOGSModuleService cogsService,
        IPayrollModuleService payrollService,
        IExpenseModuleService expenseService,
        ICapexModuleService capexService,
        IFinancingModuleService financingService,
        IProjectCostService projectCostService,
        IFinancialStatementsService statementsService,
        IFinancialForecastingService forecastingService)
    {
        _financialPlanService = financialPlanService;
        _salesService = salesService;
        _cogsService = cogsService;
        _payrollService = payrollService;
        _expenseService = expenseService;
        _capexService = capexService;
        _financingService = financingService;
        _projectCostService = projectCostService;
        _statementsService = statementsService;
        _forecastingService = forecastingService;
    }

    // === Financial Plan ===

    [HttpPost]
    public async Task<IActionResult> CreatePlan(Guid businessPlanId, [FromBody] CreateFinancialPlanRequest request, CancellationToken ct)
        => HandleResult(await _financialPlanService.CreateAsync(businessPlanId, request, ct));

    [HttpGet]
    public async Task<IActionResult> GetPlan(Guid businessPlanId, CancellationToken ct)
        => HandleResult(await _financialPlanService.GetByBusinessPlanIdAsync(businessPlanId, ct));

    [HttpPut("settings")]
    public async Task<IActionResult> UpdateSettings(Guid businessPlanId, [FromBody] UpdateFinancialPlanSettingsRequest request, CancellationToken ct)
        => HandleResult(await _financialPlanService.UpdateSettingsAsync(businessPlanId, request, ct));

    [HttpDelete]
    public async Task<IActionResult> DeletePlan(Guid businessPlanId, CancellationToken ct)
        => HandleResult(await _financialPlanService.DeleteAsync(businessPlanId, ct));

    // === Sales ===

    [HttpGet("sales")]
    public async Task<IActionResult> GetSales(Guid businessPlanId, CancellationToken ct)
        => HandleResult(await _salesService.GetAllAsync(businessPlanId, ct));

    [HttpPost("sales/products")]
    public async Task<IActionResult> CreateProduct(Guid businessPlanId, [FromBody] CreateSalesProductRequest request, CancellationToken ct)
        => HandleResult(await _salesService.CreateProductAsync(businessPlanId, request, ct));

    [HttpPut("sales/products/{productId}")]
    public async Task<IActionResult> UpdateProduct(Guid businessPlanId, Guid productId, [FromBody] UpdateSalesProductRequest request, CancellationToken ct)
        => HandleResult(await _salesService.UpdateProductAsync(businessPlanId, productId, request, ct));

    [HttpDelete("sales/products/{productId}")]
    public async Task<IActionResult> DeleteProduct(Guid businessPlanId, Guid productId, CancellationToken ct)
        => HandleResult(await _salesService.DeleteProductAsync(businessPlanId, productId, ct));

    [HttpGet("sales/products/{productId}/volumes/{year}")]
    public async Task<IActionResult> GetVolumeGrid(Guid businessPlanId, Guid productId, int year, CancellationToken ct)
        => HandleResult(await _salesService.GetVolumeGridAsync(businessPlanId, productId, year, ct));

    [HttpPut("sales/volumes")]
    public async Task<IActionResult> UpdateVolumeGrid(Guid businessPlanId, [FromBody] UpdateSalesVolumeGridRequest request, CancellationToken ct)
        => HandleResult(await _salesService.UpdateVolumeGridAsync(businessPlanId, request, ct));

    [HttpPost("sales/replicate")]
    public async Task<IActionResult> ReplicateYear(Guid businessPlanId, [FromBody] ReplicateYearRequest request, CancellationToken ct)
        => HandleResult(await _salesService.ReplicateYearAsync(businessPlanId, request, ct));

    // === COGS ===

    [HttpGet("cogs")]
    public async Task<IActionResult> GetCOGS(Guid businessPlanId, CancellationToken ct)
        => HandleResult(await _cogsService.GetAllAsync(businessPlanId, ct));

    [HttpPost("cogs")]
    public async Task<IActionResult> CreateCOGS(Guid businessPlanId, [FromBody] CreateCOGSItemRequest request, CancellationToken ct)
        => HandleResult(await _cogsService.CreateAsync(businessPlanId, request, ct));

    [HttpPut("cogs/{itemId}")]
    public async Task<IActionResult> UpdateCOGS(Guid businessPlanId, Guid itemId, [FromBody] UpdateCOGSItemRequest request, CancellationToken ct)
        => HandleResult(await _cogsService.UpdateAsync(businessPlanId, itemId, request, ct));

    [HttpDelete("cogs/{itemId}")]
    public async Task<IActionResult> DeleteCOGS(Guid businessPlanId, Guid itemId, CancellationToken ct)
        => HandleResult(await _cogsService.DeleteAsync(businessPlanId, itemId, ct));

    // === Payroll ===

    [HttpGet("payroll")]
    public async Task<IActionResult> GetPayroll(Guid businessPlanId, CancellationToken ct)
        => HandleResult(await _payrollService.GetAllAsync(businessPlanId, ct));

    [HttpPost("payroll")]
    public async Task<IActionResult> CreatePayroll(Guid businessPlanId, [FromBody] CreatePayrollItemRequest request, CancellationToken ct)
        => HandleResult(await _payrollService.CreateAsync(businessPlanId, request, ct));

    [HttpPut("payroll/{itemId}")]
    public async Task<IActionResult> UpdatePayroll(Guid businessPlanId, Guid itemId, [FromBody] UpdatePayrollItemRequest request, CancellationToken ct)
        => HandleResult(await _payrollService.UpdateAsync(businessPlanId, itemId, request, ct));

    [HttpDelete("payroll/{itemId}")]
    public async Task<IActionResult> DeletePayroll(Guid businessPlanId, Guid itemId, CancellationToken ct)
        => HandleResult(await _payrollService.DeleteAsync(businessPlanId, itemId, ct));

    [HttpPost("payroll/calculate-salary")]
    public async Task<IActionResult> CalculateSalary([FromBody] CalculateSalaryRequest request, CancellationToken ct)
        => HandleResult(await _payrollService.CalculateSalaryAsync(request, ct));

    // === Sales Expenses ===

    [HttpGet("expenses/sales")]
    public async Task<IActionResult> GetSalesExpenses(Guid businessPlanId, CancellationToken ct)
        => HandleResult(await _expenseService.GetSalesExpensesAsync(businessPlanId, ct));

    [HttpPost("expenses/sales")]
    public async Task<IActionResult> CreateSalesExpense(Guid businessPlanId, [FromBody] CreateSalesExpenseRequest request, CancellationToken ct)
        => HandleResult(await _expenseService.CreateSalesExpenseAsync(businessPlanId, request, ct));

    [HttpPut("expenses/sales/{itemId}")]
    public async Task<IActionResult> UpdateSalesExpense(Guid businessPlanId, Guid itemId, [FromBody] UpdateSalesExpenseRequest request, CancellationToken ct)
        => HandleResult(await _expenseService.UpdateSalesExpenseAsync(businessPlanId, itemId, request, ct));

    [HttpDelete("expenses/sales/{itemId}")]
    public async Task<IActionResult> DeleteSalesExpense(Guid businessPlanId, Guid itemId, CancellationToken ct)
        => HandleResult(await _expenseService.DeleteSalesExpenseAsync(businessPlanId, itemId, ct));

    // === Admin Expenses ===

    [HttpGet("expenses/admin")]
    public async Task<IActionResult> GetAdminExpenses(Guid businessPlanId, CancellationToken ct)
        => HandleResult(await _expenseService.GetAdminExpensesAsync(businessPlanId, ct));

    [HttpPost("expenses/admin")]
    public async Task<IActionResult> CreateAdminExpense(Guid businessPlanId, [FromBody] CreateAdminExpenseRequest request, CancellationToken ct)
        => HandleResult(await _expenseService.CreateAdminExpenseAsync(businessPlanId, request, ct));

    [HttpPut("expenses/admin/{itemId}")]
    public async Task<IActionResult> UpdateAdminExpense(Guid businessPlanId, Guid itemId, [FromBody] UpdateAdminExpenseRequest request, CancellationToken ct)
        => HandleResult(await _expenseService.UpdateAdminExpenseAsync(businessPlanId, itemId, request, ct));

    [HttpDelete("expenses/admin/{itemId}")]
    public async Task<IActionResult> DeleteAdminExpense(Guid businessPlanId, Guid itemId, CancellationToken ct)
        => HandleResult(await _expenseService.DeleteAdminExpenseAsync(businessPlanId, itemId, ct));

    // === CAPEX ===

    [HttpGet("capex")]
    public async Task<IActionResult> GetCapex(Guid businessPlanId, CancellationToken ct)
        => HandleResult(await _capexService.GetAllAsync(businessPlanId, ct));

    [HttpPost("capex")]
    public async Task<IActionResult> CreateCapex(Guid businessPlanId, [FromBody] CreateCapexAssetRequest request, CancellationToken ct)
        => HandleResult(await _capexService.CreateAsync(businessPlanId, request, ct));

    [HttpPut("capex/{assetId}")]
    public async Task<IActionResult> UpdateCapex(Guid businessPlanId, Guid assetId, [FromBody] UpdateCapexAssetRequest request, CancellationToken ct)
        => HandleResult(await _capexService.UpdateAsync(businessPlanId, assetId, request, ct));

    [HttpDelete("capex/{assetId}")]
    public async Task<IActionResult> DeleteCapex(Guid businessPlanId, Guid assetId, CancellationToken ct)
        => HandleResult(await _capexService.DeleteAsync(businessPlanId, assetId, ct));

    // === Financing ===

    [HttpGet("financing")]
    public async Task<IActionResult> GetFinancing(Guid businessPlanId, CancellationToken ct)
        => HandleResult(await _financingService.GetAllAsync(businessPlanId, ct));

    [HttpPost("financing")]
    public async Task<IActionResult> CreateFinancing(Guid businessPlanId, [FromBody] CreateFinancingSourceRequest request, CancellationToken ct)
        => HandleResult(await _financingService.CreateAsync(businessPlanId, request, ct));

    [HttpPut("financing/{sourceId}")]
    public async Task<IActionResult> UpdateFinancing(Guid businessPlanId, Guid sourceId, [FromBody] UpdateFinancingSourceRequest request, CancellationToken ct)
        => HandleResult(await _financingService.UpdateAsync(businessPlanId, sourceId, request, ct));

    [HttpDelete("financing/{sourceId}")]
    public async Task<IActionResult> DeleteFinancing(Guid businessPlanId, Guid sourceId, CancellationToken ct)
        => HandleResult(await _financingService.DeleteAsync(businessPlanId, sourceId, ct));

    [HttpGet("financing/{sourceId}/amortization")]
    public async Task<IActionResult> GetAmortization(Guid businessPlanId, Guid sourceId, CancellationToken ct)
        => HandleResult(await _financingService.GetAmortizationScheduleAsync(businessPlanId, sourceId, ct));

    // === Project Cost ===

    [HttpGet("project-cost")]
    public async Task<IActionResult> GetProjectCost(Guid businessPlanId, CancellationToken ct)
        => HandleResult(await _projectCostService.GetAsync(businessPlanId, ct));

    [HttpPut("project-cost")]
    public async Task<IActionResult> UpdateProjectCost(Guid businessPlanId, [FromBody] UpdateProjectCostSettingsRequest request, CancellationToken ct)
        => HandleResult(await _projectCostService.UpdateSettingsAsync(businessPlanId, request, ct));

    // === Statements ===

    [HttpPost("statements/recalculate")]
    public async Task<IActionResult> Recalculate(Guid businessPlanId, [FromQuery] string language = "fr", CancellationToken ct = default)
        => HandleResult(await _statementsService.RecalculateAsync(businessPlanId, language, ct));

    [HttpGet("statements/profit-loss/{year}")]
    public async Task<IActionResult> GetProfitLoss(Guid businessPlanId, int year, [FromQuery] string language = "fr", CancellationToken ct = default)
        => HandleResult(await _statementsService.GetProfitLossAsync(businessPlanId, year, language, ct));

    [HttpGet("statements/cash-flow/{year}")]
    public async Task<IActionResult> GetCashFlow(Guid businessPlanId, int year, [FromQuery] string language = "fr", CancellationToken ct = default)
        => HandleResult(await _statementsService.GetCashFlowAsync(businessPlanId, year, language, ct));

    [HttpGet("statements/balance-sheet/{year}")]
    public async Task<IActionResult> GetBalanceSheet(Guid businessPlanId, int year, [FromQuery] string language = "fr", CancellationToken ct = default)
        => HandleResult(await _statementsService.GetBalanceSheetAsync(businessPlanId, year, language, ct));

    [HttpGet("statements/ratios")]
    public async Task<IActionResult> GetRatios(Guid businessPlanId, CancellationToken ct)
        => HandleResult(await _statementsService.GetRatiosAsync(businessPlanId, ct));

    // === AI Forecasting ===

    [HttpPost("forecast")]
    public async Task<IActionResult> GenerateForecast(Guid businessPlanId, [FromQuery] string language = "fr", CancellationToken ct = default)
        => HandleResult(await _forecastingService.GenerateForecastAsync(businessPlanId, language, ct));

    [HttpPost("forecast/sales-volume/{productId}")]
    public async Task<IActionResult> ForecastSalesVolume(Guid businessPlanId, Guid productId, [FromQuery] string language = "fr", CancellationToken ct = default)
        => HandleResult(await _forecastingService.ForecastSalesVolumeAsync(businessPlanId, productId, language, ct));

    [HttpPost("forecast/analyze")]
    public async Task<IActionResult> AnalyzeFinancials(Guid businessPlanId, [FromQuery] string language = "fr", CancellationToken ct = default)
        => HandleResult(await _forecastingService.AnalyzeFinancialsAsync(businessPlanId, language, ct));
}

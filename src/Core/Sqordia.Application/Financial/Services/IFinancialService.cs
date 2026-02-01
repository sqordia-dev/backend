using Sqordia.Application.Common.Models;
using Sqordia.Application.Financial.Commands;
using Sqordia.Application.Financial.Queries;
using Sqordia.Contracts.Requests.Financial;
using Sqordia.Domain.Enums;

namespace Sqordia.Application.Financial.Services;

public interface IFinancialService
{
    // Financial Projections
    Task<Result<FinancialProjectionDto>> CreateFinancialProjectionAsync(CreateFinancialProjectionCommand command);
    Task<Result<FinancialProjectionDto>> GetFinancialProjectionByIdAsync(Guid id);
    Task<Result<List<FinancialProjectionDto>>> GetFinancialProjectionsByBusinessPlanAsync(Guid businessPlanId);
    Task<Result<List<FinancialProjectionDto>>> GetFinancialProjectionsByScenarioAsync(Guid businessPlanId, ScenarioType scenario);
    Task<Result<FinancialProjectionDto>> UpdateFinancialProjectionAsync(UpdateFinancialProjectionCommand command);
    Task<Result<bool>> DeleteFinancialProjectionAsync(Guid id);

    // Currency Management
    Task<Result<CurrencyDto>> GetCurrencyAsync(string currencyCode);
    Task<Result<List<CurrencyDto>>> GetAllCurrenciesAsync();
    Task<Result<decimal>> ConvertCurrencyAsync(decimal amount, string fromCurrency, string toCurrency);
    Task<Result<ExchangeRateDto>> GetExchangeRateAsync(string fromCurrency, string toCurrency);

    // Tax Calculations
    Task<Result<TaxCalculationDto>> CalculateTaxAsync(TaxCalculationRequest request);
    Task<Result<List<TaxCalculationDto>>> CalculateTaxesForProjectionAsync(Guid projectionId);
    Task<Result<List<TaxRuleDto>>> GetTaxRulesAsync(string country, string region);

    // Financial KPIs
    Task<Result<List<FinancialKPIDto>>> CalculateKPIsAsync(Guid businessPlanId);
    Task<Result<FinancialKPIDto>> GetKPIByNameAsync(Guid businessPlanId, string kpiName);
    Task<Result<List<FinancialKPIDto>>> GetKPIsByCategoryAsync(Guid businessPlanId, string category);

    // Investment Analysis
    Task<Result<InvestmentAnalysisDto>> CreateInvestmentAnalysisAsync(CreateInvestmentAnalysisCommand command);
    Task<Result<InvestmentAnalysisDto>> CalculateROIAsync(Guid businessPlanId, decimal investmentAmount);
    Task<Result<InvestmentAnalysisDto>> CalculateNPVAsync(Guid businessPlanId, decimal discountRate);
    Task<Result<InvestmentAnalysisDto>> CalculateIRRAsync(Guid businessPlanId);

    // Financial Reports
    Task<Result<FinancialReportDto>> GenerateFinancialReportAsync(Guid businessPlanId, string reportType);
    Task<Result<CashFlowReportDto>> GenerateCashFlowReportAsync(Guid businessPlanId);
    Task<Result<ProfitLossReportDto>> GenerateProfitLossReportAsync(Guid businessPlanId);
    Task<Result<BalanceSheetReportDto>> GenerateBalanceSheetReportAsync(Guid businessPlanId);

    // Scenario Analysis
    Task<Result<List<ScenarioAnalysisDto>>> PerformScenarioAnalysisAsync(Guid businessPlanId);
    Task<Result<SensitivityAnalysisDto>> PerformSensitivityAnalysisAsync(Guid businessPlanId, string variable);
    Task<Result<BreakEvenAnalysisDto>> CalculateBreakEvenAsync(Guid businessPlanId);

    // Consultant Financial Calculations
    Task<Result<ConsultantFinancialProjectionDto>> CalculateConsultantFinancialsAsync(CalculateConsultantFinancialsRequest request);
    Task<Result<LocationOverheadEstimateDto>> GetLocationOverheadEstimateAsync(string city, string province);
    Task<Result<object>> UpdateFinancialCellAsync(Guid planId, UpdateFinancialCellRequest request);
}

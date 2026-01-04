using MediatR;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Financial.Queries;

namespace Sqordia.Application.Financial.Commands;

public record CreateInvestmentAnalysisCommand : IRequest<Result<InvestmentAnalysisDto>>
{
    public Guid BusinessPlanId { get; init; }
    public string AnalysisType { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal InitialInvestment { get; init; }
    public decimal ExpectedReturn { get; init; }
    public string CurrencyCode { get; init; } = string.Empty;
    public decimal DiscountRate { get; init; }
    public int AnalysisPeriod { get; init; }
    public string RiskLevel { get; init; } = string.Empty;
    public string InvestmentType { get; init; } = string.Empty;
    public string InvestorType { get; init; } = string.Empty;
    public decimal Valuation { get; init; }
    public decimal EquityOffering { get; init; }
    public decimal FundingRequired { get; init; }
    public string FundingStage { get; init; } = string.Empty;
    public string Assumptions { get; init; } = string.Empty;
    public string Notes { get; init; } = string.Empty;
}

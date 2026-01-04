namespace Sqordia.Application.Financial.Queries;

public class InvestmentAnalysisDto
{
    public Guid Id { get; set; }
    public Guid BusinessPlanId { get; set; }
    public string AnalysisType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal InitialInvestment { get; set; }
    public decimal ExpectedReturn { get; set; }
    public decimal NetPresentValue { get; set; }
    public decimal InternalRateOfReturn { get; set; }
    public decimal PaybackPeriod { get; set; }
    public decimal ReturnOnInvestment { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal DiscountRate { get; set; }
    public int AnalysisPeriod { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
    public string InvestmentType { get; set; } = string.Empty;
    public string InvestorType { get; set; } = string.Empty;
    public decimal Valuation { get; set; }
    public decimal EquityOffering { get; set; }
    public decimal FundingRequired { get; set; }
    public string FundingStage { get; set; } = string.Empty;
    public string Assumptions { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

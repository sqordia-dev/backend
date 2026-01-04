using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities;

public class InvestmentAnalysis : BaseEntity
{
    public Guid BusinessPlanId { get; set; }
    public string AnalysisType { get; set; } = string.Empty; // ROI, NPV, IRR, Payback, etc.
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal InitialInvestment { get; set; }
    public decimal ExpectedReturn { get; set; }
    public decimal NetPresentValue { get; set; }
    public decimal InternalRateOfReturn { get; set; }
    public decimal PaybackPeriod { get; set; } // in months
    public decimal ReturnOnInvestment { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal DiscountRate { get; set; }
    public int AnalysisPeriod { get; set; } // years
    public string RiskLevel { get; set; } = string.Empty; // Low, Medium, High
    public string InvestmentType { get; set; } = string.Empty; // Equity, Debt, Hybrid
    public string InvestorType { get; set; } = string.Empty; // Angel, VC, PE, etc.
    public decimal Valuation { get; set; }
    public decimal EquityOffering { get; set; } // percentage
    public decimal FundingRequired { get; set; }
    public string FundingStage { get; set; } = string.Empty; // Seed, Series A, etc.
    public string Assumptions { get; set; } = string.Empty; // JSON of assumptions
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;

    // Navigation properties
    public Domain.Entities.BusinessPlan.BusinessPlan BusinessPlan { get; set; } = null!;
    public Currency? Currency { get; set; }
}

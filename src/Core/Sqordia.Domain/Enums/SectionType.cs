namespace Sqordia.Domain.Enums;

/// <summary>
/// Types of sections in a business plan
/// </summary>
public enum SectionType
{
    // Standard Business Plan Sections
    ExecutiveSummary = 1,
    CompanyOverview = 2,
    MarketAnalysis = 3,
    ProductsServices = 4,
    MarketingStrategy = 5,
    OperationsPlan = 6,
    ManagementTeam = 7,
    FinancialProjections = 8,
    FundingRequest = 9,
    Appendix = 10,
    SWOTAnalysis = 11,
    RiskAssessment = 12,
    ImplementationTimeline = 13,
    ExitStrategy = 14,

    // OBNL-specific Sections (Non-profit)
    MissionVision = 100,
    ImpactMeasurement = 101,
    GovernanceStructure = 102,

    // Lean Canvas-specific Sections
    Problem = 200,
    Solution = 201,
    UniqueValueProposition = 202,
    Channels = 203,
    CustomerSegments = 204,
    CostStructure = 205,
    RevenueStreams = 206,
    KeyMetrics = 207,
    UnfairAdvantage = 208
}

namespace Sqordia.Domain.Enums;

/// <summary>
/// Type of business plan being created
/// Maps to organization types but allows flexibility
/// </summary>
public enum BusinessPlanType
{
    /// <summary>
    /// Traditional business plan for startups/SMEs
    /// Focus: Revenue, market, profitability, growth
    /// </summary>
    BusinessPlan = 0,
    
    /// <summary>
    /// Strategic plan for non-profits (OBNL)
    /// Focus: Mission, impact, grants, beneficiaries
    /// </summary>
    StrategicPlan = 1,
    
    /// <summary>
    /// One-page lean business plan
    /// Focus: Quick validation, MVP, iteration
    /// </summary>
    LeanCanvas = 2
}


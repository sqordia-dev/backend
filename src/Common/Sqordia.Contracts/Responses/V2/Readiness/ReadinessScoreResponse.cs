namespace Sqordia.Contracts.Responses.V2.Readiness;

/// <summary>
/// Bank-ready percentage score response
/// </summary>
public class ReadinessScoreResponse
{
    public Guid BusinessPlanId { get; set; }

    /// <summary>
    /// Overall readiness score (0-100)
    /// </summary>
    public decimal OverallScore { get; set; }

    /// <summary>
    /// Consistency score (50% weight) - alignment across sections
    /// </summary>
    public decimal ConsistencyScore { get; set; }

    /// <summary>
    /// Risk mitigation score (30% weight) - risk coverage
    /// </summary>
    public decimal RiskMitigationScore { get; set; }

    /// <summary>
    /// Completeness score (20% weight) - section completion
    /// </summary>
    public decimal CompletenessScore { get; set; }

    /// <summary>
    /// Readiness level: NotReady, Developing, Ready, BankReady
    /// </summary>
    public required string ReadinessLevel { get; set; }

    /// <summary>
    /// Actionable recommendations to improve score
    /// </summary>
    public required List<string> Recommendations { get; set; }

    public DateTime CalculatedAt { get; set; }
}

/// <summary>
/// Detailed breakdown of readiness by section
/// </summary>
public class ReadinessBreakdownResponse
{
    public Guid BusinessPlanId { get; set; }
    public required List<SectionReadiness> Sections { get; set; }
    public required List<string> MissingElements { get; set; }
    public required List<string> InconsistentElements { get; set; }
    public required List<string> RiskGaps { get; set; }
    public DateTime CalculatedAt { get; set; }
}

/// <summary>
/// Readiness status for a single section
/// </summary>
public class SectionReadiness
{
    public required string SectionName { get; set; }
    public decimal Score { get; set; }
    public bool IsComplete { get; set; }
    public required List<string> Issues { get; set; }
    public required List<string> Strengths { get; set; }
}

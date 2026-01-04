namespace Sqordia.Contracts.Responses.AI;

/// <summary>
/// Response containing AI-generated risk mitigation analysis
/// </summary>
public class RiskMitigationResponse
{
    /// <summary>
    /// The business plan ID
    /// </summary>
    public required Guid BusinessPlanId { get; set; }
    
    /// <summary>
    /// List of identified risks with mitigation strategies
    /// </summary>
    public required List<RiskAnalysis> Risks { get; set; }
    
    /// <summary>
    /// Overall risk assessment summary
    /// </summary>
    public required string Summary { get; set; }
    
    /// <summary>
    /// Overall risk level (Low, Medium, High, Critical)
    /// </summary>
    public required string OverallRiskLevel { get; set; }
    
    /// <summary>
    /// Timestamp when analysis was generated
    /// </summary>
    public required DateTime GeneratedAt { get; set; }
    
    /// <summary>
    /// AI model used for generation
    /// </summary>
    public required string Model { get; set; }
    
    /// <summary>
    /// Language of the generated analysis
    /// </summary>
    public required string Language { get; set; }
}

/// <summary>
/// Individual risk analysis with mitigation plan
/// </summary>
public class RiskAnalysis
{
    /// <summary>
    /// Risk title
    /// </summary>
    public required string Title { get; set; }
    
    /// <summary>
    /// Detailed description of the risk
    /// </summary>
    public required string Description { get; set; }
    
    /// <summary>
    /// Risk category (e.g., "financial", "market", "operational", "technical", "regulatory")
    /// </summary>
    public required string Category { get; set; }
    
    /// <summary>
    /// Likelihood of the risk occurring (Low, Medium, High)
    /// </summary>
    public required string Likelihood { get; set; }
    
    /// <summary>
    /// Impact if the risk occurs (Low, Medium, High, Critical)
    /// </summary>
    public required string Impact { get; set; }
    
    /// <summary>
    /// Risk score (1-10, calculated from likelihood and impact)
    /// </summary>
    public required int RiskScore { get; set; }
    
    /// <summary>
    /// Mitigation strategies to address this risk
    /// </summary>
    public required List<MitigationStrategy> MitigationStrategies { get; set; }
    
    /// <summary>
    /// Contingency plan if the risk materializes
    /// </summary>
    public string? ContingencyPlan { get; set; }
}

/// <summary>
/// Mitigation strategy for a risk
/// </summary>
public class MitigationStrategy
{
    /// <summary>
    /// Strategy title
    /// </summary>
    public required string Title { get; set; }
    
    /// <summary>
    /// Detailed description of the mitigation strategy
    /// </summary>
    public required string Description { get; set; }
    
    /// <summary>
    /// Implementation priority (High, Medium, Low)
    /// </summary>
    public required string Priority { get; set; }
    
    /// <summary>
    /// Estimated cost or effort required
    /// </summary>
    public string? EstimatedCost { get; set; }
    
    /// <summary>
    /// Expected effectiveness of this mitigation strategy
    /// </summary>
    public required string Effectiveness { get; set; }
}


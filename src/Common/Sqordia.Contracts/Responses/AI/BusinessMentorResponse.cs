namespace Sqordia.Contracts.Responses.AI;

/// <summary>
/// Response containing comprehensive business mentor AI analysis
/// </summary>
public class BusinessMentorResponse
{
    /// <summary>
    /// The business plan ID
    /// </summary>
    public required Guid BusinessPlanId { get; set; }
    
    /// <summary>
    /// Executive summary of the analysis
    /// </summary>
    public required string ExecutiveSummary { get; set; }
    
    /// <summary>
    /// Identified opportunities
    /// </summary>
    public required List<Opportunity> Opportunities { get; set; }
    
    /// <summary>
    /// Identified weaknesses and areas for improvement
    /// </summary>
    public required List<Weakness> Weaknesses { get; set; }
    
    /// <summary>
    /// Strategic recommendations
    /// </summary>
    public required List<StrategicRecommendation> Recommendations { get; set; }
    
    /// <summary>
    /// Overall business health score (0-100)
    /// </summary>
    public required int HealthScore { get; set; }
    
    /// <summary>
    /// Key strengths identified
    /// </summary>
    public required List<string> Strengths { get; set; }
    
    /// <summary>
    /// Areas requiring immediate attention
    /// </summary>
    public required List<string> CriticalAreas { get; set; }
    
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
/// Identified business opportunity
/// </summary>
public class Opportunity
{
    /// <summary>
    /// Opportunity title
    /// </summary>
    public required string Title { get; set; }
    
    /// <summary>
    /// Detailed description of the opportunity
    /// </summary>
    public required string Description { get; set; }
    
    /// <summary>
    /// Potential impact of pursuing this opportunity (High, Medium, Low)
    /// </summary>
    public required string PotentialImpact { get; set; }
    
    /// <summary>
    /// Recommended actions to capitalize on this opportunity
    /// </summary>
    public required List<string> RecommendedActions { get; set; }
    
    /// <summary>
    /// Estimated time to realize this opportunity
    /// </summary>
    public string? EstimatedTimeframe { get; set; }
}

/// <summary>
/// Identified business weakness
/// </summary>
public class Weakness
{
    /// <summary>
    /// Weakness title
    /// </summary>
    public required string Title { get; set; }
    
    /// <summary>
    /// Detailed description of the weakness
    /// </summary>
    public required string Description { get; set; }
    
    /// <summary>
    /// Severity of the weakness (Critical, High, Medium, Low)
    /// </summary>
    public required string Severity { get; set; }
    
    /// <summary>
    /// Recommended actions to address this weakness
    /// </summary>
    public required List<string> RecommendedActions { get; set; }
    
    /// <summary>
    /// Impact if not addressed
    /// </summary>
    public required string ImpactIfNotAddressed { get; set; }
}

/// <summary>
/// Strategic recommendation from business mentor
/// </summary>
public class StrategicRecommendation
{
    /// <summary>
    /// Recommendation title
    /// </summary>
    public required string Title { get; set; }
    
    /// <summary>
    /// Detailed description of the recommendation
    /// </summary>
    public required string Description { get; set; }
    
    /// <summary>
    /// Category of the recommendation
    /// </summary>
    public required string Category { get; set; }
    
    /// <summary>
    /// Priority level (High, Medium, Low)
    /// </summary>
    public required string Priority { get; set; }
    
    /// <summary>
    /// Expected benefits of implementing this recommendation
    /// </summary>
    public required string ExpectedBenefits { get; set; }
    
    /// <summary>
    /// Implementation steps
    /// </summary>
    public required List<string> ImplementationSteps { get; set; }
}


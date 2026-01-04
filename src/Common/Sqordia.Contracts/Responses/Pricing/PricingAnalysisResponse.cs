namespace Sqordia.Contracts.Responses.Pricing;

/// <summary>
/// Response containing pricing analysis and competitive report
/// </summary>
public class PricingAnalysisResponse
{
    /// <summary>
    /// The business plan ID
    /// </summary>
    public required Guid BusinessPlanId { get; set; }
    
    /// <summary>
    /// Pricing grid with different pricing strategies
    /// </summary>
    public required PricingGrid PricingGrid { get; set; }
    
    /// <summary>
    /// Recommended pricing strategy
    /// </summary>
    public required PricingStrategy RecommendedStrategy { get; set; }
    
    /// <summary>
    /// Competitive analysis report
    /// </summary>
    public required CompetitiveAnalysisReport CompetitiveAnalysis { get; set; }
    
    /// <summary>
    /// Market positioning recommendations
    /// </summary>
    public required List<string> MarketPositioningRecommendations { get; set; }
    
    /// <summary>
    /// Timestamp when analysis was generated
    /// </summary>
    public required DateTime GeneratedAt { get; set; }
    
    /// <summary>
    /// Language of the analysis
    /// </summary>
    public required string Language { get; set; }
}

/// <summary>
/// Pricing grid with different pricing options
/// </summary>
public class PricingGrid
{
    /// <summary>
    /// Low-end pricing option
    /// </summary>
    public required PricingOption LowEnd { get; set; }
    
    /// <summary>
    /// Mid-range pricing option
    /// </summary>
    public required PricingOption MidRange { get; set; }
    
    /// <summary>
    /// Premium pricing option
    /// </summary>
    public required PricingOption Premium { get; set; }
    
    /// <summary>
    /// Value-based pricing option
    /// </summary>
    public required PricingOption ValueBased { get; set; }
}

/// <summary>
/// Individual pricing option
/// </summary>
public class PricingOption
{
    /// <summary>
    /// Price per unit
    /// </summary>
    public required decimal Price { get; set; }
    
    /// <summary>
    /// Pricing strategy name
    /// </summary>
    public required string StrategyName { get; set; }
    
    /// <summary>
    /// Description of this pricing strategy
    /// </summary>
    public required string Description { get; set; }
    
    /// <summary>
    /// Expected profit margin percentage
    /// </summary>
    public required decimal ProfitMargin { get; set; }
    
    /// <summary>
    /// Expected market share percentage
    /// </summary>
    public decimal? ExpectedMarketShare { get; set; }
    
    /// <summary>
    /// Pros of this pricing strategy
    /// </summary>
    public required List<string> Pros { get; set; }
    
    /// <summary>
    /// Cons of this pricing strategy
    /// </summary>
    public required List<string> Cons { get; set; }
    
    /// <summary>
    /// Target customer segment
    /// </summary>
    public required string TargetSegment { get; set; }
}

/// <summary>
/// Recommended pricing strategy details
/// </summary>
public class PricingStrategy
{
    /// <summary>
    /// Recommended price
    /// </summary>
    public required decimal RecommendedPrice { get; set; }
    
    /// <summary>
    /// Strategy name
    /// </summary>
    public required string StrategyName { get; set; }
    
    /// <summary>
    /// Reasoning behind the recommendation
    /// </summary>
    public required string Reasoning { get; set; }
    
    /// <summary>
    /// Expected revenue at this price point
    /// </summary>
    public decimal? ExpectedRevenue { get; set; }
    
    /// <summary>
    /// Implementation steps
    /// </summary>
    public required List<string> ImplementationSteps { get; set; }
}

/// <summary>
/// Competitive analysis report
/// </summary>
public class CompetitiveAnalysisReport
{
    /// <summary>
    /// Summary of competitive landscape
    /// </summary>
    public required string Summary { get; set; }
    
    /// <summary>
    /// List of analyzed competitors
    /// </summary>
    public required List<CompetitorAnalysis> Competitors { get; set; }
    
    /// <summary>
    /// Competitive positioning
    /// </summary>
    public required string CompetitivePositioning { get; set; }
    
    /// <summary>
    /// Key differentiators
    /// </summary>
    public required List<string> KeyDifferentiators { get; set; }
    
    /// <summary>
    /// Market opportunities
    /// </summary>
    public required List<string> MarketOpportunities { get; set; }
}

/// <summary>
/// Individual competitor analysis
/// </summary>
public class CompetitorAnalysis
{
    /// <summary>
    /// Competitor name
    /// </summary>
    public required string Name { get; set; }
    
    /// <summary>
    /// Competitor price
    /// </summary>
    public decimal? Price { get; set; }
    
    /// <summary>
    /// Competitor strengths
    /// </summary>
    public required List<string> Strengths { get; set; }
    
    /// <summary>
    /// Competitor weaknesses
    /// </summary>
    public required List<string> Weaknesses { get; set; }
    
    /// <summary>
    /// Market share estimate
    /// </summary>
    public decimal? MarketShare { get; set; }
    
    /// <summary>
    /// Competitive threat level (Low, Medium, High)
    /// </summary>
    public required string ThreatLevel { get; set; }
}


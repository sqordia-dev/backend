using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.Pricing;

/// <summary>
/// Request for pricing and market analysis
/// </summary>
public class PricingAnalysisRequest
{
    /// <summary>
    /// The business plan ID
    /// </summary>
    [Required]
    public required Guid BusinessPlanId { get; set; }
    
    /// <summary>
    /// Product or service name
    /// </summary>
    [Required]
    [StringLength(200)]
    public required string ProductName { get; set; }
    
    /// <summary>
    /// Product description
    /// </summary>
    [StringLength(1000)]
    public string? ProductDescription { get; set; }
    
    /// <summary>
    /// Target market
    /// </summary>
    [StringLength(200)]
    public string? TargetMarket { get; set; }
    
    /// <summary>
    /// Cost per unit (production cost)
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal? CostPerUnit { get; set; }
    
    /// <summary>
    /// Estimated market size
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal? MarketSize { get; set; }
    
    /// <summary>
    /// Industry or sector
    /// </summary>
    [StringLength(200)]
    public string? Industry { get; set; }
    
    /// <summary>
    /// Language for the analysis (fr or en)
    /// </summary>
    [StringLength(2, MinimumLength = 2)]
    public string Language { get; set; } = "fr";
    
    /// <summary>
    /// Competitor information (optional)
    /// </summary>
    public List<CompetitorInfo>? Competitors { get; set; }
}

/// <summary>
/// Competitor information for analysis
/// </summary>
public class CompetitorInfo
{
    public required string Name { get; set; }
    public decimal? Price { get; set; }
    public string? Description { get; set; }
    public string? Strengths { get; set; }
    public string? Weaknesses { get; set; }
}


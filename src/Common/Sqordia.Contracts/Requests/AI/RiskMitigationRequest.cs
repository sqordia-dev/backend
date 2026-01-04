using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.AI;

/// <summary>
/// Request to generate AI risk mitigation analysis for a business plan
/// </summary>
public class RiskMitigationRequest
{
    /// <summary>
    /// The business plan ID
    /// </summary>
    [Required]
    public required Guid BusinessPlanId { get; set; }
    
    /// <summary>
    /// The business plan type for context
    /// </summary>
    [Required]
    public required string PlanType { get; set; }
    
    /// <summary>
    /// Industry or sector context
    /// </summary>
    [StringLength(200)]
    public string? Industry { get; set; }
    
    /// <summary>
    /// Minimum number of risks to identify (default: 3)
    /// </summary>
    [Range(1, 10)]
    public int MinRiskCount { get; set; } = 3;
    
    /// <summary>
    /// Language for the analysis (fr or en, default: fr)
    /// </summary>
    [StringLength(2, MinimumLength = 2)]
    public string Language { get; set; } = "fr";
    
    /// <summary>
    /// Risk categories to focus on (e.g., "financial", "market", "operational", "technical")
    /// </summary>
    public List<string>? RiskCategories { get; set; }
}


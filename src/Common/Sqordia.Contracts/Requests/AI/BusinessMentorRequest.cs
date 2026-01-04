using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.AI;

/// <summary>
/// Request for comprehensive business mentor AI analysis
/// </summary>
public class BusinessMentorRequest
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
    /// Language for the analysis (fr or en, default: fr)
    /// </summary>
    [StringLength(2, MinimumLength = 2)]
    public string Language { get; set; } = "fr";
    
    /// <summary>
    /// Specific areas to analyze (e.g., "opportunities", "weaknesses", "strategy", "financial")
    /// If null, performs comprehensive analysis
    /// </summary>
    public List<string>? AnalysisAreas { get; set; }
}


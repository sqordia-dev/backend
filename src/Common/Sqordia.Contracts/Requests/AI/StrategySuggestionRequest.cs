using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.AI;

/// <summary>
/// Request to generate AI strategy suggestions for a business plan
/// </summary>
public class StrategySuggestionRequest
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
    /// Number of strategy suggestions to generate (1-10, default: 3)
    /// </summary>
    [Range(1, 10)]
    public int SuggestionCount { get; set; } = 3;
    
    /// <summary>
    /// Language for the suggestions (fr or en, default: fr)
    /// </summary>
    [StringLength(2, MinimumLength = 2)]
    public string Language { get; set; } = "fr";
    
    /// <summary>
    /// Focus area for strategies (e.g., "growth", "marketing", "operations")
    /// </summary>
    [StringLength(100)]
    public string? FocusArea { get; set; }
}


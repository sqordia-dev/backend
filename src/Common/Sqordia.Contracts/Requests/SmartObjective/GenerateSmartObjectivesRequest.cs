using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.SmartObjective;

/// <summary>
/// Request to generate SMART objectives for a business plan
/// </summary>
public class GenerateSmartObjectivesRequest
{
    /// <summary>
    /// The business plan ID
    /// </summary>
    [Required]
    public required Guid BusinessPlanId { get; set; }
    
    /// <summary>
    /// Number of objectives to generate (default: 5)
    /// </summary>
    [Range(1, 20)]
    public int ObjectiveCount { get; set; } = 5;
    
    /// <summary>
    /// Categories to focus on (e.g., "Revenue", "Marketing", "Operations")
    /// </summary>
    public List<string>? Categories { get; set; }
    
    /// <summary>
    /// Time horizon in months (default: 12)
    /// </summary>
    [Range(1, 60)]
    public int TimeHorizonMonths { get; set; } = 12;
    
    /// <summary>
    /// Language for generation (fr or en)
    /// </summary>
    [StringLength(2, MinimumLength = 2)]
    public string Language { get; set; } = "fr";
}


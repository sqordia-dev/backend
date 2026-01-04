using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.Sections;

/// <summary>
/// Request to improve a business plan section using AI
/// </summary>
public class SectionImprovementRequest
{
    /// <summary>
    /// The current content of the section to improve
    /// </summary>
    [Required]
    [StringLength(10000, MinimumLength = 10)]
    public required string CurrentContent { get; set; }
    
    /// <summary>
    /// The type of improvement requested (improve, expand, simplify)
    /// </summary>
    [Required]
    [StringLength(20)]
    public required string ImprovementType { get; set; } // "improve", "expand", "simplify"
    
    /// <summary>
    /// Language for the improvement (fr or en)
    /// </summary>
    [Required]
    [StringLength(2, MinimumLength = 2)]
    public string Language { get; set; } = "fr";
    
    /// <summary>
    /// Business plan type context (BusinessPlan, StrategicPlan, LeanCanvas)
    /// </summary>
    [Required]
    [StringLength(50)]
    public required string PlanType { get; set; }
    
    /// <summary>
    /// Specific instructions for the improvement
    /// </summary>
    [StringLength(1000)]
    public string? Instructions { get; set; }
    
    /// <summary>
    /// Target audience for the content (e.g., "investors", "stakeholders", "general")
    /// </summary>
    [StringLength(100)]
    public string? TargetAudience { get; set; }
    
    /// <summary>
    /// Industry context for more relevant improvements
    /// </summary>
    [StringLength(200)]
    public string? IndustryContext { get; set; }
    
    /// <summary>
    /// Maximum length for the improved content (optional)
    /// </summary>
    [Range(100, 5000)]
    public int? MaxLength { get; set; }
    
    /// <summary>
    /// Tone for the content (professional, casual, technical, etc.)
    /// </summary>
    [StringLength(50)]
    public string? Tone { get; set; }
}

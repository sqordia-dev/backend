using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.Admin;

/// <summary>
/// Request to create a new AI prompt template
/// </summary>
public class CreateAIPromptRequest
{
    /// <summary>
    /// Name of the prompt template
    /// </summary>
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public required string Name { get; set; }
    
    /// <summary>
    /// Description of what this prompt is used for
    /// </summary>
    [Required]
    [StringLength(1000, MinimumLength = 10)]
    public required string Description { get; set; }
    
    /// <summary>
    /// Category of the prompt (e.g., QuestionSuggestions, SectionImprovement, ContentGeneration)
    /// </summary>
    [Required]
    [StringLength(100)]
    public required string Category { get; set; }
    
    /// <summary>
    /// Business plan type this prompt is for (BusinessPlan, StrategicPlan, LeanCanvas)
    /// </summary>
    [Required]
    [StringLength(50)]
    public required string PlanType { get; set; }
    
    /// <summary>
    /// Language of the prompt (fr or en)
    /// </summary>
    [Required]
    [StringLength(2, MinimumLength = 2)]
    public required string Language { get; set; }
    
    /// <summary>
    /// Section name this prompt is for (e.g., "ExecutiveSummary", "MarketAnalysis")
    /// </summary>
    [StringLength(100)]
    public string? SectionName { get; set; }
    
    /// <summary>
    /// The system prompt that defines the AI's role and behavior
    /// </summary>
    [Required]
    [StringLength(5000, MinimumLength = 50)]
    public required string SystemPrompt { get; set; }
    
    /// <summary>
    /// Template for the user prompt with placeholders for variables
    /// </summary>
    [Required]
    [StringLength(2000, MinimumLength = 20)]
    public required string UserPromptTemplate { get; set; }
    
    /// <summary>
    /// JSON string containing available variables and their descriptions
    /// </summary>
    [StringLength(2000)]
    public string? Variables { get; set; }
    
    /// <summary>
    /// Additional notes about this prompt
    /// </summary>
    [StringLength(1000)]
    public string? Notes { get; set; }
}

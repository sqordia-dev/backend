using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.Admin;

/// <summary>
/// Request to update an existing AI prompt template
/// </summary>
public class UpdateAIPromptRequest
{
    /// <summary>
    /// Name of the prompt template
    /// </summary>
    [StringLength(200, MinimumLength = 3)]
    public string? Name { get; set; }
    
    /// <summary>
    /// Description of what this prompt is used for
    /// </summary>
    [StringLength(1000, MinimumLength = 10)]
    public string? Description { get; set; }
    
    /// <summary>
    /// The system prompt that defines the AI's role and behavior
    /// </summary>
    [StringLength(5000, MinimumLength = 50)]
    public string? SystemPrompt { get; set; }
    
    /// <summary>
    /// Template for the user prompt with placeholders for variables
    /// </summary>
    [StringLength(2000, MinimumLength = 20)]
    public string? UserPromptTemplate { get; set; }
    
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
    
    /// <summary>
    /// Section name this prompt is for (e.g., "ExecutiveSummary", "MarketAnalysis")
    /// </summary>
    [StringLength(100)]
    public string? SectionName { get; set; }
    
    /// <summary>
    /// Whether this prompt is active
    /// </summary>
    public bool? IsActive { get; set; }
}

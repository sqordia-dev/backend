using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.BusinessPlan;

/// <summary>
/// Request to update a business plan section
/// </summary>
public class UpdateSectionRequest
{
    /// <summary>
    /// The section content
    /// </summary>
    [Required]
    [StringLength(50000, MinimumLength = 1)]
    public required string Content { get; set; }
    
    /// <summary>
    /// Whether this update is from AI generation
    /// </summary>
    public bool IsAIGenerated { get; set; } = false;
    
    /// <summary>
    /// Whether this is a manual edit by the user
    /// </summary>
    public bool IsManualEdit { get; set; } = true;
    
    /// <summary>
    /// Section status (draft, review, final)
    /// </summary>
    [StringLength(20)]
    public string Status { get; set; } = "draft";
    
    /// <summary>
    /// Tags to associate with the section
    /// </summary>
    public List<string> Tags { get; set; } = new();
    
    /// <summary>
    /// Notes about the update
    /// </summary>
    [StringLength(500)]
    public string? Notes { get; set; }
}

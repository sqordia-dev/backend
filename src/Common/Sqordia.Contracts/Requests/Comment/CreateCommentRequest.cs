using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.Comment;

/// <summary>
/// Request to create a comment on a business plan section
/// </summary>
public class CreateCommentRequest
{
    /// <summary>
    /// The business plan ID
    /// </summary>
    [Required]
    public required Guid BusinessPlanId { get; set; }
    
    /// <summary>
    /// The section name (e.g., "executive-summary", "market-analysis")
    /// </summary>
    [Required]
    [StringLength(100)]
    public required string SectionName { get; set; }
    
    /// <summary>
    /// The comment text
    /// </summary>
    [Required]
    [StringLength(2000, MinimumLength = 1)]
    public required string CommentText { get; set; }
    
    /// <summary>
    /// Parent comment ID for replies (optional)
    /// </summary>
    public Guid? ParentCommentId { get; set; }
}


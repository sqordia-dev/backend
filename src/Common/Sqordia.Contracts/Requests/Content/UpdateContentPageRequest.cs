using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.Content;

/// <summary>
/// Request to update a content page
/// </summary>
public class UpdateContentPageRequest
{
    /// <summary>
    /// Page title
    /// </summary>
    [Required]
    [StringLength(200)]
    public required string Title { get; set; }
    
    /// <summary>
    /// Page content (HTML or Markdown)
    /// </summary>
    [Required]
    public required string Content { get; set; }
    
    /// <summary>
    /// Language (fr or en)
    /// </summary>
    [StringLength(2)]
    public string Language { get; set; } = "fr";
    
    /// <summary>
    /// Whether to publish immediately
    /// </summary>
    public bool Publish { get; set; } = false;
}


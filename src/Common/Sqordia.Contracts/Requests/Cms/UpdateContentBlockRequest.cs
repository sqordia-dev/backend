using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.Cms;

/// <summary>
/// Request to update an existing content block
/// </summary>
public class UpdateContentBlockRequest
{
    /// <summary>
    /// The updated content value
    /// </summary>
    [Required]
    public required string Content { get; set; }

    /// <summary>
    /// Optional updated sort order
    /// </summary>
    public int? SortOrder { get; set; }

    /// <summary>
    /// Optional updated JSON metadata
    /// </summary>
    [StringLength(2000)]
    public string? Metadata { get; set; }
}

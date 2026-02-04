using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.Cms;

/// <summary>
/// Request to update multiple content blocks in a single operation
/// </summary>
public class BulkUpdateContentBlocksRequest
{
    /// <summary>
    /// List of content blocks to update
    /// </summary>
    [Required]
    public required List<UpdateContentBlockItem> Blocks { get; set; }
}

/// <summary>
/// Individual content block update item within a bulk update
/// </summary>
public class UpdateContentBlockItem
{
    /// <summary>
    /// ID of the content block to update
    /// </summary>
    [Required]
    public required Guid Id { get; set; }

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

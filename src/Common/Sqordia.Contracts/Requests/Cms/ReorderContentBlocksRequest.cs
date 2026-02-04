using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.Cms;

/// <summary>
/// Request to reorder content blocks within a section
/// </summary>
public class ReorderContentBlocksRequest
{
    /// <summary>
    /// List of block reorder instructions
    /// </summary>
    [Required]
    public required List<ReorderItem> Items { get; set; }
}

/// <summary>
/// Individual reorder instruction for a content block
/// </summary>
public class ReorderItem
{
    /// <summary>
    /// ID of the content block to reorder
    /// </summary>
    [Required]
    public required Guid BlockId { get; set; }

    /// <summary>
    /// New sort order position for the block
    /// </summary>
    [Required]
    public required int NewSortOrder { get; set; }
}

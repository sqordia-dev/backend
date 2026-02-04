using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.Cms;

/// <summary>
/// Request to create a new content block within a CMS version
/// </summary>
public class CreateContentBlockRequest
{
    /// <summary>
    /// Unique key identifying this content block
    /// </summary>
    [Required]
    [StringLength(200)]
    public required string BlockKey { get; set; }

    /// <summary>
    /// Type of content block (Text, RichText, Image, Link, Json, Number, Boolean)
    /// </summary>
    [Required]
    public required string BlockType { get; set; }

    /// <summary>
    /// The content value for this block
    /// </summary>
    [Required]
    public required string Content { get; set; }

    /// <summary>
    /// Language code (fr or en)
    /// </summary>
    [StringLength(5)]
    public string Language { get; set; } = "fr";

    /// <summary>
    /// Display order within the section
    /// </summary>
    public int SortOrder { get; set; } = 0;

    /// <summary>
    /// Section this block belongs to
    /// </summary>
    [Required]
    [StringLength(200)]
    public required string SectionKey { get; set; }

    /// <summary>
    /// Optional JSON metadata for the block
    /// </summary>
    [StringLength(2000)]
    public string? Metadata { get; set; }
}

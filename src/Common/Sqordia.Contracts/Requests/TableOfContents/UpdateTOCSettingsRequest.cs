using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.TableOfContents;

/// <summary>
/// Request to update table of contents settings for a business plan
/// </summary>
public class UpdateTOCSettingsRequest
{
    /// <summary>
    /// The TOC style preset: classic, modern, minimal, magazine, corporate
    /// </summary>
    [Required]
    [RegularExpression(@"^(classic|modern|minimal|magazine|corporate)$",
        ErrorMessage = "Style must be 'classic', 'modern', 'minimal', 'magazine', or 'corporate'")]
    public required string Style { get; set; }

    /// <summary>
    /// Whether to show page numbers in the TOC
    /// </summary>
    public bool ShowPageNumbers { get; set; } = true;

    /// <summary>
    /// Whether to show section icons in the TOC
    /// </summary>
    public bool ShowIcons { get; set; } = true;

    /// <summary>
    /// Whether to show category headers (grouping sections)
    /// </summary>
    public bool ShowCategoryHeaders { get; set; } = true;

    /// <summary>
    /// JSON-serialized extended style settings for future customization
    /// </summary>
    public string? StyleSettingsJson { get; set; }
}

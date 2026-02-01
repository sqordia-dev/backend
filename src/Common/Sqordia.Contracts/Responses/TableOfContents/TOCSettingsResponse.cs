namespace Sqordia.Contracts.Responses.TableOfContents;

/// <summary>
/// Response containing table of contents settings for a business plan
/// </summary>
public class TOCSettingsResponse
{
    public Guid Id { get; set; }
    public Guid BusinessPlanId { get; set; }

    /// <summary>
    /// The TOC style preset: classic, modern, minimal, magazine, corporate
    /// </summary>
    public string Style { get; set; } = "classic";

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
    /// JSON-serialized extended style settings
    /// </summary>
    public string? StyleSettingsJson { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

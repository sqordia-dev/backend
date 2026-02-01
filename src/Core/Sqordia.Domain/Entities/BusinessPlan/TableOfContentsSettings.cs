using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities.BusinessPlan;

/// <summary>
/// Table of Contents settings for a business plan
/// Contains style preset selection and display options
/// </summary>
public class TableOfContentsSettings : BaseAuditableEntity
{
    public Guid BusinessPlanId { get; private set; }

    /// <summary>
    /// The selected TOC style preset: classic, modern, minimal, magazine, corporate
    /// </summary>
    public string Style { get; private set; } = "classic";

    /// <summary>
    /// Whether to show page numbers in the TOC
    /// </summary>
    public bool ShowPageNumbers { get; private set; } = true;

    /// <summary>
    /// Whether to show section icons in the TOC
    /// </summary>
    public bool ShowIcons { get; private set; } = true;

    /// <summary>
    /// Whether to show category headers (grouping sections)
    /// </summary>
    public bool ShowCategoryHeaders { get; private set; } = true;

    /// <summary>
    /// JSON-serialized extended style settings for future customization
    /// </summary>
    public string? StyleSettingsJson { get; private set; }

    // Navigation
    public BusinessPlan BusinessPlan { get; private set; } = null!;

    private TableOfContentsSettings() { } // EF Core constructor

    public TableOfContentsSettings(Guid businessPlanId)
    {
        BusinessPlanId = businessPlanId;
    }

    public void UpdateStyle(string style)
    {
        var validStyles = new[] { "classic", "modern", "minimal", "magazine", "corporate" };
        if (!validStyles.Contains(style.ToLowerInvariant()))
        {
            throw new ArgumentException($"Invalid TOC style. Must be one of: {string.Join(", ", validStyles)}", nameof(style));
        }
        Style = style.ToLowerInvariant();
    }

    public void UpdateDisplayOptions(bool showPageNumbers, bool showIcons, bool showCategoryHeaders)
    {
        ShowPageNumbers = showPageNumbers;
        ShowIcons = showIcons;
        ShowCategoryHeaders = showCategoryHeaders;
    }

    public void UpdateStyleSettings(string? styleSettingsJson)
    {
        StyleSettingsJson = styleSettingsJson;
    }
}

using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities.Cms;

/// <summary>
/// Represents a page definition in the CMS registry.
/// Pages are top-level content containers that group related sections.
/// </summary>
public class CmsPage : BaseAuditableEntity
{
    /// <summary>
    /// Unique key identifier for the page (e.g., "landing", "dashboard").
    /// Used for routing and content lookup.
    /// </summary>
    public string Key { get; private set; } = string.Empty;

    /// <summary>
    /// Display label for the page in the CMS navigation.
    /// </summary>
    public string Label { get; private set; } = string.Empty;

    /// <summary>
    /// Optional description of the page's purpose.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Order in which this page appears in the navigation.
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// Whether this page is currently active and visible in the CMS.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Lucide icon name for display in the navigation (e.g., "Globe", "LayoutDashboard").
    /// </summary>
    public string? IconName { get; private set; }

    /// <summary>
    /// Optional special renderer identifier for pages that don't use standard CMS blocks.
    /// For example, "question-templates" for the Questions page.
    /// </summary>
    public string? SpecialRenderer { get; private set; }

    /// <summary>
    /// Collection of sections belonging to this page.
    /// </summary>
    public ICollection<CmsSection> Sections { get; private set; } = new List<CmsSection>();

    private CmsPage() { } // EF Core constructor

    public CmsPage(
        string key,
        string label,
        int sortOrder,
        string? description = null,
        string? iconName = null,
        string? specialRenderer = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(label);

        Key = key.ToLowerInvariant().Trim();
        Label = label.Trim();
        Description = description?.Trim();
        SortOrder = sortOrder;
        IconName = iconName?.Trim();
        SpecialRenderer = specialRenderer?.Trim();
    }

    /// <summary>
    /// Updates the page's display properties.
    /// </summary>
    public void Update(
        string label,
        string? description,
        int sortOrder,
        string? iconName,
        string? specialRenderer)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(label);

        Label = label.Trim();
        Description = description?.Trim();
        SortOrder = sortOrder;
        IconName = iconName?.Trim();
        SpecialRenderer = specialRenderer?.Trim();
    }

    /// <summary>
    /// Activates the page, making it visible in the CMS.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Deactivates the page, hiding it from the CMS navigation.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Adds a section to this page.
    /// </summary>
    public void AddSection(CmsSection section)
    {
        ArgumentNullException.ThrowIfNull(section);
        Sections.Add(section);
    }

    /// <summary>
    /// Removes a section from this page.
    /// </summary>
    public void RemoveSection(Guid sectionId)
    {
        var section = Sections.FirstOrDefault(s => s.Id == sectionId);
        if (section != null)
        {
            Sections.Remove(section);
        }
    }
}

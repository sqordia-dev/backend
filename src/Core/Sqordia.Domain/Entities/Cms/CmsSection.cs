using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities.Cms;

/// <summary>
/// Represents a section definition within a CMS page.
/// Sections group related content blocks within a page.
/// </summary>
public class CmsSection : BaseAuditableEntity
{
    /// <summary>
    /// Reference to the parent page.
    /// </summary>
    public Guid CmsPageId { get; private set; }

    /// <summary>
    /// Unique key identifier for the section (e.g., "landing.hero", "dashboard.labels").
    /// Typically formatted as "pageKey.sectionName".
    /// </summary>
    public string Key { get; private set; } = string.Empty;

    /// <summary>
    /// Display label for the section in the CMS navigation.
    /// </summary>
    public string Label { get; private set; } = string.Empty;

    /// <summary>
    /// Optional description of the section's purpose.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Order in which this section appears within its parent page.
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// Whether this section is currently active and visible.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Lucide icon name for display in the navigation.
    /// </summary>
    public string? IconName { get; private set; }

    /// <summary>
    /// Navigation property to the parent page.
    /// </summary>
    public CmsPage Page { get; private set; } = null!;

    /// <summary>
    /// Collection of block definitions belonging to this section.
    /// </summary>
    public ICollection<CmsBlockDefinition> BlockDefinitions { get; private set; } = new List<CmsBlockDefinition>();

    private CmsSection() { } // EF Core constructor

    public CmsSection(
        Guid cmsPageId,
        string key,
        string label,
        int sortOrder,
        string? description = null,
        string? iconName = null)
    {
        if (cmsPageId == Guid.Empty)
            throw new ArgumentException("CmsPageId cannot be empty", nameof(cmsPageId));
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(label);

        CmsPageId = cmsPageId;
        Key = key.ToLowerInvariant().Trim();
        Label = label.Trim();
        Description = description?.Trim();
        SortOrder = sortOrder;
        IconName = iconName?.Trim();
    }

    /// <summary>
    /// Updates the section's display properties.
    /// </summary>
    public void Update(
        string label,
        string? description,
        int sortOrder,
        string? iconName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(label);

        Label = label.Trim();
        Description = description?.Trim();
        SortOrder = sortOrder;
        IconName = iconName?.Trim();
    }

    /// <summary>
    /// Activates the section, making it visible in the CMS.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Deactivates the section, hiding it from the CMS navigation.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Adds a block definition to this section.
    /// </summary>
    public void AddBlockDefinition(CmsBlockDefinition blockDefinition)
    {
        ArgumentNullException.ThrowIfNull(blockDefinition);
        BlockDefinitions.Add(blockDefinition);
    }

    /// <summary>
    /// Removes a block definition from this section.
    /// </summary>
    public void RemoveBlockDefinition(Guid blockDefinitionId)
    {
        var blockDef = BlockDefinitions.FirstOrDefault(b => b.Id == blockDefinitionId);
        if (blockDef != null)
        {
            BlockDefinitions.Remove(blockDef);
        }
    }
}

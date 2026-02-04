using Sqordia.Domain.Common;
using Sqordia.Domain.Enums;

namespace Sqordia.Domain.Entities.Cms;

/// <summary>
/// Represents an individual content block within a CMS version.
/// Each block holds a piece of content identified by a key, type, language, and section.
/// </summary>
public class CmsContentBlock : BaseAuditableEntity
{
    public Guid CmsVersionId { get; private set; }
    public string BlockKey { get; private set; } = null!;
    public CmsBlockType BlockType { get; private set; }
    public string Content { get; private set; } = null!;
    public string Language { get; private set; } = "fr";
    public int SortOrder { get; private set; }
    public string SectionKey { get; private set; } = null!;
    public string? Metadata { get; private set; }

    // Navigation property
    public CmsVersion Version { get; private set; } = null!;

    private CmsContentBlock() { } // EF Core constructor

    public CmsContentBlock(
        Guid cmsVersionId,
        string blockKey,
        CmsBlockType blockType,
        string content,
        string sectionKey,
        int sortOrder = 0,
        string language = "fr",
        string? metadata = null)
    {
        if (cmsVersionId == Guid.Empty)
            throw new ArgumentException("CMS version ID cannot be empty.", nameof(cmsVersionId));

        CmsVersionId = cmsVersionId;
        BlockKey = blockKey ?? throw new ArgumentNullException(nameof(blockKey));
        BlockType = blockType;
        Content = content ?? throw new ArgumentNullException(nameof(content));
        SectionKey = sectionKey ?? throw new ArgumentNullException(nameof(sectionKey));
        SortOrder = sortOrder;
        Language = language ?? throw new ArgumentNullException(nameof(language));
        Metadata = metadata;
    }

    /// <summary>
    /// Updates the content of this block.
    /// </summary>
    public void UpdateContent(string content)
    {
        Content = content ?? throw new ArgumentNullException(nameof(content));
    }

    /// <summary>
    /// Updates the sort order of this block within its section.
    /// </summary>
    public void UpdateSortOrder(int order)
    {
        SortOrder = order;
    }

    /// <summary>
    /// Updates the metadata JSON for this block.
    /// </summary>
    public void UpdateMetadata(string? metadata)
    {
        Metadata = metadata;
    }
}

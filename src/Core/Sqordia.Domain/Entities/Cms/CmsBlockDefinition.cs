using Sqordia.Domain.Common;
using Sqordia.Domain.Enums;

namespace Sqordia.Domain.Entities.Cms;

/// <summary>
/// Represents a block definition within a CMS section.
/// Block definitions describe what content blocks can exist in a section,
/// along with their default values and validation rules.
/// </summary>
public class CmsBlockDefinition : BaseAuditableEntity
{
    /// <summary>
    /// Reference to the parent section.
    /// </summary>
    public Guid CmsSectionId { get; private set; }

    /// <summary>
    /// Unique key identifier for the block within the section.
    /// Combined with section key for full identification.
    /// </summary>
    public string BlockKey { get; private set; } = string.Empty;

    /// <summary>
    /// Type of content this block can contain.
    /// </summary>
    public CmsBlockType BlockType { get; private set; }

    /// <summary>
    /// Display label for this block in the CMS editor.
    /// </summary>
    public string Label { get; private set; } = string.Empty;

    /// <summary>
    /// Optional description or help text for content editors.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Default content value when creating new content blocks.
    /// </summary>
    public string? DefaultContent { get; private set; }

    /// <summary>
    /// Order in which this block appears within its section.
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// Whether this block is required to have content.
    /// </summary>
    public bool IsRequired { get; private set; }

    /// <summary>
    /// Whether this block definition is currently active.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Optional JSON schema for validating the block's content.
    /// Useful for Json-type blocks.
    /// </summary>
    public string? ValidationRules { get; private set; }

    /// <summary>
    /// Optional JSON schema for the block's metadata.
    /// </summary>
    public string? MetadataSchema { get; private set; }

    /// <summary>
    /// Optional placeholder text for the editor.
    /// </summary>
    public string? Placeholder { get; private set; }

    /// <summary>
    /// Maximum character length for text content (0 = unlimited).
    /// </summary>
    public int MaxLength { get; private set; }

    /// <summary>
    /// Navigation property to the parent section.
    /// </summary>
    public CmsSection Section { get; private set; } = null!;

    private CmsBlockDefinition() { } // EF Core constructor

    public CmsBlockDefinition(
        Guid cmsSectionId,
        string blockKey,
        CmsBlockType blockType,
        string label,
        int sortOrder,
        string? description = null,
        string? defaultContent = null,
        bool isRequired = false,
        string? validationRules = null,
        string? metadataSchema = null,
        string? placeholder = null,
        int maxLength = 0)
    {
        if (cmsSectionId == Guid.Empty)
            throw new ArgumentException("CmsSectionId cannot be empty", nameof(cmsSectionId));
        ArgumentException.ThrowIfNullOrWhiteSpace(blockKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(label);

        CmsSectionId = cmsSectionId;
        BlockKey = blockKey.ToLowerInvariant().Trim();
        BlockType = blockType;
        Label = label.Trim();
        Description = description?.Trim();
        DefaultContent = defaultContent;
        SortOrder = sortOrder;
        IsRequired = isRequired;
        ValidationRules = validationRules;
        MetadataSchema = metadataSchema;
        Placeholder = placeholder?.Trim();
        MaxLength = maxLength >= 0 ? maxLength : 0;
    }

    /// <summary>
    /// Updates the block definition's properties.
    /// </summary>
    public void Update(
        string label,
        string? description,
        string? defaultContent,
        int sortOrder,
        bool isRequired,
        string? validationRules,
        string? metadataSchema,
        string? placeholder,
        int maxLength)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(label);

        Label = label.Trim();
        Description = description?.Trim();
        DefaultContent = defaultContent;
        SortOrder = sortOrder;
        IsRequired = isRequired;
        ValidationRules = validationRules;
        MetadataSchema = metadataSchema;
        Placeholder = placeholder?.Trim();
        MaxLength = maxLength >= 0 ? maxLength : 0;
    }

    /// <summary>
    /// Changes the block type. Use with caution as this may affect existing content.
    /// </summary>
    public void ChangeBlockType(CmsBlockType newType)
    {
        BlockType = newType;
    }

    /// <summary>
    /// Activates the block definition.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Deactivates the block definition.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }
}

using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities.Cms;

/// <summary>
/// Represents a reusable content template that can be applied to CMS sections.
/// Templates store block configurations that can be saved and reused across versions.
/// </summary>
public class CmsContentTemplate : BaseAuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? PageKey { get; private set; }
    public string? SectionKey { get; private set; }
    public string TemplateData { get; private set; } = string.Empty; // JSON data
    public string? PreviewImageUrl { get; private set; }
    public bool IsPublic { get; private set; }
    public Guid CreatedByUserId { get; private set; }

    private CmsContentTemplate() { } // EF Core constructor

    public CmsContentTemplate(
        string name,
        string templateData,
        Guid createdByUserId,
        string? description = null,
        string? pageKey = null,
        string? sectionKey = null,
        string? previewImageUrl = null,
        bool isPublic = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Template name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(templateData))
            throw new ArgumentException("Template data cannot be empty.", nameof(templateData));

        if (createdByUserId == Guid.Empty)
            throw new ArgumentException("Created by user ID cannot be empty.", nameof(createdByUserId));

        Name = name.Trim();
        Description = description?.Trim();
        PageKey = pageKey?.Trim();
        SectionKey = sectionKey?.Trim();
        TemplateData = templateData;
        PreviewImageUrl = previewImageUrl?.Trim();
        IsPublic = isPublic;
        CreatedByUserId = createdByUserId;
    }

    /// <summary>
    /// Updates the template metadata.
    /// </summary>
    public void UpdateMetadata(
        string name,
        string? description = null,
        string? pageKey = null,
        string? sectionKey = null,
        string? previewImageUrl = null,
        bool? isPublic = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Template name cannot be empty.", nameof(name));

        Name = name.Trim();
        Description = description?.Trim();
        PageKey = pageKey?.Trim();
        SectionKey = sectionKey?.Trim();
        PreviewImageUrl = previewImageUrl?.Trim();

        if (isPublic.HasValue)
            IsPublic = isPublic.Value;
    }

    /// <summary>
    /// Updates the template content data.
    /// </summary>
    public void UpdateTemplateData(string templateData)
    {
        if (string.IsNullOrWhiteSpace(templateData))
            throw new ArgumentException("Template data cannot be empty.", nameof(templateData));

        TemplateData = templateData;
    }

    /// <summary>
    /// Makes the template public (visible to all users).
    /// </summary>
    public void MakePublic()
    {
        IsPublic = true;
    }

    /// <summary>
    /// Makes the template private (visible only to the creator).
    /// </summary>
    public void MakePrivate()
    {
        IsPublic = false;
    }

    /// <summary>
    /// Sets the preview image URL for the template.
    /// </summary>
    public void SetPreviewImage(string? previewImageUrl)
    {
        PreviewImageUrl = previewImageUrl?.Trim();
    }
}

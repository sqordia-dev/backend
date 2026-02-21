using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.Cms;
using Sqordia.Contracts.Responses.Cms;

namespace Sqordia.Application.Services.Cms;

/// <summary>
/// Service for managing CMS content templates.
/// </summary>
public interface ICmsTemplateService
{
    /// <summary>
    /// Gets all templates accessible to the current user.
    /// Returns public templates and templates created by the current user.
    /// </summary>
    Task<Result<List<CmsContentTemplateSummaryResponse>>> GetTemplatesAsync(
        string? pageKey = null,
        string? sectionKey = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a template by ID.
    /// </summary>
    Task<Result<CmsContentTemplateResponse>> GetTemplateAsync(
        Guid templateId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new content template.
    /// </summary>
    Task<Result<CmsContentTemplateResponse>> CreateTemplateAsync(
        CreateCmsTemplateRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing template.
    /// Only the creator can update a template.
    /// </summary>
    Task<Result<CmsContentTemplateResponse>> UpdateTemplateAsync(
        Guid templateId,
        UpdateCmsTemplateRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a template.
    /// Only the creator can delete a template.
    /// </summary>
    Task<Result> DeleteTemplateAsync(
        Guid templateId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a template from an existing section's blocks.
    /// </summary>
    Task<Result<CmsContentTemplateResponse>> CreateTemplateFromSectionAsync(
        Guid versionId,
        string sectionKey,
        string language,
        string name,
        string? description = null,
        bool isPublic = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies a template to a version's section, creating or replacing content blocks.
    /// </summary>
    Task<Result<List<CmsContentBlockResponse>>> ApplyTemplateAsync(
        Guid versionId,
        Guid templateId,
        string sectionKey,
        string language,
        bool replaceExisting = false,
        CancellationToken cancellationToken = default);
}

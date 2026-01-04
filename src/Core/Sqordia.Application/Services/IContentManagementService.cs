using Sqordia.Contracts.Requests.Content;
using Sqordia.Contracts.Responses.Content;

namespace Sqordia.Application.Services;

/// <summary>
/// Service for managing content pages (CMS)
/// </summary>
public interface IContentManagementService
{
    /// <summary>
    /// Get a content page by key and language
    /// </summary>
    Task<ContentPageResponse?> GetContentPageAsync(
        string pageKey,
        string language = "fr",
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all content pages
    /// </summary>
    Task<List<ContentPageResponse>> GetAllContentPagesAsync(
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Update or create a content page
    /// </summary>
    Task<ContentPageResponse> UpdateContentPageAsync(
        string pageKey,
        UpdateContentPageRequest request,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Publish a content page
    /// </summary>
    Task<ContentPageResponse> PublishContentPageAsync(
        string pageKey,
        string language,
        CancellationToken cancellationToken = default);
}


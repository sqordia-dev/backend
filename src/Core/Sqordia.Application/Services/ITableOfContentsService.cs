using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.TableOfContents;
using Sqordia.Contracts.Responses.TableOfContents;

namespace Sqordia.Application.Services;

/// <summary>
/// Service for managing business plan table of contents settings
/// </summary>
public interface ITableOfContentsService
{
    /// <summary>
    /// Get table of contents settings for a business plan
    /// </summary>
    Task<Result<TOCSettingsResponse>> GetSettingsAsync(Guid businessPlanId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update or create table of contents settings for a business plan
    /// </summary>
    Task<Result<TOCSettingsResponse>> UpdateSettingsAsync(Guid businessPlanId, UpdateTOCSettingsRequest request, CancellationToken cancellationToken = default);
}

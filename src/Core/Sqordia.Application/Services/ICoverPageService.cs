using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.CoverPage;
using Sqordia.Contracts.Responses.CoverPage;

namespace Sqordia.Application.Services;

/// <summary>
/// Service for managing business plan cover page settings
/// </summary>
public interface ICoverPageService
{
    /// <summary>
    /// Get cover page settings for a business plan
    /// </summary>
    Task<Result<CoverPageResponse>> GetCoverPageAsync(Guid businessPlanId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update or create cover page settings for a business plan
    /// </summary>
    Task<Result<CoverPageResponse>> UpdateCoverPageAsync(Guid businessPlanId, UpdateCoverPageRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Upload a logo for the cover page
    /// </summary>
    Task<Result<string>> UploadLogoAsync(Guid businessPlanId, Stream logoStream, string fileName, string contentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete the logo from the cover page
    /// </summary>
    Task<Result> DeleteLogoAsync(Guid businessPlanId, CancellationToken cancellationToken = default);
}

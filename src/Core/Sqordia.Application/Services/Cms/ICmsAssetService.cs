using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.Cms;
using Sqordia.Contracts.Responses.Cms;

namespace Sqordia.Application.Services.Cms;

public interface ICmsAssetService
{
    Task<Result<CmsAssetResponse>> UploadAssetAsync(UploadCmsAssetRequest request, CancellationToken cancellationToken = default);
    Task<Result<List<CmsAssetResponse>>> GetAssetsAsync(string? category = null, CancellationToken cancellationToken = default);
    Task<Result> DeleteAssetAsync(Guid assetId, CancellationToken cancellationToken = default);
}

using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.Cms;
using Sqordia.Contracts.Responses.Cms;

namespace Sqordia.Application.Services.Cms;

public interface ICmsVersionService
{
    Task<Result<CmsVersionDetailResponse?>> GetActiveVersionAsync(CancellationToken cancellationToken = default);
    Task<Result<CmsVersionDetailResponse>> GetVersionAsync(Guid versionId, CancellationToken cancellationToken = default);
    Task<Result<List<CmsVersionResponse>>> GetAllVersionsAsync(CancellationToken cancellationToken = default);
    Task<Result<CmsVersionDetailResponse>> CreateVersionAsync(CreateCmsVersionRequest request, CancellationToken cancellationToken = default);
    Task<Result<CmsVersionResponse>> UpdateVersionAsync(Guid versionId, UpdateCmsVersionRequest request, CancellationToken cancellationToken = default);
    Task<Result<CmsVersionResponse>> PublishVersionAsync(Guid versionId, CancellationToken cancellationToken = default);
    Task<Result> DeleteVersionAsync(Guid versionId, CancellationToken cancellationToken = default);
}

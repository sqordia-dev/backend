using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.Cms;
using Sqordia.Contracts.Responses.Cms;

namespace Sqordia.Application.Services.Cms;

public interface ICmsContentBlockService
{
    Task<Result<List<CmsContentBlockResponse>>> GetBlocksByVersionAsync(Guid versionId, string? sectionKey = null, string? language = null, CancellationToken cancellationToken = default);
    Task<Result<CmsContentBlockResponse>> GetBlockAsync(Guid blockId, CancellationToken cancellationToken = default);
    Task<Result<CmsContentBlockResponse>> CreateBlockAsync(Guid versionId, CreateContentBlockRequest request, CancellationToken cancellationToken = default);
    Task<Result<CmsContentBlockResponse>> UpdateBlockAsync(Guid blockId, UpdateContentBlockRequest request, CancellationToken cancellationToken = default);
    Task<Result<List<CmsContentBlockResponse>>> BulkUpdateBlocksAsync(Guid versionId, BulkUpdateContentBlocksRequest request, CancellationToken cancellationToken = default);
    Task<Result> ReorderBlocksAsync(Guid versionId, ReorderContentBlocksRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteBlockAsync(Guid blockId, CancellationToken cancellationToken = default);
    Task<Result<List<CmsContentBlockResponse>>> CloneBlocksFromPublishedAsync(Guid targetVersionId, CancellationToken cancellationToken = default);
}

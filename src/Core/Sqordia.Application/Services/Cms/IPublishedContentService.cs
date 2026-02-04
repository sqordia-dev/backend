using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Responses.Cms;

namespace Sqordia.Application.Services.Cms;

public interface IPublishedContentService
{
    Task<Result<PublishedContentResponse>> GetPublishedContentAsync(string? sectionKey = null, string language = "fr", CancellationToken cancellationToken = default);
    Task<Result<PublishedContentResponse>> GetPublishedContentByPageAsync(string pageKey, string language = "fr", CancellationToken cancellationToken = default);
    Task<Result<CmsContentBlockResponse>> GetPublishedBlockAsync(string blockKey, string language = "fr", CancellationToken cancellationToken = default);
}

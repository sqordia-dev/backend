using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Responses.Cms;

namespace Sqordia.Application.Services.Cms;

/// <summary>
/// Service for comparing CMS versions and generating diffs
/// </summary>
public interface ICmsDiffService
{
    /// <summary>
    /// Compare two versions and return the diff
    /// </summary>
    /// <param name="sourceVersionId">The source (newer/draft) version to compare</param>
    /// <param name="targetVersionId">The target (older/published) version to compare against</param>
    /// <param name="language">Optional language filter (en, fr). If null, compares all languages.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<Result<CmsDiffResponse>> CompareVersionsAsync(
        Guid sourceVersionId,
        Guid targetVersionId,
        string? language = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a diff between the current draft and the published version
    /// </summary>
    Task<Result<CmsDiffResponse>> GetDraftVsPublishedDiffAsync(
        string? language = null,
        CancellationToken cancellationToken = default);
}

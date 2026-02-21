using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Responses.Cms;

namespace Sqordia.Application.Services.Cms;

/// <summary>
/// Service for managing CMS version approval workflow and scheduling.
/// </summary>
public interface ICmsApprovalService
{
    /// <summary>
    /// Submits a version for approval.
    /// </summary>
    Task<Result> SubmitForApprovalAsync(Guid versionId, string? notes = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Approves a version that is pending approval.
    /// </summary>
    Task<Result> ApproveAsync(Guid versionId, string? notes = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rejects a version that is pending approval.
    /// </summary>
    Task<Result> RejectAsync(Guid versionId, string? reason = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Schedules a version for future publishing.
    /// </summary>
    Task<Result> ScheduleAsync(Guid versionId, DateTime publishAt, string? notes = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels the scheduled publishing for a version.
    /// </summary>
    Task<Result> CancelScheduleAsync(Guid versionId, string? notes = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the version history for a specific version.
    /// </summary>
    Task<Result<List<CmsVersionHistoryResponse>>> GetHistoryAsync(Guid versionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes all versions that are due for scheduled publishing.
    /// Called by a background job/scheduler.
    /// </summary>
    Task<Result<int>> ProcessScheduledPublishingAsync(CancellationToken cancellationToken = default);
}

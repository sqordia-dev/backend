using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Services.Cms;
using Sqordia.Contracts.Responses.Cms;
using Sqordia.Domain.Entities.Cms;
using Sqordia.Domain.Enums;

namespace Sqordia.Application.Services.Implementations.Cms;

/// <summary>
/// Service for managing CMS version approval workflow and scheduling.
/// </summary>
public class CmsApprovalService : ICmsApprovalService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CmsApprovalService> _logger;

    private static readonly Error VersionNotFoundError = new("CmsApproval.VersionNotFound", "Version not found");
    private static readonly Error UnauthorizedError = new("CmsApproval.Unauthorized", "User not authenticated");

    public CmsApprovalService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<CmsApprovalService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result> SubmitForApprovalAsync(Guid versionId, string? notes = null, CancellationToken cancellationToken = default)
    {
        var userIdStr = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return Result.Failure(UnauthorizedError);

        var version = await _context.CmsVersions
            .FirstOrDefaultAsync(v => v.Id == versionId, cancellationToken);

        if (version == null)
            return Result.Failure(VersionNotFoundError);

        try
        {
            version.SubmitForApproval(userId);

            var history = CmsVersionHistory.ForSubmitForApproval(versionId, userId, notes);
            _context.CmsVersionHistory.Add(history);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Version {VersionId} submitted for approval by user {UserId}", versionId, userId);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(new Error("CmsApproval.InvalidOperation", ex.Message));
        }
    }

    public async Task<Result> ApproveAsync(Guid versionId, string? notes = null, CancellationToken cancellationToken = default)
    {
        var userIdStr = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return Result.Failure(UnauthorizedError);

        var version = await _context.CmsVersions
            .FirstOrDefaultAsync(v => v.Id == versionId, cancellationToken);

        if (version == null)
            return Result.Failure(VersionNotFoundError);

        try
        {
            version.Approve(userId);

            var history = CmsVersionHistory.ForApproval(versionId, userId, notes);
            _context.CmsVersionHistory.Add(history);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Version {VersionId} approved by user {UserId}", versionId, userId);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(new Error("CmsApproval.InvalidOperation", ex.Message));
        }
    }

    public async Task<Result> RejectAsync(Guid versionId, string? reason = null, CancellationToken cancellationToken = default)
    {
        var userIdStr = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return Result.Failure(UnauthorizedError);

        var version = await _context.CmsVersions
            .FirstOrDefaultAsync(v => v.Id == versionId, cancellationToken);

        if (version == null)
            return Result.Failure(VersionNotFoundError);

        try
        {
            version.Reject(userId, reason);

            var history = CmsVersionHistory.ForRejection(versionId, userId, reason);
            _context.CmsVersionHistory.Add(history);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Version {VersionId} rejected by user {UserId}. Reason: {Reason}", versionId, userId, reason);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(new Error("CmsApproval.InvalidOperation", ex.Message));
        }
    }

    public async Task<Result> ScheduleAsync(Guid versionId, DateTime publishAt, string? notes = null, CancellationToken cancellationToken = default)
    {
        var userIdStr = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return Result.Failure(UnauthorizedError);

        var version = await _context.CmsVersions
            .FirstOrDefaultAsync(v => v.Id == versionId, cancellationToken);

        if (version == null)
            return Result.Failure(VersionNotFoundError);

        try
        {
            version.Schedule(publishAt);

            var history = CmsVersionHistory.ForSchedule(versionId, userId, publishAt, notes);
            _context.CmsVersionHistory.Add(history);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Version {VersionId} scheduled for publishing at {PublishAt} by user {UserId}", versionId, publishAt, userId);
            return Result.Success();
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(new Error("CmsApproval.InvalidSchedule", ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(new Error("CmsApproval.InvalidOperation", ex.Message));
        }
    }

    public async Task<Result> CancelScheduleAsync(Guid versionId, string? notes = null, CancellationToken cancellationToken = default)
    {
        var userIdStr = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return Result.Failure(UnauthorizedError);

        var version = await _context.CmsVersions
            .FirstOrDefaultAsync(v => v.Id == versionId, cancellationToken);

        if (version == null)
            return Result.Failure(VersionNotFoundError);

        try
        {
            version.CancelSchedule();

            var history = CmsVersionHistory.ForScheduleCancellation(versionId, userId, notes);
            _context.CmsVersionHistory.Add(history);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Scheduled publishing cancelled for version {VersionId} by user {UserId}", versionId, userId);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(new Error("CmsApproval.InvalidOperation", ex.Message));
        }
    }

    public async Task<Result<List<CmsVersionHistoryResponse>>> GetHistoryAsync(Guid versionId, CancellationToken cancellationToken = default)
    {
        var version = await _context.CmsVersions
            .AsNoTracking()
            .AnyAsync(v => v.Id == versionId, cancellationToken);

        if (!version)
            return Result.Failure<List<CmsVersionHistoryResponse>>(VersionNotFoundError);

        var history = await _context.CmsVersionHistory
            .AsNoTracking()
            .Where(h => h.CmsVersionId == versionId)
            .OrderByDescending(h => h.PerformedAt)
            .Select(h => new CmsVersionHistoryResponse
            {
                Id = h.Id,
                CmsVersionId = h.CmsVersionId,
                Action = h.Action.ToString(),
                PerformedByUserId = h.PerformedByUserId,
                PerformedAt = h.PerformedAt,
                Notes = h.Notes,
                OldStatus = h.OldStatus.HasValue ? h.OldStatus.Value.ToString() : null,
                NewStatus = h.NewStatus.HasValue ? h.NewStatus.Value.ToString() : null,
                OldApprovalStatus = h.OldApprovalStatus.HasValue ? h.OldApprovalStatus.Value.ToString() : null,
                NewApprovalStatus = h.NewApprovalStatus.HasValue ? h.NewApprovalStatus.Value.ToString() : null,
                ChangeSummary = h.ChangeSummary,
                ScheduledPublishAt = h.ScheduledPublishAt
            })
            .ToListAsync(cancellationToken);

        return Result.Success(history);
    }

    public async Task<Result<int>> ProcessScheduledPublishingAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var scheduledVersions = await _context.CmsVersions
            .Where(v => v.Status == CmsVersionStatus.Draft
                        && v.ScheduledPublishAt.HasValue
                        && v.ScheduledPublishAt.Value <= now)
            .ToListAsync(cancellationToken);

        if (scheduledVersions.Count == 0)
            return Result.Success(0);

        var publishedCount = 0;

        foreach (var version in scheduledVersions)
        {
            try
            {
                // Archive any currently published version
                var currentlyPublished = await _context.CmsVersions
                    .Where(v => v.Status == CmsVersionStatus.Published)
                    .ToListAsync(cancellationToken);

                foreach (var published in currentlyPublished)
                {
                    published.Archive();
                }

                // Publish the scheduled version
                // Use a system user ID for scheduled publishing
                var systemUserId = version.CreatedByUserId; // Fall back to creator
                version.Publish(systemUserId);

                var history = CmsVersionHistory.ForPublish(version.Id, systemUserId, "Scheduled publishing");
                _context.CmsVersionHistory.Add(history);

                publishedCount++;

                _logger.LogInformation("Scheduled version {VersionId} published successfully", version.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish scheduled version {VersionId}", version.Id);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(publishedCount);
    }
}

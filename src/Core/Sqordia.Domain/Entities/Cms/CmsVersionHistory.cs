using Sqordia.Domain.Common;
using Sqordia.Domain.Enums;

namespace Sqordia.Domain.Entities.Cms;

/// <summary>
/// Represents a history entry for a CMS version, tracking all actions performed on the version.
/// Provides a complete audit trail of the version lifecycle.
/// </summary>
public class CmsVersionHistory : BaseEntity
{
    public Guid CmsVersionId { get; private set; }
    public CmsVersionAction Action { get; private set; }
    public Guid PerformedByUserId { get; private set; }
    public DateTime PerformedAt { get; private set; }
    public string? Notes { get; private set; }

    // State tracking
    public CmsVersionStatus? OldStatus { get; private set; }
    public CmsVersionStatus? NewStatus { get; private set; }
    public CmsApprovalStatus? OldApprovalStatus { get; private set; }
    public CmsApprovalStatus? NewApprovalStatus { get; private set; }

    // Additional context
    public string? ChangeSummary { get; private set; }
    public DateTime? ScheduledPublishAt { get; private set; }

    // Navigation property
    public CmsVersion Version { get; private set; } = null!;

    private CmsVersionHistory() { } // EF Core constructor

    public CmsVersionHistory(
        Guid cmsVersionId,
        CmsVersionAction action,
        Guid performedByUserId,
        string? notes = null,
        CmsVersionStatus? oldStatus = null,
        CmsVersionStatus? newStatus = null,
        CmsApprovalStatus? oldApprovalStatus = null,
        CmsApprovalStatus? newApprovalStatus = null,
        string? changeSummary = null,
        DateTime? scheduledPublishAt = null)
    {
        if (cmsVersionId == Guid.Empty)
            throw new ArgumentException("CMS version ID cannot be empty.", nameof(cmsVersionId));

        if (performedByUserId == Guid.Empty)
            throw new ArgumentException("Performed by user ID cannot be empty.", nameof(performedByUserId));

        CmsVersionId = cmsVersionId;
        Action = action;
        PerformedByUserId = performedByUserId;
        PerformedAt = DateTime.UtcNow;
        Notes = notes;
        OldStatus = oldStatus;
        NewStatus = newStatus;
        OldApprovalStatus = oldApprovalStatus;
        NewApprovalStatus = newApprovalStatus;
        ChangeSummary = changeSummary;
        ScheduledPublishAt = scheduledPublishAt;
    }

    /// <summary>
    /// Creates a history entry for version creation
    /// </summary>
    public static CmsVersionHistory ForCreation(Guid cmsVersionId, Guid userId, string? notes = null)
    {
        return new CmsVersionHistory(
            cmsVersionId,
            CmsVersionAction.Created,
            userId,
            notes,
            newStatus: CmsVersionStatus.Draft);
    }

    /// <summary>
    /// Creates a history entry for content modification
    /// </summary>
    public static CmsVersionHistory ForModification(Guid cmsVersionId, Guid userId, string? changeSummary = null)
    {
        return new CmsVersionHistory(
            cmsVersionId,
            CmsVersionAction.Modified,
            userId,
            changeSummary: changeSummary);
    }

    /// <summary>
    /// Creates a history entry for submission for approval
    /// </summary>
    public static CmsVersionHistory ForSubmitForApproval(Guid cmsVersionId, Guid userId, string? notes = null)
    {
        return new CmsVersionHistory(
            cmsVersionId,
            CmsVersionAction.SubmittedForApproval,
            userId,
            notes,
            oldApprovalStatus: CmsApprovalStatus.None,
            newApprovalStatus: CmsApprovalStatus.Pending);
    }

    /// <summary>
    /// Creates a history entry for approval
    /// </summary>
    public static CmsVersionHistory ForApproval(Guid cmsVersionId, Guid userId, string? notes = null)
    {
        return new CmsVersionHistory(
            cmsVersionId,
            CmsVersionAction.Approved,
            userId,
            notes,
            oldApprovalStatus: CmsApprovalStatus.Pending,
            newApprovalStatus: CmsApprovalStatus.Approved);
    }

    /// <summary>
    /// Creates a history entry for rejection
    /// </summary>
    public static CmsVersionHistory ForRejection(Guid cmsVersionId, Guid userId, string? reason = null)
    {
        return new CmsVersionHistory(
            cmsVersionId,
            CmsVersionAction.Rejected,
            userId,
            reason,
            oldApprovalStatus: CmsApprovalStatus.Pending,
            newApprovalStatus: CmsApprovalStatus.Rejected);
    }

    /// <summary>
    /// Creates a history entry for scheduling
    /// </summary>
    public static CmsVersionHistory ForSchedule(Guid cmsVersionId, Guid userId, DateTime scheduledAt, string? notes = null)
    {
        return new CmsVersionHistory(
            cmsVersionId,
            CmsVersionAction.Scheduled,
            userId,
            notes,
            scheduledPublishAt: scheduledAt);
    }

    /// <summary>
    /// Creates a history entry for schedule cancellation
    /// </summary>
    public static CmsVersionHistory ForScheduleCancellation(Guid cmsVersionId, Guid userId, string? notes = null)
    {
        return new CmsVersionHistory(
            cmsVersionId,
            CmsVersionAction.ScheduleCancelled,
            userId,
            notes);
    }

    /// <summary>
    /// Creates a history entry for publishing
    /// </summary>
    public static CmsVersionHistory ForPublish(Guid cmsVersionId, Guid userId, string? notes = null)
    {
        return new CmsVersionHistory(
            cmsVersionId,
            CmsVersionAction.Published,
            userId,
            notes,
            oldStatus: CmsVersionStatus.Draft,
            newStatus: CmsVersionStatus.Published);
    }

    /// <summary>
    /// Creates a history entry for archiving
    /// </summary>
    public static CmsVersionHistory ForArchive(Guid cmsVersionId, Guid userId, CmsVersionStatus oldStatus, string? notes = null)
    {
        return new CmsVersionHistory(
            cmsVersionId,
            CmsVersionAction.Archived,
            userId,
            notes,
            oldStatus: oldStatus,
            newStatus: CmsVersionStatus.Archived);
    }
}

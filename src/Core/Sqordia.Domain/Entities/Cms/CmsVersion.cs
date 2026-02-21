using Sqordia.Domain.Common;
using Sqordia.Domain.Enums;

namespace Sqordia.Domain.Entities.Cms;

/// <summary>
/// Represents a versioned snapshot of CMS content.
/// Each version contains a collection of content blocks and goes through a Draft -> Published -> Archived lifecycle.
/// Supports optional approval workflow and scheduled publishing.
/// </summary>
public class CmsVersion : BaseAuditableEntity
{
    public int VersionNumber { get; private set; }
    public CmsVersionStatus Status { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public Guid? PublishedByUserId { get; private set; }
    public string? Notes { get; private set; }

    // Scheduling
    public DateTime? ScheduledPublishAt { get; private set; }

    // Approval workflow
    public CmsApprovalStatus ApprovalStatus { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public Guid? ApprovedByUserId { get; private set; }
    public DateTime? RejectedAt { get; private set; }
    public Guid? RejectedByUserId { get; private set; }
    public string? RejectionReason { get; private set; }

    private readonly List<CmsContentBlock> _contentBlocks = new();
    public ICollection<CmsContentBlock> ContentBlocks => _contentBlocks;

    private readonly List<CmsVersionHistory> _history = new();
    public ICollection<CmsVersionHistory> History => _history;

    private CmsVersion() { } // EF Core constructor

    public CmsVersion(
        int versionNumber,
        Guid createdByUserId,
        string? notes = null)
    {
        if (versionNumber <= 0)
            throw new ArgumentException("Version number must be greater than zero.", nameof(versionNumber));

        if (createdByUserId == Guid.Empty)
            throw new ArgumentException("Created by user ID cannot be empty.", nameof(createdByUserId));

        VersionNumber = versionNumber;
        CreatedByUserId = createdByUserId;
        Status = CmsVersionStatus.Draft;
        Notes = notes;
    }

    /// <summary>
    /// Publishes this version, making it the live content.
    /// Only draft versions can be published.
    /// </summary>
    public void Publish(Guid userId)
    {
        if (Status != CmsVersionStatus.Draft)
            throw new InvalidOperationException($"Cannot publish a version with status '{Status}'. Only draft versions can be published.");

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        Status = CmsVersionStatus.Published;
        PublishedAt = DateTime.UtcNow;
        PublishedByUserId = userId;
    }

    /// <summary>
    /// Archives this version. Published or draft versions can be archived.
    /// </summary>
    public void Archive()
    {
        if (Status == CmsVersionStatus.Archived)
            throw new InvalidOperationException("Version is already archived.");

        Status = CmsVersionStatus.Archived;
    }

    /// <summary>
    /// Adds a content block to this version. Only draft versions can be modified.
    /// </summary>
    public void AddContentBlock(CmsContentBlock block)
    {
        if (Status != CmsVersionStatus.Draft)
            throw new InvalidOperationException($"Cannot add content blocks to a version with status '{Status}'. Only draft versions can be modified.");

        if (block == null)
            throw new ArgumentNullException(nameof(block));

        _contentBlocks.Add(block);
    }

    /// <summary>
    /// Removes a content block from this version by its ID. Only draft versions can be modified.
    /// </summary>
    public void RemoveContentBlock(Guid blockId)
    {
        if (Status != CmsVersionStatus.Draft)
            throw new InvalidOperationException($"Cannot remove content blocks from a version with status '{Status}'. Only draft versions can be modified.");

        var block = _contentBlocks.FirstOrDefault(b => b.Id == blockId);
        if (block == null)
            throw new InvalidOperationException($"Content block with ID '{blockId}' was not found in this version.");

        _contentBlocks.Remove(block);
    }

    /// <summary>
    /// Submits this version for approval. Only draft versions can be submitted.
    /// </summary>
    public void SubmitForApproval(Guid userId)
    {
        if (Status != CmsVersionStatus.Draft)
            throw new InvalidOperationException($"Cannot submit a version with status '{Status}' for approval. Only draft versions can be submitted.");

        if (ApprovalStatus == CmsApprovalStatus.Pending)
            throw new InvalidOperationException("Version is already pending approval.");

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        ApprovalStatus = CmsApprovalStatus.Pending;
        // Clear any previous rejection
        RejectedAt = null;
        RejectedByUserId = null;
        RejectionReason = null;
    }

    /// <summary>
    /// Approves this version. Only versions pending approval can be approved.
    /// </summary>
    public void Approve(Guid userId)
    {
        if (ApprovalStatus != CmsApprovalStatus.Pending)
            throw new InvalidOperationException($"Cannot approve a version with approval status '{ApprovalStatus}'. Only pending versions can be approved.");

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        ApprovalStatus = CmsApprovalStatus.Approved;
        ApprovedAt = DateTime.UtcNow;
        ApprovedByUserId = userId;
    }

    /// <summary>
    /// Rejects this version. Only versions pending approval can be rejected.
    /// </summary>
    public void Reject(Guid userId, string? reason = null)
    {
        if (ApprovalStatus != CmsApprovalStatus.Pending)
            throw new InvalidOperationException($"Cannot reject a version with approval status '{ApprovalStatus}'. Only pending versions can be rejected.");

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        ApprovalStatus = CmsApprovalStatus.Rejected;
        RejectedAt = DateTime.UtcNow;
        RejectedByUserId = userId;
        RejectionReason = reason;
    }

    /// <summary>
    /// Schedules this version for future publishing.
    /// Version must be a draft and optionally approved (if approval workflow is enabled).
    /// </summary>
    public void Schedule(DateTime publishAt)
    {
        if (Status != CmsVersionStatus.Draft)
            throw new InvalidOperationException($"Cannot schedule a version with status '{Status}'. Only draft versions can be scheduled.");

        if (publishAt <= DateTime.UtcNow)
            throw new ArgumentException("Scheduled publish time must be in the future.", nameof(publishAt));

        ScheduledPublishAt = publishAt;
    }

    /// <summary>
    /// Cancels the scheduled publishing for this version.
    /// </summary>
    public void CancelSchedule()
    {
        if (ScheduledPublishAt == null)
            throw new InvalidOperationException("Version is not scheduled for publishing.");

        ScheduledPublishAt = null;
    }

    /// <summary>
    /// Checks if this version is ready to be published based on its schedule.
    /// </summary>
    public bool IsReadyForScheduledPublish()
    {
        return Status == CmsVersionStatus.Draft
               && ScheduledPublishAt.HasValue
               && ScheduledPublishAt.Value <= DateTime.UtcNow;
    }
}

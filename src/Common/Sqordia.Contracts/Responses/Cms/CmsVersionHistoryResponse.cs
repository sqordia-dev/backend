namespace Sqordia.Contracts.Responses.Cms;

/// <summary>
/// Response representing a CMS version history entry.
/// </summary>
public class CmsVersionHistoryResponse
{
    public required Guid Id { get; set; }
    public required Guid CmsVersionId { get; set; }
    public required string Action { get; set; }
    public required Guid PerformedByUserId { get; set; }
    public string? PerformedByUserName { get; set; }
    public required DateTime PerformedAt { get; set; }
    public string? Notes { get; set; }
    public string? OldStatus { get; set; }
    public string? NewStatus { get; set; }
    public string? OldApprovalStatus { get; set; }
    public string? NewApprovalStatus { get; set; }
    public string? ChangeSummary { get; set; }
    public DateTime? ScheduledPublishAt { get; set; }
}

/// <summary>
/// Request for submitting a version for approval.
/// </summary>
public class SubmitForApprovalRequest
{
    public string? Notes { get; set; }
}

/// <summary>
/// Request for approving a version.
/// </summary>
public class ApproveVersionRequest
{
    public string? Notes { get; set; }
}

/// <summary>
/// Request for rejecting a version.
/// </summary>
public class RejectVersionRequest
{
    public string? Reason { get; set; }
}

/// <summary>
/// Request for scheduling a version for publishing.
/// </summary>
public class ScheduleVersionRequest
{
    public required DateTime PublishAt { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Request for canceling a scheduled version.
/// </summary>
public class CancelScheduleRequest
{
    public string? Notes { get; set; }
}

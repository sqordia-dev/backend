namespace Sqordia.Domain.Enums;

/// <summary>
/// Represents the approval status of a CMS content version
/// </summary>
public enum CmsApprovalStatus
{
    /// <summary>
    /// No approval required or not submitted for approval
    /// </summary>
    None = 0,

    /// <summary>
    /// Version has been submitted and is pending approval
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Version has been approved and can be published
    /// </summary>
    Approved = 2,

    /// <summary>
    /// Version has been rejected and needs revision
    /// </summary>
    Rejected = 3
}

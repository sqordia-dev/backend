namespace Sqordia.Domain.Enums;

/// <summary>
/// Represents an action performed on a CMS version
/// </summary>
public enum CmsVersionAction
{
    /// <summary>
    /// Version was created
    /// </summary>
    Created = 0,

    /// <summary>
    /// Version content was modified
    /// </summary>
    Modified = 1,

    /// <summary>
    /// Version was submitted for approval
    /// </summary>
    SubmittedForApproval = 2,

    /// <summary>
    /// Version was approved
    /// </summary>
    Approved = 3,

    /// <summary>
    /// Version was rejected
    /// </summary>
    Rejected = 4,

    /// <summary>
    /// Version was scheduled for publishing
    /// </summary>
    Scheduled = 5,

    /// <summary>
    /// Scheduled publishing was cancelled
    /// </summary>
    ScheduleCancelled = 6,

    /// <summary>
    /// Version was published
    /// </summary>
    Published = 7,

    /// <summary>
    /// Version was archived
    /// </summary>
    Archived = 8
}

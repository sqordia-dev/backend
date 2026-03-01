namespace Sqordia.Contracts.Requests.GitHub;

/// <summary>
/// Request to update a GitHub issue (partial update - only provided fields are updated)
/// </summary>
public class UpdateGitHubIssueRequest
{
    /// <summary>
    /// The new state for the issue: "open" or "closed"
    /// </summary>
    public string? State { get; set; }

    /// <summary>
    /// The new title for the issue
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// The new body/description for the issue
    /// </summary>
    public string? Body { get; set; }

    /// <summary>
    /// The new priority: "low", "medium", "high", or "critical"
    /// </summary>
    public string? Priority { get; set; }

    /// <summary>
    /// The new category: "bug", "feature", "enhancement", "documentation", "performance"
    /// </summary>
    public string? Category { get; set; }
}

/// <summary>
/// Request to archive (soft delete) a GitHub issue
/// </summary>
public class ArchiveGitHubIssueRequest
{
    /// <summary>
    /// Optional reason for archiving
    /// </summary>
    public string? Reason { get; set; }
}

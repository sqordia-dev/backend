namespace Sqordia.Contracts.Requests.GitHub;

public class ListGitHubIssuesRequest
{
    /// <summary>
    /// Filter by repository: "frontend", "backend", or "all"
    /// </summary>
    public string Repository { get; set; } = "all";

    /// <summary>
    /// Filter by state: "open", "closed", or "all"
    /// </summary>
    public string State { get; set; } = "all";

    /// <summary>
    /// Filter by label (e.g., "priority: critical", "bug")
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// Search query for title/body
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Items per page (max 100)
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Sort by: "created", "updated", "comments"
    /// </summary>
    public string Sort { get; set; } = "created";

    /// <summary>
    /// Sort direction: "asc" or "desc"
    /// </summary>
    public string Direction { get; set; } = "desc";
}

namespace Sqordia.Contracts.Responses.GitHub;

public class GitHubIssueResponse
{
    public int IssueNumber { get; set; }
    public string IssueUrl { get; set; } = string.Empty;
    public string HtmlUrl { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Repository { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

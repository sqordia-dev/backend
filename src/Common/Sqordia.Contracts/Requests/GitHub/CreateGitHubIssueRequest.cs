namespace Sqordia.Contracts.Requests.GitHub;

public class CreateGitHubIssueRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Repository { get; set; } = string.Empty;
    public string? ReproductionSteps { get; set; }
    public string? Browser { get; set; }
    public string? OperatingSystem { get; set; }
    public string? ScreenSize { get; set; }
    public string? CurrentPageUrl { get; set; }
    public List<string>? ScreenshotUrls { get; set; }
}

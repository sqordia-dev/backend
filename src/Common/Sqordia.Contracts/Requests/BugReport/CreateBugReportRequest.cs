namespace Sqordia.Contracts.Requests.BugReport;

public class CreateBugReportRequest
{
    public string Title { get; set; } = string.Empty;
    public string PageSection { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty; // Low, Medium, High, Critical
    public string Description { get; set; } = string.Empty;

    // System Information
    public string? AppVersion { get; set; }
    public string? Browser { get; set; }
    public string? OperatingSystem { get; set; }
}

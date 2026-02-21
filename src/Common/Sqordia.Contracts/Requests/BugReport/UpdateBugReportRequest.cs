namespace Sqordia.Contracts.Requests.BugReport;

public class UpdateBugReportRequest
{
    public string? Title { get; set; }
    public string? PageSection { get; set; }
    public string? Severity { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
    public string? ResolutionNotes { get; set; }
}

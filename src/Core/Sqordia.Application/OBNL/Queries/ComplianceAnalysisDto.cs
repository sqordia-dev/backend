namespace Sqordia.Application.OBNL.Queries;

public record ComplianceAnalysisDto
{
    public string Status { get; init; } = string.Empty;
    public string Level { get; init; } = string.Empty;
    public List<string> Requirements { get; init; } = new();
    public List<string> Recommendations { get; init; } = new();
    public DateTime LastUpdated { get; init; }
    public string Notes { get; init; } = string.Empty;
}

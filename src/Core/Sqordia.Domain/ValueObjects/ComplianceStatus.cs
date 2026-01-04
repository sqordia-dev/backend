namespace Sqordia.Domain.ValueObjects;

public record ComplianceStatus
{
    public string Status { get; init; } = string.Empty;
    public string Level { get; init; } = string.Empty;
    public DateTime LastUpdated { get; init; }
    public string Notes { get; init; } = string.Empty;

    public ComplianceStatus(string status, string level, DateTime lastUpdated, string notes = "")
    {
        Status = status;
        Level = level;
        LastUpdated = lastUpdated;
        Notes = notes;
    }

    public static ComplianceStatus Create(string status, string level, string notes = "")
    {
        return new ComplianceStatus(status, level, DateTime.UtcNow, notes);
    }

    public static ComplianceStatus Pending => new("Pending", "Basic", DateTime.MinValue, "Compliance review pending");
    public static ComplianceStatus InProgress => new("In Progress", "Intermediate", DateTime.MinValue, "Compliance review in progress");
    public static ComplianceStatus Compliant => new("Compliant", "Full", DateTime.MinValue, "All requirements met");
    public static ComplianceStatus NonCompliant => new("Non-Compliant", "None", DateTime.MinValue, "Requirements not met");
}

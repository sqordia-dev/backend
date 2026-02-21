using Sqordia.Domain.Common;
using Sqordia.Domain.Enums;

namespace Sqordia.Domain.Entities;

public class BugReport : BaseAuditableEntity
{
    public string Title { get; private set; } = null!;
    public string PageSection { get; private set; } = null!;
    public BugReportSeverity Severity { get; private set; }
    public string Description { get; private set; } = null!;
    public BugReportStatus Status { get; private set; }

    // System Information
    public string? AppVersion { get; private set; }
    public string? Browser { get; private set; }
    public string? OperatingSystem { get; private set; }

    // User who reported the bug
    public Guid? ReportedByUserId { get; private set; }

    // Ticket number for display (e.g., BUG-9041)
    public string TicketNumber { get; private set; } = null!;

    // Resolution details
    public string? ResolutionNotes { get; private set; }
    public DateTime? ResolvedAt { get; private set; }
    public Guid? ResolvedByUserId { get; private set; }

    // Navigation properties
    public ICollection<BugReportAttachment> Attachments { get; private set; } = new List<BugReportAttachment>();

    private BugReport() { } // EF Core constructor

    public BugReport(
        string title,
        string pageSection,
        BugReportSeverity severity,
        string description,
        string ticketNumber,
        Guid? reportedByUserId = null,
        string? appVersion = null,
        string? browser = null,
        string? operatingSystem = null)
    {
        Title = title ?? throw new ArgumentNullException(nameof(title));
        PageSection = pageSection ?? throw new ArgumentNullException(nameof(pageSection));
        Severity = severity;
        Description = description ?? throw new ArgumentNullException(nameof(description));
        TicketNumber = ticketNumber ?? throw new ArgumentNullException(nameof(ticketNumber));
        Status = BugReportStatus.Open;
        ReportedByUserId = reportedByUserId;
        AppVersion = appVersion;
        Browser = browser;
        OperatingSystem = operatingSystem;
        Created = DateTime.UtcNow;
    }

    public void Update(string title, string pageSection, BugReportSeverity severity, string description)
    {
        Title = title ?? throw new ArgumentNullException(nameof(title));
        PageSection = pageSection ?? throw new ArgumentNullException(nameof(pageSection));
        Severity = severity;
        Description = description ?? throw new ArgumentNullException(nameof(description));
    }

    public void UpdateSystemInfo(string? appVersion, string? browser, string? operatingSystem)
    {
        AppVersion = appVersion;
        Browser = browser;
        OperatingSystem = operatingSystem;
    }

    public void SetInProgress()
    {
        Status = BugReportStatus.InProgress;
    }

    public void Resolve(string resolutionNotes, Guid resolvedByUserId)
    {
        Status = BugReportStatus.Resolved;
        ResolutionNotes = resolutionNotes;
        ResolvedAt = DateTime.UtcNow;
        ResolvedByUserId = resolvedByUserId;
    }

    public void Close()
    {
        Status = BugReportStatus.Closed;
    }

    public void MarkAsWontFix(string resolutionNotes, Guid resolvedByUserId)
    {
        Status = BugReportStatus.WontFix;
        ResolutionNotes = resolutionNotes;
        ResolvedAt = DateTime.UtcNow;
        ResolvedByUserId = resolvedByUserId;
    }

    public void Reopen()
    {
        Status = BugReportStatus.Open;
        ResolutionNotes = null;
        ResolvedAt = null;
        ResolvedByUserId = null;
    }

    public void AddAttachment(BugReportAttachment attachment)
    {
        Attachments.Add(attachment);
    }
}

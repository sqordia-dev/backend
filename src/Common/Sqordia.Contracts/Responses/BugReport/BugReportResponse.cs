namespace Sqordia.Contracts.Responses.BugReport;

public class BugReportResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string PageSection { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string TicketNumber { get; set; } = string.Empty;

    // System Information
    public string? AppVersion { get; set; }
    public string? Browser { get; set; }
    public string? OperatingSystem { get; set; }

    // Reporter information
    public Guid? ReportedByUserId { get; set; }
    public string? ReportedByUserName { get; set; }

    // Resolution details
    public string? ResolutionNotes { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public Guid? ResolvedByUserId { get; set; }
    public string? ResolvedByUserName { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }

    // Attachments
    public List<BugReportAttachmentResponse> Attachments { get; set; } = new();
}

public class BugReportAttachmentResponse
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string StorageUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class BugReportSummaryResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string PageSection { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string TicketNumber { get; set; } = string.Empty;
    public string? ReportedByUserName { get; set; }
    public DateTime CreatedAt { get; set; }
    public int AttachmentCount { get; set; }
}

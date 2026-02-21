using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities;

public class BugReportAttachment : BaseAuditableEntity
{
    public Guid BugReportId { get; private set; }
    public string FileName { get; private set; } = null!;
    public string ContentType { get; private set; } = null!;
    public long FileSizeBytes { get; private set; }
    public string StorageUrl { get; private set; } = null!;

    // Navigation property
    public BugReport BugReport { get; private set; } = null!;

    private BugReportAttachment() { } // EF Core constructor

    public BugReportAttachment(
        Guid bugReportId,
        string fileName,
        string contentType,
        long fileSizeBytes,
        string storageUrl)
    {
        BugReportId = bugReportId;
        FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        ContentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
        FileSizeBytes = fileSizeBytes;
        StorageUrl = storageUrl ?? throw new ArgumentNullException(nameof(storageUrl));
        Created = DateTime.UtcNow;
    }

    public void UpdateStorageUrl(string storageUrl)
    {
        StorageUrl = storageUrl ?? throw new ArgumentNullException(nameof(storageUrl));
    }
}

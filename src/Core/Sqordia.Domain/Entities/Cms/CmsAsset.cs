using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities.Cms;

/// <summary>
/// Represents a file asset uploaded for use in CMS content (images, documents, etc.).
/// </summary>
public class CmsAsset : BaseAuditableEntity
{
    public string FileName { get; private set; } = null!;
    public string ContentType { get; private set; } = null!;
    public string Url { get; private set; } = null!;
    public long FileSize { get; private set; }
    public Guid UploadedByUserId { get; private set; }
    public string Category { get; private set; } = null!;

    private CmsAsset() { } // EF Core constructor

    public CmsAsset(
        string fileName,
        string contentType,
        string url,
        long fileSize,
        Guid uploadedByUserId,
        string category)
    {
        FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        ContentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
        Url = url ?? throw new ArgumentNullException(nameof(url));
        Category = category ?? throw new ArgumentNullException(nameof(category));

        if (fileSize < 0)
            throw new ArgumentException("File size cannot be negative.", nameof(fileSize));

        if (uploadedByUserId == Guid.Empty)
            throw new ArgumentException("Uploaded by user ID cannot be empty.", nameof(uploadedByUserId));

        FileSize = fileSize;
        UploadedByUserId = uploadedByUserId;
    }
}

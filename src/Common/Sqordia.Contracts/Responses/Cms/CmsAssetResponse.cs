namespace Sqordia.Contracts.Responses.Cms;

/// <summary>
/// Response representing an uploaded CMS asset
/// </summary>
public class CmsAssetResponse
{
    public required Guid Id { get; set; }
    public required string FileName { get; set; }
    public required string ContentType { get; set; }
    public required string Url { get; set; }
    public required long FileSize { get; set; }
    public required string Category { get; set; }
    public required DateTime CreatedAt { get; set; }
}

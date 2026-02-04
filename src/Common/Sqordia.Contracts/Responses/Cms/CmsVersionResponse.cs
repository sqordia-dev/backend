namespace Sqordia.Contracts.Responses.Cms;

/// <summary>
/// Response representing a CMS version summary
/// </summary>
public class CmsVersionResponse
{
    public required Guid Id { get; set; }
    public required int VersionNumber { get; set; }
    public required string Status { get; set; }
    public string? Notes { get; set; }
    public required Guid CreatedByUserId { get; set; }
    public string? CreatedByUserName { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? PublishedByUserName { get; set; }
    public required int ContentBlockCount { get; set; }
    public required DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

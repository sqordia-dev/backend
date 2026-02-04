namespace Sqordia.Contracts.Responses.Cms;

/// <summary>
/// Response representing a single CMS content block
/// </summary>
public class CmsContentBlockResponse
{
    public required Guid Id { get; set; }
    public required string BlockKey { get; set; }
    public required string BlockType { get; set; }
    public required string Content { get; set; }
    public required string Language { get; set; }
    public required int SortOrder { get; set; }
    public required string SectionKey { get; set; }
    public string? Metadata { get; set; }
    public required DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

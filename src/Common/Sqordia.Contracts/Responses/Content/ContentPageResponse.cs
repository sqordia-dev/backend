namespace Sqordia.Contracts.Responses.Content;

/// <summary>
/// Content page response
/// </summary>
public class ContentPageResponse
{
    public required Guid Id { get; set; }
    public required string PageKey { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
    public required string Language { get; set; }
    public required bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public required int Version { get; set; }
    public required DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}


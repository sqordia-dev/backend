namespace Sqordia.Contracts.Responses.Cms;

/// <summary>
/// Response representing a CMS content template
/// </summary>
public class CmsContentTemplateResponse
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? PageKey { get; set; }
    public string? SectionKey { get; set; }
    public required string TemplateData { get; set; }
    public string? PreviewImageUrl { get; set; }
    public required bool IsPublic { get; set; }
    public required Guid CreatedByUserId { get; set; }
    public string? CreatedByUserName { get; set; }
    public required DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Summary response for template listings (without full template data)
/// </summary>
public class CmsContentTemplateSummaryResponse
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? PageKey { get; set; }
    public string? SectionKey { get; set; }
    public string? PreviewImageUrl { get; set; }
    public required bool IsPublic { get; set; }
    public required Guid CreatedByUserId { get; set; }
    public string? CreatedByUserName { get; set; }
    public required DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

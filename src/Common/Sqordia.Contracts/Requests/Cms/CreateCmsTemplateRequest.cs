namespace Sqordia.Contracts.Requests.Cms;

/// <summary>
/// Request to create a new CMS content template
/// </summary>
public class CreateCmsTemplateRequest
{
    /// <summary>
    /// Name of the template
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Optional description of the template
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Optional page key to scope the template to a specific page
    /// </summary>
    public string? PageKey { get; set; }

    /// <summary>
    /// Optional section key to scope the template to a specific section
    /// </summary>
    public string? SectionKey { get; set; }

    /// <summary>
    /// JSON data containing the template content blocks
    /// </summary>
    public required string TemplateData { get; set; }

    /// <summary>
    /// Optional preview image URL for the template
    /// </summary>
    public string? PreviewImageUrl { get; set; }

    /// <summary>
    /// Whether the template should be visible to all users
    /// </summary>
    public bool IsPublic { get; set; }
}

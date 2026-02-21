namespace Sqordia.Contracts.Requests.Cms;

/// <summary>
/// Request to update an existing CMS content template
/// </summary>
public class UpdateCmsTemplateRequest
{
    /// <summary>
    /// Updated name of the template
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Updated description of the template
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Updated page key scope
    /// </summary>
    public string? PageKey { get; set; }

    /// <summary>
    /// Updated section key scope
    /// </summary>
    public string? SectionKey { get; set; }

    /// <summary>
    /// Updated template data (optional, only updates if provided)
    /// </summary>
    public string? TemplateData { get; set; }

    /// <summary>
    /// Updated preview image URL
    /// </summary>
    public string? PreviewImageUrl { get; set; }

    /// <summary>
    /// Updated visibility setting
    /// </summary>
    public bool? IsPublic { get; set; }
}

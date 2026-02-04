namespace Sqordia.Contracts.Responses.Cms;

/// <summary>
/// Response containing published content organized by section keys
/// </summary>
public class PublishedContentResponse
{
    /// <summary>
    /// Content blocks grouped by section key
    /// </summary>
    public required Dictionary<string, List<CmsContentBlockResponse>> Sections { get; set; }
}

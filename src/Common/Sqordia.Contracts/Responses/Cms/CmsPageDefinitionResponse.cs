namespace Sqordia.Contracts.Responses.Cms;

/// <summary>
/// Response representing a CMS page definition with its sections
/// </summary>
public class CmsPageDefinitionResponse
{
    public required string Key { get; set; }
    public required string Label { get; set; }
    public required List<CmsSectionDefinitionResponse> Sections { get; set; }
}

/// <summary>
/// Response representing a CMS section definition within a page
/// </summary>
public class CmsSectionDefinitionResponse
{
    public required string Key { get; set; }
    public required string Label { get; set; }
    public required int SortOrder { get; set; }
}

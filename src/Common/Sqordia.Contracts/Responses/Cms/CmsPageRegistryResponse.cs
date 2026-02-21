namespace Sqordia.Contracts.Responses.Cms;

/// <summary>
/// Response representing a CMS page in the registry navigation.
/// </summary>
public class CmsPageRegistryResponse
{
    public required Guid Id { get; set; }
    public required string Key { get; set; }
    public required string Label { get; set; }
    public string? Description { get; set; }
    public required int SortOrder { get; set; }
    public string? IconName { get; set; }
    public string? SpecialRenderer { get; set; }
    public required List<CmsSectionResponse> Sections { get; set; }
}

/// <summary>
/// Detailed response for a CMS page including block definitions.
/// </summary>
public class CmsPageDetailResponse
{
    public required Guid Id { get; set; }
    public required string Key { get; set; }
    public required string Label { get; set; }
    public string? Description { get; set; }
    public required int SortOrder { get; set; }
    public required bool IsActive { get; set; }
    public string? IconName { get; set; }
    public string? SpecialRenderer { get; set; }
    public required List<CmsSectionDetailResponse> Sections { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

/// <summary>
/// Response representing a CMS section in the registry.
/// </summary>
public class CmsSectionResponse
{
    public required Guid Id { get; set; }
    public required string Key { get; set; }
    public required string Label { get; set; }
    public string? Description { get; set; }
    public required int SortOrder { get; set; }
    public string? IconName { get; set; }
}

/// <summary>
/// Detailed response for a CMS section including block definitions.
/// </summary>
public class CmsSectionDetailResponse
{
    public required Guid Id { get; set; }
    public required Guid CmsPageId { get; set; }
    public required string Key { get; set; }
    public required string Label { get; set; }
    public string? Description { get; set; }
    public required int SortOrder { get; set; }
    public required bool IsActive { get; set; }
    public string? IconName { get; set; }
    public required List<CmsBlockDefinitionResponse> BlockDefinitions { get; set; }
}

/// <summary>
/// Response representing a CMS block definition.
/// </summary>
public class CmsBlockDefinitionResponse
{
    public required Guid Id { get; set; }
    public required Guid CmsSectionId { get; set; }
    public required string BlockKey { get; set; }
    public required string BlockType { get; set; }
    public required string Label { get; set; }
    public string? Description { get; set; }
    public string? DefaultContent { get; set; }
    public required int SortOrder { get; set; }
    public required bool IsRequired { get; set; }
    public required bool IsActive { get; set; }
    public string? ValidationRules { get; set; }
    public string? MetadataSchema { get; set; }
    public string? Placeholder { get; set; }
    public int MaxLength { get; set; }
}

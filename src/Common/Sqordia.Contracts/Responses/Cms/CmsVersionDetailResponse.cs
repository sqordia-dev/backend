namespace Sqordia.Contracts.Responses.Cms;

/// <summary>
/// Detailed response for a CMS version including all its content blocks
/// </summary>
public class CmsVersionDetailResponse : CmsVersionResponse
{
    public required List<CmsContentBlockResponse> ContentBlocks { get; set; }
}

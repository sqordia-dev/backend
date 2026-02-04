using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services.Cms;
using Sqordia.Contracts.Responses.Cms;

namespace WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/cms/pages")]
[Authorize(Roles = "Admin")]
public class CmsPageRegistryController : BaseApiController
{
    /// <summary>
    /// Get the full CMS page registry (all pages and their sections)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<CmsPageDefinitionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetPageRegistry()
    {
        var pages = CmsPageRegistry.Pages.Select(p => new CmsPageDefinitionResponse
        {
            Key = p.Key,
            Label = p.Label,
            Sections = p.Sections.OrderBy(s => s.SortOrder).Select(s => new CmsSectionDefinitionResponse
            {
                Key = s.Key,
                Label = s.Label,
                SortOrder = s.SortOrder,
            }).ToList(),
        }).ToList();

        return Ok(pages);
    }
}

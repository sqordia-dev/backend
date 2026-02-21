using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services.Cms;
using Sqordia.Contracts.Responses.Cms;

namespace WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/cms/registry")]
[Authorize(Roles = "Admin")]
public class CmsPageRegistryController : BaseApiController
{
    private readonly ICmsRegistryService _registryService;

    public CmsPageRegistryController(ICmsRegistryService registryService)
    {
        _registryService = registryService;
    }

    /// <summary>
    /// Get the full CMS page registry (all pages and their sections)
    /// </summary>
    [HttpGet("pages")]
    [ProducesResponseType(typeof(List<CmsPageRegistryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPageRegistry(CancellationToken cancellationToken)
    {
        var result = await _registryService.GetAllPagesAsync(cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get a specific page with its sections and block definitions
    /// </summary>
    [HttpGet("pages/{pageKey}")]
    [ProducesResponseType(typeof(CmsPageDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPage(string pageKey, CancellationToken cancellationToken)
    {
        var result = await _registryService.GetPageAsync(pageKey, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get all sections for a specific page
    /// </summary>
    [HttpGet("pages/{pageKey}/sections")]
    [ProducesResponseType(typeof(List<CmsSectionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSections(string pageKey, CancellationToken cancellationToken)
    {
        var result = await _registryService.GetSectionsAsync(pageKey, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get all block definitions for a specific section
    /// </summary>
    [HttpGet("sections/{sectionKey}/block-definitions")]
    [ProducesResponseType(typeof(List<CmsBlockDefinitionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBlockDefinitions(string sectionKey, CancellationToken cancellationToken)
    {
        var result = await _registryService.GetBlockDefinitionsAsync(sectionKey, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Validate if a section key exists in the registry
    /// </summary>
    [HttpGet("sections/{sectionKey}/validate")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ValidateSectionKey(string sectionKey, CancellationToken cancellationToken)
    {
        var result = await _registryService.IsValidSectionKeyAsync(sectionKey, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Seed the database registry from the static registry (admin only)
    /// </summary>
    [HttpPost("seed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SeedRegistry(CancellationToken cancellationToken)
    {
        var result = await _registryService.SeedFromStaticRegistryAsync(cancellationToken);
        return HandleResult(result);
    }
}

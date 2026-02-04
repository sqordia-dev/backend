using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services.Cms;
using Sqordia.Contracts.Requests.Cms;
using Sqordia.Contracts.Responses.Cms;

namespace WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/cms/versions")]
[Authorize(Roles = "Admin")]
public class CmsVersionController : BaseApiController
{
    private readonly ICmsVersionService _versionService;
    private readonly ILogger<CmsVersionController> _logger;

    public CmsVersionController(
        ICmsVersionService versionService,
        ILogger<CmsVersionController> logger)
    {
        _versionService = versionService;
        _logger = logger;
    }

    /// <summary>
    /// Get all CMS versions
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<CmsVersionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllVersions(CancellationToken cancellationToken = default)
    {
        var result = await _versionService.GetAllVersionsAsync(cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get the current active draft version
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(CmsVersionDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetActiveVersion(CancellationToken cancellationToken = default)
    {
        var result = await _versionService.GetActiveVersionAsync(cancellationToken);
        if (result.IsSuccess && result.Value == null)
        {
            return NoContent();
        }
        return HandleResult(result);
    }

    /// <summary>
    /// Get a specific version by ID with all its content blocks
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CmsVersionDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetVersion(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _versionService.GetVersionAsync(id, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a new draft version
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CmsVersionDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateVersion([FromBody] CreateCmsVersionRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _versionService.CreateVersionAsync(request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Update version notes
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CmsVersionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateVersion(Guid id, [FromBody] UpdateCmsVersionRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _versionService.UpdateVersionAsync(id, request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Publish a draft version, making it live
    /// </summary>
    [HttpPost("{id:guid}/publish")]
    [ProducesResponseType(typeof(CmsVersionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> PublishVersion(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _versionService.PublishVersionAsync(id, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Delete a draft version
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteVersion(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _versionService.DeleteVersionAsync(id, cancellationToken);
        return HandleResult(result);
    }
}

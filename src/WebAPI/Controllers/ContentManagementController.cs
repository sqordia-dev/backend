using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services;
using Sqordia.Contracts.Requests.Content;

namespace WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/content")]
[Authorize(Roles = "Admin")]
public class ContentManagementController : BaseApiController
{
    private readonly IContentManagementService _contentService;
    private readonly ILogger<ContentManagementController> _logger;

    public ContentManagementController(
        IContentManagementService contentService,
        ILogger<ContentManagementController> logger)
    {
        _contentService = contentService;
        _logger = logger;
    }

    /// <summary>
    /// Get a content page (public endpoint, no auth required)
    /// </summary>
    [HttpGet("{pageKey}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetContentPage(
        string pageKey,
        [FromQuery] string language = "fr",
        CancellationToken cancellationToken = default)
    {
        var result = await _contentService.GetContentPageAsync(pageKey, language, cancellationToken);
        if (result == null)
        {
            return NotFound();
        }
        return Ok(result);
    }

    /// <summary>
    /// Get all content pages (admin only)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllContentPages(CancellationToken cancellationToken = default)
    {
        var result = await _contentService.GetAllContentPagesAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Update or create a content page (admin only)
    /// </summary>
    [HttpPut("{pageKey}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateContentPage(
        string pageKey,
        [FromBody] UpdateContentPageRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _contentService.UpdateContentPageAsync(pageKey, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Publish a content page (admin only)
    /// </summary>
    [HttpPost("{pageKey}/publish")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> PublishContentPage(
        string pageKey,
        [FromQuery] string language = "fr",
        CancellationToken cancellationToken = default)
    {
        var result = await _contentService.PublishContentPageAsync(pageKey, language, cancellationToken);
        return Ok(result);
    }
}


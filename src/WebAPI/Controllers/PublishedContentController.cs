using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services.Cms;
using Sqordia.Contracts.Responses.Cms;

namespace WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/content")]
[AllowAnonymous]
public class PublishedContentController : BaseApiController
{
    private readonly IPublishedContentService _publishedContentService;
    private readonly ILogger<PublishedContentController> _logger;

    public PublishedContentController(
        IPublishedContentService publishedContentService,
        ILogger<PublishedContentController> logger)
    {
        _publishedContentService = publishedContentService;
        _logger = logger;
    }

    /// <summary>
    /// Get published content, optionally filtered by section key
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PublishedContentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPublishedContent(
        [FromQuery] string? sectionKey,
        [FromQuery] string language = "fr",
        CancellationToken cancellationToken = default)
    {
        var result = await _publishedContentService.GetPublishedContentAsync(sectionKey, language, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get published content for a specific page (all sections matching the page key prefix)
    /// </summary>
    [HttpGet("pages/{pageKey}")]
    [ProducesResponseType(typeof(PublishedContentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPublishedContentByPage(
        string pageKey,
        [FromQuery] string language = "fr",
        CancellationToken cancellationToken = default)
    {
        var result = await _publishedContentService.GetPublishedContentByPageAsync(pageKey, language, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get a specific published content block by its block key
    /// </summary>
    [HttpGet("{blockKey}")]
    [ProducesResponseType(typeof(CmsContentBlockResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPublishedBlock(
        string blockKey,
        [FromQuery] string language = "fr",
        CancellationToken cancellationToken = default)
    {
        var result = await _publishedContentService.GetPublishedBlockAsync(blockKey, language, cancellationToken);
        return HandleResult(result);
    }
}

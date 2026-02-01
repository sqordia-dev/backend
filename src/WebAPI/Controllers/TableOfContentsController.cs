using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services;
using Sqordia.Contracts.Requests.TableOfContents;
using Sqordia.Contracts.Responses.TableOfContents;

namespace WebAPI.Controllers;

/// <summary>
/// Controller for managing business plan table of contents settings
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/business-plans/{businessPlanId}/toc-settings")]
[Authorize]
public class TableOfContentsController : BaseApiController
{
    private readonly ITableOfContentsService _tocService;
    private readonly ILogger<TableOfContentsController> _logger;

    public TableOfContentsController(
        ITableOfContentsService tocService,
        ILogger<TableOfContentsController> logger)
    {
        _tocService = tocService;
        _logger = logger;
    }

    /// <summary>
    /// Get table of contents settings for a business plan
    /// </summary>
    /// <param name="businessPlanId">The business plan ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Table of contents settings</returns>
    [HttpGet]
    [ProducesResponseType(typeof(TOCSettingsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSettings(
        Guid businessPlanId,
        CancellationToken cancellationToken = default)
    {
        var result = await _tocService.GetSettingsAsync(businessPlanId, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Update table of contents settings for a business plan
    /// </summary>
    /// <param name="businessPlanId">The business plan ID</param>
    /// <param name="request">TOC settings to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated table of contents settings</returns>
    [HttpPut]
    [ProducesResponseType(typeof(TOCSettingsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSettings(
        Guid businessPlanId,
        [FromBody] UpdateTOCSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _tocService.UpdateSettingsAsync(businessPlanId, request, cancellationToken);
        return HandleResult(result);
    }
}

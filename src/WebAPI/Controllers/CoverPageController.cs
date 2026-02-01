using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services;
using Sqordia.Contracts.Requests.CoverPage;
using Sqordia.Contracts.Responses.CoverPage;

namespace WebAPI.Controllers;

/// <summary>
/// Controller for managing business plan cover page settings
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/business-plans/{businessPlanId}/cover")]
[Authorize]
public class CoverPageController : BaseApiController
{
    private readonly ICoverPageService _coverPageService;
    private readonly ILogger<CoverPageController> _logger;

    public CoverPageController(
        ICoverPageService coverPageService,
        ILogger<CoverPageController> logger)
    {
        _coverPageService = coverPageService;
        _logger = logger;
    }

    /// <summary>
    /// Get cover page settings for a business plan
    /// </summary>
    /// <param name="businessPlanId">The business plan ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cover page settings</returns>
    [HttpGet]
    [ProducesResponseType(typeof(CoverPageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCoverPage(
        Guid businessPlanId,
        CancellationToken cancellationToken = default)
    {
        var result = await _coverPageService.GetCoverPageAsync(businessPlanId, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Update cover page settings for a business plan
    /// </summary>
    /// <param name="businessPlanId">The business plan ID</param>
    /// <param name="request">Cover page settings to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated cover page settings</returns>
    [HttpPut]
    [ProducesResponseType(typeof(CoverPageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCoverPage(
        Guid businessPlanId,
        [FromBody] UpdateCoverPageRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _coverPageService.UpdateCoverPageAsync(businessPlanId, request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Upload a logo for the cover page
    /// </summary>
    /// <param name="businessPlanId">The business plan ID</param>
    /// <param name="file">The logo file (PNG, JPG, SVG, WebP - max 2MB)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>URL of the uploaded logo</returns>
    [HttpPost("logo")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadLogo(
        Guid businessPlanId,
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No file provided" });
        }

        using var stream = file.OpenReadStream();
        var result = await _coverPageService.UploadLogoAsync(
            businessPlanId,
            stream,
            file.FileName,
            file.ContentType,
            cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Delete the logo from the cover page
    /// </summary>
    /// <param name="businessPlanId">The business plan ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpDelete("logo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteLogo(
        Guid businessPlanId,
        CancellationToken cancellationToken = default)
    {
        var result = await _coverPageService.DeleteLogoAsync(businessPlanId, cancellationToken);
        return HandleResult(result);
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services.Cms;
using Sqordia.Contracts.Requests.Cms;
using Sqordia.Contracts.Responses.Cms;

namespace WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/cms/assets")]
[Authorize(Roles = "Admin")]
public class CmsAssetController : BaseApiController
{
    private readonly ICmsAssetService _assetService;
    private readonly ILogger<CmsAssetController> _logger;

    public CmsAssetController(
        ICmsAssetService assetService,
        ILogger<CmsAssetController> logger)
    {
        _assetService = assetService;
        _logger = logger;
    }

    /// <summary>
    /// Upload a new CMS asset (image or file)
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(CmsAssetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UploadAsset([FromForm] UploadCmsAssetRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _assetService.UploadAssetAsync(request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get all assets, optionally filtered by category
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<CmsAssetResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAssets([FromQuery] string? category, CancellationToken cancellationToken = default)
    {
        var result = await _assetService.GetAssetsAsync(category, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Delete an asset by ID
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteAsset(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _assetService.DeleteAssetAsync(id, cancellationToken);
        return HandleResult(result);
    }
}

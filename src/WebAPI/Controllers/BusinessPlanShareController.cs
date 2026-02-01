using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Services;
using Sqordia.Application.Services.V2;
using Sqordia.Contracts.Requests.BusinessPlan;
using Sqordia.Contracts.Requests.V2.Share;
using Sqordia.Contracts.Responses.BusinessPlan;

namespace WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/business-plans/{businessPlanId}/shares")]
[Authorize]
public class BusinessPlanShareController : BaseApiController
{
    private readonly IBusinessPlanShareService _shareService;
    private readonly IVaultShareService _vaultShareService;
    private readonly ILogger<BusinessPlanShareController> _logger;

    public BusinessPlanShareController(
        IBusinessPlanShareService shareService,
        IVaultShareService vaultShareService,
        ILogger<BusinessPlanShareController> logger)
    {
        _shareService = shareService;
        _vaultShareService = vaultShareService;
        _logger = logger;
    }

    /// <summary>
    /// Share a business plan with a specific user
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ShareBusinessPlan(
        Guid businessPlanId,
        [FromBody] ShareBusinessPlanRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _shareService.ShareBusinessPlanAsync(businessPlanId, request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a public share link for a business plan
    /// </summary>
    [HttpPost("public")]
    public async Task<IActionResult> CreatePublicShare(
        Guid businessPlanId,
        [FromBody] CreatePublicShareRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _shareService.CreatePublicShareAsync(businessPlanId, request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get all shares for a business plan
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetShares(
        Guid businessPlanId,
        CancellationToken cancellationToken = default)
    {
        var result = await _shareService.GetSharesAsync(businessPlanId, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Revoke a share
    /// </summary>
    [HttpDelete("{shareId}")]
    public async Task<IActionResult> RevokeShare(
        Guid businessPlanId,
        Guid shareId,
        CancellationToken cancellationToken = default)
    {
        var result = await _shareService.RevokeShareAsync(businessPlanId, shareId, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Update share permissions
    /// </summary>
    [HttpPut("{shareId}/permission")]
    public async Task<IActionResult> UpdateSharePermission(
        Guid businessPlanId,
        Guid shareId,
        [FromBody] UpdateSharePermissionRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _shareService.UpdateSharePermissionAsync(businessPlanId, shareId, request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get a business plan by public token (no authentication required)
    /// Note: businessPlanId parameter is required by route but ignored - lookup is by token only
    /// </summary>
    [HttpGet("public/{token}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBusinessPlanByPublicToken(
        Guid businessPlanId, // Required by route but not used - lookup is by token
        string token,
        CancellationToken cancellationToken = default)
    {
        var result = await _shareService.GetBusinessPlanByPublicTokenAsync(token, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a secure vault share for a business plan (V1 compatibility endpoint)
    /// Provides enhanced sharing with watermarking, view tracking, and password protection
    /// </summary>
    /// <param name="businessPlanId">The business plan ID</param>
    /// <param name="request">Vault share configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpPost("vault")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateVaultShare(
        Guid businessPlanId,
        [FromBody] CreateVaultShareRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid vault share request for plan {PlanId}: {Errors}", businessPlanId, ModelState);
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Creating vault share for plan {PlanId} via V1 endpoint", businessPlanId);

        var result = await _vaultShareService.CreateVaultShareAsync(businessPlanId, request, cancellationToken);
        return HandleResult(result);
    }
}


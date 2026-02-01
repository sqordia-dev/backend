using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services.V2;
using Sqordia.Contracts.Requests.V2.Share;

namespace WebAPI.Controllers.V2;

/// <summary>
/// Secure vault share endpoints
/// Provides enhanced sharing with watermarking, view tracking, and password protection
/// </summary>
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/business-plans/{businessPlanId}/vault")]
[Authorize]
public class VaultShareController : BaseApiController
{
    private readonly IVaultShareService _vaultShareService;
    private readonly ILogger<VaultShareController> _logger;

    public VaultShareController(
        IVaultShareService vaultShareService,
        ILogger<VaultShareController> logger)
    {
        _vaultShareService = vaultShareService;
        _logger = logger;
    }

    /// <summary>
    /// Create a secure vault share for a business plan
    /// </summary>
    /// <param name="businessPlanId">The business plan ID</param>
    /// <param name="request">Vault share configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpPost]
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
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Creating vault share for plan {PlanId}", businessPlanId);

        var result = await _vaultShareService.CreateVaultShareAsync(businessPlanId, request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get all vault shares for a business plan
    /// </summary>
    /// <param name="businessPlanId">The business plan ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVaultShares(
        Guid businessPlanId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting vault shares for plan {PlanId}", businessPlanId);

        var result = await _vaultShareService.GetVaultSharesAsync(businessPlanId, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get a specific vault share
    /// </summary>
    /// <param name="businessPlanId">The business plan ID</param>
    /// <param name="shareId">The share ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("{shareId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVaultShare(
        Guid businessPlanId,
        Guid shareId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting vault share {ShareId} for plan {PlanId}", shareId, businessPlanId);

        var result = await _vaultShareService.GetVaultShareAsync(shareId, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get analytics for a vault share
    /// </summary>
    /// <param name="businessPlanId">The business plan ID</param>
    /// <param name="shareId">The share ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("{shareId}/analytics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVaultShareAnalytics(
        Guid businessPlanId,
        Guid shareId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting analytics for vault share {ShareId}", shareId);

        var result = await _vaultShareService.GetVaultShareAnalyticsAsync(shareId, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Revoke a vault share
    /// </summary>
    /// <param name="businessPlanId">The business plan ID</param>
    /// <param name="shareId">The share ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpDelete("{shareId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeVaultShare(
        Guid businessPlanId,
        Guid shareId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Revoking vault share {ShareId}", shareId);

        var result = await _vaultShareService.RevokeVaultShareAsync(shareId, cancellationToken);
        return HandleResult(result);
    }
}

/// <summary>
/// Public vault access endpoints (no authentication required)
/// </summary>
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/vault")]
public class VaultAccessController : BaseApiController
{
    private readonly IVaultShareService _vaultShareService;
    private readonly ILogger<VaultAccessController> _logger;

    public VaultAccessController(
        IVaultShareService vaultShareService,
        ILogger<VaultAccessController> logger)
    {
        _vaultShareService = vaultShareService;
        _logger = logger;
    }

    /// <summary>
    /// Record a view of a shared document (called when vault is accessed)
    /// </summary>
    /// <param name="token">The share token</param>
    /// <param name="viewerEmail">Optional viewer email</param>
    /// <param name="viewerName">Optional viewer name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpPost("{token}/view")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RecordView(
        string token,
        [FromQuery] string? viewerEmail = null,
        [FromQuery] string? viewerName = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Recording view for vault share {Token}", token);

        var result = await _vaultShareService.RecordViewAsync(token, viewerEmail, viewerName, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Validate password for a password-protected vault share
    /// </summary>
    /// <param name="token">The share token</param>
    /// <param name="password">The password to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpPost("{token}/validate-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ValidatePassword(
        string token,
        [FromBody] ValidatePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Validating password for vault share {Token}", token);

        var result = await _vaultShareService.ValidatePasswordAsync(token, request.Password, cancellationToken);
        return HandleResult(result);
    }
}

/// <summary>
/// Request to validate a vault share password
/// </summary>
public class ValidatePasswordRequest
{
    public required string Password { get; set; }
}

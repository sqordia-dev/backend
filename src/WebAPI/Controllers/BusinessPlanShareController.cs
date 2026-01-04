using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services;
using Sqordia.Contracts.Requests.BusinessPlan;
using Sqordia.Contracts.Responses.BusinessPlan;

namespace WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/business-plans/{businessPlanId}/shares")]
[Authorize]
public class BusinessPlanShareController : BaseApiController
{
    private readonly IBusinessPlanShareService _shareService;

    public BusinessPlanShareController(IBusinessPlanShareService shareService)
    {
        _shareService = shareService;
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
}


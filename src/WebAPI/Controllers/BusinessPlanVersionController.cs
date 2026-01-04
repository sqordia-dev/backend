using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services;
using Sqordia.Contracts.Requests.BusinessPlan;
using Sqordia.Contracts.Responses.BusinessPlan;

namespace WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/business-plans/{businessPlanId}/versions")]
[Authorize]
public class BusinessPlanVersionController : BaseApiController
{
    private readonly IBusinessPlanVersionService _versionService;

    public BusinessPlanVersionController(IBusinessPlanVersionService versionService)
    {
        _versionService = versionService;
    }

    /// <summary>
    /// Create a new version snapshot of a business plan
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateVersion(
        Guid businessPlanId,
        [FromBody] CreateVersionRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _versionService.CreateVersionAsync(businessPlanId, request?.Comment, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get all versions of a business plan
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetVersions(
        Guid businessPlanId,
        CancellationToken cancellationToken = default)
    {
        var result = await _versionService.GetVersionsAsync(businessPlanId, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get a specific version of a business plan
    /// </summary>
    [HttpGet("{versionNumber}")]
    public async Task<IActionResult> GetVersion(
        Guid businessPlanId,
        int versionNumber,
        CancellationToken cancellationToken = default)
    {
        var result = await _versionService.GetVersionAsync(businessPlanId, versionNumber, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Restore a business plan to a specific version
    /// </summary>
    [HttpPost("{versionNumber}/restore")]
    public async Task<IActionResult> RestoreVersion(
        Guid businessPlanId,
        int versionNumber,
        CancellationToken cancellationToken = default)
    {
        var result = await _versionService.RestoreVersionAsync(businessPlanId, versionNumber, cancellationToken);
        return HandleResult(result);
    }
}

public record CreateVersionRequest(string? Comment);


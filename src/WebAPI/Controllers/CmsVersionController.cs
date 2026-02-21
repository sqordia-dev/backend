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
    private readonly ICmsApprovalService _approvalService;
    private readonly ICmsDiffService _diffService;
    private readonly ILogger<CmsVersionController> _logger;

    public CmsVersionController(
        ICmsVersionService versionService,
        ICmsApprovalService approvalService,
        ICmsDiffService diffService,
        ILogger<CmsVersionController> logger)
    {
        _versionService = versionService;
        _approvalService = approvalService;
        _diffService = diffService;
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

    // ========== Approval Workflow Endpoints ==========

    /// <summary>
    /// Submit a draft version for approval
    /// </summary>
    [HttpPost("{id:guid}/submit-for-approval")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SubmitForApproval(Guid id, [FromBody] SubmitForApprovalRequest? request, CancellationToken cancellationToken = default)
    {
        var result = await _approvalService.SubmitForApprovalAsync(id, request?.Notes, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Approve a version that is pending approval
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ApproveVersion(Guid id, [FromBody] ApproveVersionRequest? request, CancellationToken cancellationToken = default)
    {
        var result = await _approvalService.ApproveAsync(id, request?.Notes, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Reject a version that is pending approval
    /// </summary>
    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RejectVersion(Guid id, [FromBody] RejectVersionRequest? request, CancellationToken cancellationToken = default)
    {
        var result = await _approvalService.RejectAsync(id, request?.Reason, cancellationToken);
        return HandleResult(result);
    }

    // ========== Scheduling Endpoints ==========

    /// <summary>
    /// Schedule a version for future publishing
    /// </summary>
    [HttpPost("{id:guid}/schedule")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ScheduleVersion(Guid id, [FromBody] ScheduleVersionRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _approvalService.ScheduleAsync(id, request.PublishAt, request.Notes, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Cancel the scheduled publishing for a version
    /// </summary>
    [HttpDelete("{id:guid}/schedule")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CancelSchedule(Guid id, [FromBody] CancelScheduleRequest? request, CancellationToken cancellationToken = default)
    {
        var result = await _approvalService.CancelScheduleAsync(id, request?.Notes, cancellationToken);
        return HandleResult(result);
    }

    // ========== History Endpoint ==========

    /// <summary>
    /// Get the history of actions performed on a version
    /// </summary>
    [HttpGet("{id:guid}/history")]
    [ProducesResponseType(typeof(List<CmsVersionHistoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetVersionHistory(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _approvalService.GetHistoryAsync(id, cancellationToken);
        return HandleResult(result);
    }

    // ========== Restore Endpoint ==========

    /// <summary>
    /// Restore a previous version by creating a new draft with the same content
    /// </summary>
    [HttpPost("{id:guid}/restore")]
    [ProducesResponseType(typeof(CmsVersionDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RestoreVersion(Guid id, [FromBody] RestoreVersionRequest? request, CancellationToken cancellationToken = default)
    {
        var result = await _versionService.RestoreVersionAsync(id, request?.Notes, cancellationToken);
        return HandleResult(result);
    }

    // ========== Compare/Diff Endpoints ==========

    /// <summary>
    /// Compare two versions and get the diff
    /// </summary>
    [HttpGet("compare")]
    [ProducesResponseType(typeof(CmsDiffResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CompareVersions(
        [FromQuery] Guid sourceId,
        [FromQuery] Guid targetId,
        [FromQuery] string? language = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _diffService.CompareVersionsAsync(sourceId, targetId, language, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get the diff between the current draft and the published version
    /// </summary>
    [HttpGet("diff")]
    [ProducesResponseType(typeof(CmsDiffResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDraftVsPublishedDiff([FromQuery] string? language = null, CancellationToken cancellationToken = default)
    {
        var result = await _diffService.GetDraftVsPublishedDiffAsync(language, cancellationToken);
        return HandleResult(result);
    }
}

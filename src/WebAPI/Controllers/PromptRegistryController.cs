using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services;
using Sqordia.Contracts.Requests.Admin.PromptRegistry;
using Sqordia.Domain.Enums;

namespace WebAPI.Controllers;

/// <summary>
/// Controller for managing prompt templates in the admin registry
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/prompt-registry")]
[Authorize(Roles = "Admin")]
public class PromptRegistryController : BaseApiController
{
    private readonly IPromptRegistryService _service;
    private readonly ILogger<PromptRegistryController> _logger;

    public PromptRegistryController(
        IPromptRegistryService service,
        ILogger<PromptRegistryController> logger)
    {
        _service = service;
        _logger = logger;
    }

    #region CRUD Operations

    /// <summary>
    /// Gets all prompt templates with filtering and pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll([FromQuery] PromptRegistryFilter filter, CancellationToken ct)
    {
        _logger.LogInformation("Getting prompt templates with filter: {@Filter}", filter);
        var result = await _service.GetAllAsync(filter, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// Gets a single prompt template by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        _logger.LogInformation("Getting prompt template {PromptId}", id);
        var result = await _service.GetByIdAsync(id, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// Creates a new prompt template
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreatePromptTemplateRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId()?.ToString() ?? "unknown";
        _logger.LogInformation("Creating prompt template for section {SectionType} by user {UserId}",
            request.SectionType, userId);

        var result = await _service.CreateAsync(request, userId, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// Updates an existing prompt template
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePromptTemplateRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId()?.ToString() ?? "unknown";
        _logger.LogInformation("Updating prompt template {PromptId} by user {UserId}", id, userId);

        var result = await _service.UpdateAsync(id, request, userId, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// Deletes a prompt template
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        _logger.LogInformation("Deleting prompt template {PromptId}", id);
        var result = await _service.DeleteAsync(id, ct);
        return HandleResult(result);
    }

    #endregion

    #region Activation & Deployment

    /// <summary>
    /// Activates a prompt template (deactivates others for same section/plan)
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
    {
        _logger.LogInformation("Activating prompt template {PromptId}", id);
        var result = await _service.ActivateAsync(id, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// Deactivates a prompt template
    /// </summary>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        _logger.LogInformation("Deactivating prompt template {PromptId}", id);
        var result = await _service.DeactivateAsync(id, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// Sets the deployment alias for a prompt template
    /// </summary>
    [HttpPost("{id:guid}/alias")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SetAlias(Guid id, [FromBody] SetAliasRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Setting alias {Alias} for prompt template {PromptId}", request.Alias, id);
        var result = await _service.SetAliasAsync(id, request.Alias, ct);
        return HandleResult(result);
    }

    #endregion

    #region Versioning

    /// <summary>
    /// Creates a new version of an existing prompt template
    /// </summary>
    [HttpPost("{id:guid}/create-version")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateVersion(Guid id, CancellationToken ct)
    {
        var userId = GetCurrentUserId()?.ToString() ?? "unknown";
        _logger.LogInformation("Creating new version of prompt template {PromptId} by user {UserId}", id, userId);

        var result = await _service.CreateNewVersionAsync(id, userId, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// Gets the version history for a section/plan type combination
    /// </summary>
    [HttpGet("versions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetVersionHistory(
        [FromQuery] SectionType sectionType,
        [FromQuery] BusinessPlanType planType,
        [FromQuery] string? industryCategory,
        CancellationToken ct)
    {
        _logger.LogInformation("Getting version history for {SectionType}/{PlanType}", sectionType, planType);
        var result = await _service.GetVersionHistoryAsync(sectionType, planType, industryCategory, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// Rolls back to a specific version (activates it)
    /// </summary>
    [HttpPost("{id:guid}/rollback")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Rollback(Guid id, CancellationToken ct)
    {
        _logger.LogInformation("Rolling back to prompt template {PromptId}", id);
        var result = await _service.RollbackToVersionAsync(id, ct);
        return HandleResult(result);
    }

    #endregion

    #region Testing

    /// <summary>
    /// Tests an existing prompt with sample data
    /// </summary>
    [HttpPost("{id:guid}/test")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> TestPrompt(Guid id, [FromBody] TestPromptRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Testing prompt template {PromptId}", id);
        var result = await _service.TestPromptAsync(id, request, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// Tests a draft prompt (before saving) with sample data
    /// </summary>
    [HttpPost("test-draft")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> TestDraft([FromBody] TestDraftPromptRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Testing draft prompt for {SectionType}/{PlanType}", request.SectionType, request.PlanType);
        var result = await _service.TestDraftPromptAsync(request, ct);
        return HandleResult(result);
    }

    #endregion

    #region Performance Metrics

    /// <summary>
    /// Gets detailed performance metrics for a prompt template
    /// </summary>
    [HttpGet("{id:guid}/performance")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPerformance(Guid id, [FromQuery] DateTime? startDate, CancellationToken ct)
    {
        _logger.LogInformation("Getting performance metrics for prompt template {PromptId}", id);
        var result = await _service.GetPerformanceAsync(id, startDate, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// Gets a summary of performance across all prompts
    /// </summary>
    [HttpGet("performance-summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPerformanceSummary(CancellationToken ct)
    {
        _logger.LogInformation("Getting performance summary");
        var result = await _service.GetPerformanceSummaryAsync(ct);
        return HandleResult(result);
    }

    #endregion
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services.V3;
using Sqordia.Contracts.Requests.Admin.SectionPrompt;
using Sqordia.Contracts.Responses.Admin.SectionPrompt;

namespace WebAPI.Controllers;

/// <summary>
/// Admin controller for managing section prompts with master/override hierarchy
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/section-prompts")]
[Authorize(Roles = "Admin")]
public class AdminSectionPromptController : BaseApiController
{
    private readonly ISectionPromptService _service;

    public AdminSectionPromptController(ISectionPromptService service)
    {
        _service = service;
    }

    /// <summary>
    /// Get all section prompts with optional filtering
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<SectionPromptListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPrompts(
        [FromQuery] SectionPromptFilterRequest? filter,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetPromptsAsync(filter, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get a section prompt by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SectionPromptResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPromptById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetPromptByIdAsync(id, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get the effective prompt for a sub-section (resolves master/override hierarchy)
    /// </summary>
    [HttpGet("effective")]
    [ProducesResponseType(typeof(SectionPromptResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEffectivePrompt(
        [FromQuery] Guid subSectionId,
        [FromQuery] string planType,
        [FromQuery] string language,
        [FromQuery] string? industryCategory,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<Sqordia.Domain.Enums.BusinessPlanType>(planType, true, out var parsedPlanType))
        {
            return BadRequest(new { message = "Invalid plan type" });
        }

        var result = await _service.GetEffectivePromptAsync(
            subSectionId,
            parsedPlanType,
            language,
            industryCategory,
            cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get the master prompt for a main section
    /// </summary>
    [HttpGet("master")]
    [ProducesResponseType(typeof(SectionPromptResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMasterPrompt(
        [FromQuery] Guid mainSectionId,
        [FromQuery] string planType,
        [FromQuery] string language,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<Sqordia.Domain.Enums.BusinessPlanType>(planType, true, out var parsedPlanType))
        {
            return BadRequest(new { message = "Invalid plan type" });
        }

        var result = await _service.GetMasterPromptAsync(
            mainSectionId,
            parsedPlanType,
            language,
            cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a new section prompt
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePrompt(
        [FromBody] CreateSectionPromptRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.CreatePromptAsync(request, cancellationToken);
        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetPromptById), new { id = result.Value }, new { Id = result.Value });
        }
        return HandleResult(result);
    }

    /// <summary>
    /// Update a section prompt
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePrompt(
        Guid id,
        [FromBody] UpdateSectionPromptRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.UpdatePromptAsync(id, request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Delete (deactivate) a section prompt
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePrompt(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.DeletePromptAsync(id, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Activate a section prompt
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivatePrompt(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.ActivatePromptAsync(id, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Deactivate a section prompt
    /// </summary>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivatePrompt(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.DeactivatePromptAsync(id, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Test a section prompt with provided variables
    /// </summary>
    [HttpPost("{id:guid}/test")]
    [ProducesResponseType(typeof(SectionPromptTestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TestPrompt(
        Guid id,
        [FromBody] TestSectionPromptRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.TestPromptAsync(id, request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get prompt versions for a section
    /// </summary>
    [HttpGet("versions")]
    [ProducesResponseType(typeof(List<SectionPromptListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPromptVersions(
        [FromQuery] Guid mainSectionId,
        [FromQuery] Guid? subSectionId,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetPromptVersionsAsync(mainSectionId, subSectionId, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Clone a section prompt
    /// </summary>
    [HttpPost("{id:guid}/clone")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ClonePrompt(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.ClonePromptAsync(id, cancellationToken);
        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetPromptById), new { id = result.Value }, new { Id = result.Value });
        }
        return HandleResult(result);
    }
}

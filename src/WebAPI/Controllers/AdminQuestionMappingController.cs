using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services.V3;
using Sqordia.Contracts.Requests.Admin.QuestionMapping;
using Sqordia.Contracts.Responses.Admin.QuestionMapping;

namespace WebAPI.Controllers;

/// <summary>
/// Admin controller for managing question-to-section mappings
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/question-mappings")]
[Authorize(Roles = "Admin")]
public class AdminQuestionMappingController : BaseApiController
{
    private readonly IQuestionSectionMappingService _service;

    public AdminQuestionMappingController(IQuestionSectionMappingService service)
    {
        _service = service;
    }

    /// <summary>
    /// Get all question mappings with optional filtering
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<QuestionMappingResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMappings(
        [FromQuery] QuestionMappingFilterRequest? filter,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetMappingsAsync(filter, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get a mapping by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(QuestionMappingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMappingById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetMappingByIdAsync(id, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get mappings for a specific question
    /// </summary>
    [HttpGet("by-question/{questionId:guid}")]
    [ProducesResponseType(typeof(List<QuestionMappingResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMappingsForQuestion(Guid questionId, CancellationToken cancellationToken)
    {
        var result = await _service.GetMappingsForQuestionAsync(questionId, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get mappings for a specific sub-section
    /// </summary>
    [HttpGet("by-subsection/{subSectionId:guid}")]
    [ProducesResponseType(typeof(List<QuestionMappingResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMappingsForSubSection(Guid subSectionId, CancellationToken cancellationToken)
    {
        var result = await _service.GetMappingsForSubSectionAsync(subSectionId, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get the mapping matrix for admin UI (questions × sub-sections)
    /// </summary>
    [HttpGet("matrix")]
    [ProducesResponseType(typeof(MappingMatrixResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMappingMatrix(
        [FromQuery] MappingMatrixRequest? request,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetMappingMatrixAsync(request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get mapping statistics
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(MappingStatsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMappingStats(CancellationToken cancellationToken)
    {
        var result = await _service.GetMappingStatsAsync(cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a new mapping
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateMapping(
        [FromBody] CreateQuestionMappingRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.CreateMappingAsync(request, cancellationToken);
        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetMappingById), new { id = result.Value }, new { Id = result.Value });
        }
        return HandleResult(result);
    }

    /// <summary>
    /// Update a mapping
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMapping(
        Guid id,
        [FromBody] UpdateQuestionMappingRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.UpdateMappingAsync(id, request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Delete a mapping
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMapping(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.DeleteMappingAsync(id, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Bulk update mappings (for matrix UI)
    /// </summary>
    [HttpPut("bulk")]
    [ProducesResponseType(typeof(BulkUpdateResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> BulkUpdateMappings(
        [FromBody] BulkUpdateMappingsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.BulkUpdateMappingsAsync(request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Toggle a mapping on/off
    /// </summary>
    [HttpPost("toggle")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ToggleMapping(
        [FromQuery] Guid questionId,
        [FromQuery] Guid subSectionId,
        CancellationToken cancellationToken)
    {
        var result = await _service.ToggleMappingAsync(questionId, subSectionId, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Check if a mapping exists
    /// </summary>
    [HttpGet("exists")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> MappingExists(
        [FromQuery] Guid questionId,
        [FromQuery] Guid subSectionId,
        CancellationToken cancellationToken)
    {
        var result = await _service.MappingExistsAsync(questionId, subSectionId, cancellationToken);
        return HandleResult(result);
    }
}

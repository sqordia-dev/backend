using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Contracts.Requests.Admin;
using Sqordia.Contracts.Responses.Admin;

namespace WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/question-templates")]
[Authorize(Roles = "Admin")]
public class AdminQuestionTemplateController : BaseApiController
{
    private readonly IAdminQuestionTemplateService _service;

    public AdminQuestionTemplateController(IAdminQuestionTemplateService service)
    {
        _service = service;
    }

    /// <summary>
    /// Get all question templates with optional filtering
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<QuestionTemplateDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? stepNumber = null,
        [FromQuery] string? personaType = null,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var questions = await _service.GetAllQuestionsAsync(stepNumber, personaType, isActive, cancellationToken);
        return Ok(questions);
    }

    /// <summary>
    /// Get a question template by ID
    /// </summary>
    [HttpGet("{questionId:guid}")]
    [ProducesResponseType(typeof(QuestionTemplateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid questionId,
        CancellationToken cancellationToken = default)
    {
        var question = await _service.GetQuestionByIdAsync(questionId, cancellationToken);
        if (question == null) return NotFound();
        return Ok(question);
    }

    /// <summary>
    /// Create a new question template
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateQuestionTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var id = await _service.CreateQuestionAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { questionId = id }, new { Id = id });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing question template
    /// </summary>
    [HttpPut("{questionId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(
        Guid questionId,
        [FromBody] UpdateQuestionTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var success = await _service.UpdateQuestionAsync(questionId, request, cancellationToken);
            if (!success) return NotFound();
            return Ok(new { message = "Question updated successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Delete (deactivate) a question template
    /// </summary>
    [HttpDelete("{questionId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid questionId,
        CancellationToken cancellationToken = default)
    {
        var success = await _service.DeleteQuestionAsync(questionId, cancellationToken);
        if (!success) return NotFound();
        return Ok(new { message = "Question deleted successfully" });
    }

    /// <summary>
    /// Reorder question templates
    /// </summary>
    [HttpPut("reorder")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Reorder(
        [FromBody] ReorderQuestionsRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _service.ReorderQuestionsAsync(request, cancellationToken);
            return Ok(new { message = "Questions reordered successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Toggle question active status
    /// </summary>
    [HttpPatch("{questionId:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleStatus(
        Guid questionId,
        [FromBody] bool isActive,
        CancellationToken cancellationToken = default)
    {
        var success = await _service.ToggleQuestionStatusAsync(questionId, isActive, cancellationToken);
        if (!success) return NotFound();
        return Ok(new { message = $"Question {(isActive ? "activated" : "deactivated")} successfully" });
    }
}

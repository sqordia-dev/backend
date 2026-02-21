using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services.Questionnaire;
using Sqordia.Contracts.Requests.Admin;
using Sqordia.Contracts.Requests.Questionnaire;
using Sqordia.Contracts.Responses.Questionnaire;

namespace WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/questionnaire-versions")]
[Authorize(Roles = "Admin")]
public class AdminQuestionnaireVersionController : BaseApiController
{
    private readonly IQuestionnaireVersionService _service;

    public AdminQuestionnaireVersionController(IQuestionnaireVersionService service)
    {
        _service = service;
    }

    #region Version Management

    /// <summary>
    /// Get all questionnaire versions (history)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<QuestionnaireVersionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVersionHistory(CancellationToken cancellationToken = default)
    {
        var result = await _service.GetVersionHistoryAsync(cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get the active draft version (if any)
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(QuestionnaireVersionDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetActiveDraft(CancellationToken cancellationToken = default)
    {
        var result = await _service.GetActiveDraftAsync(cancellationToken);
        if (result.IsFailure) return HandleResult(result);
        if (result.Value == null) return NoContent();
        return Ok(result.Value);
    }

    /// <summary>
    /// Get the currently published version
    /// </summary>
    [HttpGet("published")]
    [ProducesResponseType(typeof(QuestionnaireVersionDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPublishedVersion(CancellationToken cancellationToken = default)
    {
        var result = await _service.GetPublishedVersionAsync(cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get a specific version by ID
    /// </summary>
    [HttpGet("{versionId:guid}")]
    [ProducesResponseType(typeof(QuestionnaireVersionDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVersionById(
        Guid versionId,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.GetVersionByIdAsync(versionId, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a new draft version (clones from published)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(QuestionnaireVersionDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateDraft(
        [FromBody] CreateQuestionnaireVersionRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.CreateDraftAsync(request?.Notes, cancellationToken);
        if (result.IsFailure) return HandleResult(result);
        return CreatedAtAction(nameof(GetVersionById), new { versionId = result.Value.Id }, result.Value);
    }

    /// <summary>
    /// Publish a draft version
    /// </summary>
    [HttpPost("{versionId:guid}/publish")]
    [ProducesResponseType(typeof(QuestionnaireVersionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PublishDraft(
        Guid versionId,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.PublishDraftAsync(versionId, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Discard (delete) a draft version
    /// </summary>
    [HttpDelete("{versionId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DiscardDraft(
        Guid versionId,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.DiscardDraftAsync(versionId, cancellationToken);
        if (result.IsFailure) return HandleResult(result);
        return NoContent();
    }

    /// <summary>
    /// Restore an archived version as a new draft
    /// </summary>
    [HttpPost("{versionId:guid}/restore")]
    [ProducesResponseType(typeof(QuestionnaireVersionDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RestoreVersion(
        Guid versionId,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.RestoreVersionAsync(versionId, cancellationToken);
        if (result.IsFailure) return HandleResult(result);
        return CreatedAtAction(nameof(GetVersionById), new { versionId = result.Value.Id }, result.Value);
    }

    #endregion

    #region Draft Question Editing

    /// <summary>
    /// Create a new question in the draft version
    /// </summary>
    [HttpPost("{versionId:guid}/questions")]
    [ProducesResponseType(typeof(Sqordia.Contracts.Responses.Admin.QuestionTemplateDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateQuestion(
        Guid versionId,
        [FromBody] CreateQuestionTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.CreateQuestionInDraftAsync(versionId, request, cancellationToken);
        if (result.IsFailure) return HandleResult(result);
        return Created($"/api/v1/admin/questionnaire-versions/{versionId}/questions/{result.Value.Id}", result.Value);
    }

    /// <summary>
    /// Update a question in the draft version
    /// </summary>
    [HttpPut("{versionId:guid}/questions/{questionId:guid}")]
    [ProducesResponseType(typeof(Sqordia.Contracts.Responses.Admin.QuestionTemplateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateQuestion(
        Guid versionId,
        Guid questionId,
        [FromBody] UpdateQuestionTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.UpdateQuestionInDraftAsync(versionId, questionId, request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Delete a question from the draft version
    /// </summary>
    [HttpDelete("{versionId:guid}/questions/{questionId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteQuestion(
        Guid versionId,
        Guid questionId,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.DeleteQuestionFromDraftAsync(versionId, questionId, cancellationToken);
        if (result.IsFailure) return HandleResult(result);
        return NoContent();
    }

    /// <summary>
    /// Reorder questions in the draft version
    /// </summary>
    [HttpPut("{versionId:guid}/questions/reorder")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReorderQuestions(
        Guid versionId,
        [FromBody] ReorderQuestionsRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.ReorderQuestionsInDraftAsync(versionId, request, cancellationToken);
        if (result.IsFailure) return HandleResult(result);
        return NoContent();
    }

    #endregion

    #region Draft Step Editing

    /// <summary>
    /// Update a step configuration in the draft version
    /// </summary>
    [HttpPut("{versionId:guid}/steps/{stepNumber:int}")]
    [ProducesResponseType(typeof(QuestionnaireStepDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateStep(
        Guid versionId,
        int stepNumber,
        [FromBody] UpdateQuestionnaireStepRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.UpdateStepInDraftAsync(versionId, stepNumber, request, cancellationToken);
        return HandleResult(result);
    }

    #endregion
}

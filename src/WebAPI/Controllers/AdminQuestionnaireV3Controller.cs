using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services.V3;
using Sqordia.Contracts.Requests.Admin.QuestionnaireV3;
using Sqordia.Contracts.Responses.Admin.QuestionnaireV3;

namespace WebAPI.Controllers;

/// <summary>
/// Admin controller for managing V3 questionnaire templates
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/questions-v3")]
[Authorize(Roles = "Admin")]
public class AdminQuestionnaireV3Controller : BaseApiController
{
    private readonly IQuestionnaireServiceV3 _service;

    public AdminQuestionnaireV3Controller(IQuestionnaireServiceV3 service)
    {
        _service = service;
    }

    /// <summary>
    /// Get all V3 questions with optional filtering
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<QuestionTemplateV3ListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetQuestions(
        [FromQuery] QuestionTemplateV3FilterRequest? filter,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetQuestionsAsync(filter, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get a V3 question by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(QuestionTemplateV3Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetQuestionById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetQuestionByIdAsync(id, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get a V3 question by question number
    /// </summary>
    [HttpGet("by-number/{questionNumber:int}")]
    [ProducesResponseType(typeof(QuestionTemplateV3Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetQuestionByNumber(int questionNumber, CancellationToken cancellationToken)
    {
        var result = await _service.GetQuestionByNumberAsync(questionNumber, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get V3 questions by step number
    /// </summary>
    [HttpGet("by-step/{stepNumber:int}")]
    [ProducesResponseType(typeof(List<QuestionTemplateV3ListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetQuestionsByStep(
        int stepNumber,
        [FromQuery] string? personaType,
        CancellationToken cancellationToken)
    {
        Sqordia.Domain.Enums.PersonaType? persona = null;
        if (!string.IsNullOrEmpty(personaType) &&
            Enum.TryParse<Sqordia.Domain.Enums.PersonaType>(personaType, true, out var parsed))
        {
            persona = parsed;
        }

        var result = await _service.GetQuestionsByStepAsync(stepNumber, persona, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get V3 questions by persona type
    /// </summary>
    [HttpGet("by-persona/{personaType}")]
    [ProducesResponseType(typeof(List<QuestionTemplateV3ListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetQuestionsByPersona(string personaType, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<Sqordia.Domain.Enums.PersonaType>(personaType, true, out var persona))
        {
            return BadRequest(new { message = "Invalid persona type" });
        }

        var result = await _service.GetQuestionsByPersonaAsync(persona, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a new V3 question
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateQuestion(
        [FromBody] CreateQuestionTemplateV3Request request,
        CancellationToken cancellationToken)
    {
        var result = await _service.CreateQuestionAsync(request, cancellationToken);
        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetQuestionById), new { id = result.Value }, new { Id = result.Value });
        }
        return HandleResult(result);
    }

    /// <summary>
    /// Update a V3 question
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateQuestion(
        Guid id,
        [FromBody] UpdateQuestionTemplateV3Request request,
        CancellationToken cancellationToken)
    {
        var result = await _service.UpdateQuestionAsync(id, request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Delete (deactivate) a V3 question
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteQuestion(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.DeleteQuestionAsync(id, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Activate a V3 question
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateQuestion(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.ActivateQuestionAsync(id, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Deactivate a V3 question
    /// </summary>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateQuestion(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.DeactivateQuestionAsync(id, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Update the coach prompt for a V3 question
    /// </summary>
    [HttpPut("{id:guid}/coach-prompt")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCoachPrompt(
        Guid id,
        [FromBody] UpdateCoachPromptRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.UpdateCoachPromptAsync(id, request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get AI coach suggestion for a question
    /// </summary>
    [HttpPost("{id:guid}/coach-suggestion")]
    [ProducesResponseType(typeof(CoachSuggestionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCoachSuggestion(
        Guid id,
        [FromBody] GetCoachSuggestionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetCoachSuggestionAsync(id, request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get the next available question number
    /// </summary>
    [HttpGet("next-number")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNextQuestionNumber(CancellationToken cancellationToken)
    {
        var result = await _service.GetNextQuestionNumberAsync(cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Reorder questions within a step
    /// </summary>
    [HttpPut("step/{stepNumber:int}/reorder")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ReorderQuestions(
        int stepNumber,
        [FromBody] ReorderQuestionsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.ReorderQuestionsAsync(stepNumber, request, cancellationToken);
        return HandleResult(result);
    }
}

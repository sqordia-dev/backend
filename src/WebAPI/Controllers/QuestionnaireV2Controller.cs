using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services.V2;
using Sqordia.Contracts.Requests.V2.Questionnaire;
using Sqordia.Domain.Enums;

namespace WebAPI.Controllers;

/// <summary>
/// Enhanced Questionnaire endpoints with persona support
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/questionnaire-v2")]
[Authorize]
public class QuestionnaireV2Controller : BaseApiController
{
    private readonly IQuestionnaireServiceV2 _questionnaireService;
    private readonly IQuestionPolishService _polishService;
    private readonly ILogger<QuestionnaireV2Controller> _logger;

    public QuestionnaireV2Controller(
        IQuestionnaireServiceV2 questionnaireService,
        IQuestionPolishService polishService,
        ILogger<QuestionnaireV2Controller> logger)
    {
        _questionnaireService = questionnaireService;
        _polishService = polishService;
        _logger = logger;
    }

    /// <summary>
    /// Get all questions for a persona (The Sqordia 20)
    /// </summary>
    /// <param name="persona">Optional persona type: Entrepreneur, Consultant, or OBNL</param>
    /// <param name="language">Language code: fr (default) or en</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("templates")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetQuestionsByPersona(
        [FromQuery] string? persona = null,
        [FromQuery] string language = "fr",
        CancellationToken cancellationToken = default)
    {
        PersonaType? personaType = null;
        if (!string.IsNullOrEmpty(persona) && Enum.TryParse<PersonaType>(persona, true, out var parsed))
        {
            personaType = parsed;
        }

        var result = await _questionnaireService.GetQuestionsByPersonaAsync(personaType, language, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get questions for a specific persona type
    /// </summary>
    [HttpGet("templates/{persona}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetQuestionsForPersona(
        string persona,
        [FromQuery] string language = "fr",
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<PersonaType>(persona, true, out var personaType))
        {
            return BadRequest(new { error = $"Invalid persona type: {persona}. Valid values: Entrepreneur, Consultant, OBNL" });
        }

        var result = await _questionnaireService.GetQuestionsByPersonaAsync(personaType, language, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get all step metadata (titles, descriptions, icons)
    /// </summary>
    [HttpGet("steps")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStepsMetadata(
        [FromQuery] string language = "fr",
        CancellationToken cancellationToken = default)
    {
        var result = await _questionnaireService.GetStepsMetadataAsync(language, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get questions for a specific step
    /// </summary>
    [HttpGet("steps/{stepNumber}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStepQuestions(
        int stepNumber,
        [FromQuery] string? persona = null,
        [FromQuery] string language = "fr",
        CancellationToken cancellationToken = default)
    {
        PersonaType? personaType = null;
        if (!string.IsNullOrEmpty(persona) && Enum.TryParse<PersonaType>(persona, true, out var parsed))
        {
            personaType = parsed;
        }

        var result = await _questionnaireService.GetStepQuestionsAsync(stepNumber, personaType, language, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get a single question by ID
    /// </summary>
    [HttpGet("questions/{questionId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetQuestion(
        Guid questionId,
        [FromQuery] string language = "fr",
        CancellationToken cancellationToken = default)
    {
        var result = await _questionnaireService.GetQuestionByIdAsync(questionId, language, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Polish/enhance text using AI
    /// Transforms raw notes into professional, BDC-standard prose
    /// </summary>
    [HttpPost("polish-text")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PolishText(
        [FromBody] PolishTextRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Polishing text with {Length} characters", request.Text.Length);

        var result = await _polishService.PolishTextAsync(request, cancellationToken);
        return HandleResult(result);
    }
}

using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Responses.V2.Questionnaire;
using Sqordia.Domain.Enums;

namespace Sqordia.Application.Services.V2;

/// <summary>
/// V2 Questionnaire service with persona support
/// </summary>
public interface IQuestionnaireServiceV2
{
    /// <summary>
    /// Gets all questions for a specific persona type
    /// </summary>
    Task<Result<QuestionnaireTemplateV2Response>> GetQuestionsByPersonaAsync(
        PersonaType? personaType,
        string language = "fr",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets questions for a specific step
    /// </summary>
    Task<Result<QuestionnaireStepResponse>> GetStepQuestionsAsync(
        int stepNumber,
        PersonaType? personaType = null,
        string language = "fr",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a single question by ID
    /// </summary>
    Task<Result<PersonaQuestionResponse>> GetQuestionByIdAsync(
        Guid questionId,
        string language = "fr",
        CancellationToken cancellationToken = default);
}

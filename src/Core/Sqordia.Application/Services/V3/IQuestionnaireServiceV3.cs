using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.Admin.QuestionnaireV3;
using Sqordia.Contracts.Responses.Admin.QuestionnaireV3;
using Sqordia.Domain.Enums;

namespace Sqordia.Application.Services.V3;

/// <summary>
/// Service for managing V3 questionnaire templates with coach prompts
/// </summary>
public interface IQuestionnaireServiceV3
{
    // Query
    Task<Result<List<QuestionTemplateV3ListResponse>>> GetQuestionsAsync(
        QuestionTemplateV3FilterRequest? filter = null,
        CancellationToken cancellationToken = default);

    Task<Result<QuestionTemplateV3Response>> GetQuestionByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<QuestionTemplateV3Response>> GetQuestionByNumberAsync(int questionNumber, CancellationToken cancellationToken = default);

    Task<Result<List<QuestionTemplateV3ListResponse>>> GetQuestionsByStepAsync(
        int stepNumber,
        PersonaType? personaType = null,
        CancellationToken cancellationToken = default);

    Task<Result<List<QuestionTemplateV3ListResponse>>> GetQuestionsByPersonaAsync(
        PersonaType personaType,
        CancellationToken cancellationToken = default);

    // Commands
    Task<Result<Guid>> CreateQuestionAsync(CreateQuestionTemplateV3Request request, CancellationToken cancellationToken = default);
    Task<Result> UpdateQuestionAsync(Guid id, UpdateQuestionTemplateV3Request request, CancellationToken cancellationToken = default);
    Task<Result> DeleteQuestionAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> ActivateQuestionAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> DeactivateQuestionAsync(Guid id, CancellationToken cancellationToken = default);

    // Coach Prompts
    Task<Result> UpdateCoachPromptAsync(Guid id, UpdateCoachPromptRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets AI coach suggestion for a question based on current answer
    /// </summary>
    Task<Result<CoachSuggestionResponse>> GetCoachSuggestionAsync(
        Guid questionId,
        GetCoachSuggestionRequest request,
        CancellationToken cancellationToken = default);

    // Utilities
    Task<Result<int>> GetNextQuestionNumberAsync(CancellationToken cancellationToken = default);
    Task<Result> ReorderQuestionsAsync(int stepNumber, ReorderQuestionsRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Request to reorder questions within a step
/// </summary>
public record ReorderQuestionsRequest
{
    public List<QuestionOrderItem> Items { get; init; } = new();
}

public record QuestionOrderItem
{
    public Guid Id { get; init; }
    public int DisplayOrder { get; init; }
}

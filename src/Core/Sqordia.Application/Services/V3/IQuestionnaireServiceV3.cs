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
    Task<Result<List<QuestionTemplateListResponse>>> GetQuestionsAsync(
        QuestionTemplateFilterRequest? filter = null,
        CancellationToken cancellationToken = default);

    Task<Result<QuestionTemplateResponse>> GetQuestionByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<QuestionTemplateResponse>> GetQuestionByNumberAsync(int questionNumber, CancellationToken cancellationToken = default);

    Task<Result<List<QuestionTemplateListResponse>>> GetQuestionsByStepAsync(
        int stepNumber,
        PersonaType? personaType = null,
        CancellationToken cancellationToken = default);

    Task<Result<List<QuestionTemplateListResponse>>> GetQuestionsByPersonaAsync(
        PersonaType personaType,
        CancellationToken cancellationToken = default);

    // Commands
    Task<Result<Guid>> CreateQuestionAsync(CreateQuestionTemplateRequest request, CancellationToken cancellationToken = default);
    Task<Result> UpdateQuestionAsync(Guid id, UpdateQuestionTemplateRequest request, CancellationToken cancellationToken = default);
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
    Task<Result> ReorderQuestionsAsync(int stepNumber, ReorderQuestionsV3Request request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Request to reorder questions within a step (V3)
/// </summary>
public record ReorderQuestionsV3Request
{
    public List<QuestionOrderV3Item> Items { get; init; } = new();
}

public record QuestionOrderV3Item
{
    public Guid Id { get; init; }
    public int DisplayOrder { get; init; }
}

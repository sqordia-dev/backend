using Sqordia.Contracts.Requests.Admin;
using Sqordia.Contracts.Responses.Admin;

namespace Sqordia.Application.Common.Interfaces;

public interface IAdminQuestionTemplateService
{
    /// <summary>
    /// Tests a question's coach prompt by generating AI coaching feedback for a sample answer
    /// </summary>
    /// <param name="questionId">The question template ID</param>
    /// <param name="request">The test request with answer and AI settings</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The AI-generated coaching response with metadata</returns>
    Task<TestCoachPromptResponse?> TestCoachPromptAsync(
        Guid questionId,
        TestCoachPromptRequest request,
        CancellationToken cancellationToken = default);

    Task<List<QuestionTemplateDto>> GetAllQuestionsAsync(
        int? stepNumber = null,
        string? personaType = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    Task<QuestionTemplateDto?> GetQuestionByIdAsync(
        Guid questionId,
        CancellationToken cancellationToken = default);

    Task<Guid> CreateQuestionAsync(
        CreateQuestionTemplateRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateQuestionAsync(
        Guid questionId,
        UpdateQuestionTemplateRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteQuestionAsync(
        Guid questionId,
        CancellationToken cancellationToken = default);

    Task<bool> ReorderQuestionsAsync(
        ReorderQuestionsRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> ToggleQuestionStatusAsync(
        Guid questionId,
        bool isActive,
        CancellationToken cancellationToken = default);
}

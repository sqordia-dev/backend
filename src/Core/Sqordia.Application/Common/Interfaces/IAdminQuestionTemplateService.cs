using Sqordia.Contracts.Requests.Admin;
using Sqordia.Contracts.Responses.Admin;

namespace Sqordia.Application.Common.Interfaces;

public interface IAdminQuestionTemplateService
{
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

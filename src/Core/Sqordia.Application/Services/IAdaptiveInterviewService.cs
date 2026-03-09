using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.Questionnaire;
using Sqordia.Contracts.Responses.Questionnaire;

namespace Sqordia.Application.Services;

public interface IAdaptiveInterviewService
{
    Task<Result<AdaptiveQuestionnaireResponse>> GetAdaptiveQuestionsAsync(
        Guid businessPlanId,
        string language = "fr",
        CancellationToken cancellationToken = default);

    Task<Result<bool>> SubmitAdaptiveResponseAsync(
        Guid businessPlanId,
        SubmitAdaptiveResponseRequest request,
        CancellationToken cancellationToken = default);
}

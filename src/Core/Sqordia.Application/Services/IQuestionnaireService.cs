using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.BusinessPlan;
using Sqordia.Contracts.Responses.BusinessPlan;

namespace Sqordia.Application.Services;

public interface IQuestionnaireService
{
    /// <summary>
    /// Get all questions for a business plan (based on plan type)
    /// </summary>
    Task<Result<IEnumerable<QuestionnaireQuestionResponse>>> GetQuestionnaireAsync(Guid businessPlanId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Submit or update a response to a question
    /// </summary>
    Task<Result<QuestionnaireQuestionResponse>> SubmitResponseAsync(Guid businessPlanId, SubmitQuestionnaireResponseRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all responses for a business plan
    /// </summary>
    Task<Result<IEnumerable<QuestionnaireQuestionResponse>>> GetResponsesAsync(Guid businessPlanId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Calculate and update completion percentage
    /// </summary>
    Task<Result> UpdateCompletionAsync(Guid businessPlanId, CancellationToken cancellationToken = default);
}


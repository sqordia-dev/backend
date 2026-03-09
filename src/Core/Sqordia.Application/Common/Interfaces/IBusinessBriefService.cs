using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Responses.BusinessPlan;

namespace Sqordia.Application.Common.Interfaces;

/// <summary>
/// Service for generating and managing Business Briefs — unified context documents
/// that provide a holistic understanding of the business for AI generation.
/// </summary>
public interface IBusinessBriefService
{
    /// <summary>
    /// Generates a Business Brief from all questionnaire answers and onboarding context.
    /// The brief is stored on the BusinessPlan entity for use during section generation.
    /// </summary>
    Task<Result<BusinessBriefDto>> GenerateBusinessBriefAsync(
        Guid businessPlanId,
        string language = "fr",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the stored Business Brief for a business plan.
    /// Returns null if no brief has been generated yet.
    /// </summary>
    Task<Result<BusinessBriefDto>> GetBusinessBriefAsync(
        Guid businessPlanId,
        CancellationToken cancellationToken = default);
}

using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.AI;
using Sqordia.Contracts.Responses.AI;

namespace Sqordia.Application.Services;

/// <summary>
/// Service for AI-powered business plan analysis features
/// </summary>
public interface IAIAnalysisService
{
    /// <summary>
    /// Generates strategy suggestions for a business plan
    /// </summary>
    Task<Result<StrategySuggestionResponse>> GenerateStrategySuggestionsAsync(
        StrategySuggestionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs risk mitigation analysis for a business plan
    /// </summary>
    Task<Result<RiskMitigationResponse>> AnalyzeRisksAsync(
        RiskMitigationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs comprehensive business mentor analysis
    /// </summary>
    Task<Result<BusinessMentorResponse>> PerformBusinessMentorAnalysisAsync(
        BusinessMentorRequest request,
        CancellationToken cancellationToken = default);
}


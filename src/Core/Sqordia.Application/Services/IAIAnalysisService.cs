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
    Task<StrategySuggestionResponse> GenerateStrategySuggestionsAsync(
        StrategySuggestionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs risk mitigation analysis for a business plan
    /// </summary>
    Task<RiskMitigationResponse> AnalyzeRisksAsync(
        RiskMitigationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs comprehensive business mentor analysis
    /// </summary>
    Task<BusinessMentorResponse> PerformBusinessMentorAnalysisAsync(
        BusinessMentorRequest request,
        CancellationToken cancellationToken = default);
}


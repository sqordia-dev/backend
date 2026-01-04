using Sqordia.Contracts.Requests.Pricing;
using Sqordia.Contracts.Responses.Pricing;

namespace Sqordia.Application.Services;

/// <summary>
/// Service for pricing and market analysis
/// </summary>
public interface IPricingAnalysisService
{
    /// <summary>
    /// Generate pricing analysis and competitive report
    /// </summary>
    Task<PricingAnalysisResponse> AnalyzePricingAsync(
        PricingAnalysisRequest request,
        CancellationToken cancellationToken = default);
}


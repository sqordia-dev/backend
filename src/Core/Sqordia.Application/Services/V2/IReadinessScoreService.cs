using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Responses.V2.Readiness;

namespace Sqordia.Application.Services.V2;

/// <summary>
/// Bank-ready percentage calculation service
/// Weights: 50% Consistency, 30% Risk Mitigation, 20% Completeness
/// </summary>
public interface IReadinessScoreService
{
    /// <summary>
    /// Calculates the overall readiness score for a business plan
    /// </summary>
    Task<Result<ReadinessScoreResponse>> CalculateReadinessScoreAsync(
        Guid businessPlanId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets detailed breakdown of readiness components by section
    /// </summary>
    Task<Result<ReadinessBreakdownResponse>> GetReadinessBreakdownAsync(
        Guid businessPlanId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Recalculates and persists the readiness score
    /// </summary>
    Task<Result<decimal>> RecalculateAndSaveAsync(
        Guid businessPlanId,
        CancellationToken cancellationToken = default);
}

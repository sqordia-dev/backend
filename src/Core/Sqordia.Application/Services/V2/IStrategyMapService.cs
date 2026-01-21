using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.V2.StrategyMap;
using Sqordia.Contracts.Responses.V2.StrategyMap;

namespace Sqordia.Application.Services.V2;

/// <summary>
/// Strategy map management service
/// Handles saving/retrieving strategy maps and triggering recalculations
/// </summary>
public interface IStrategyMapService
{
    /// <summary>
    /// Saves strategy map and optionally triggers recalculation of financial projections
    /// </summary>
    Task<Result<StrategyMapResponse>> SaveStrategyMapAsync(
        Guid businessPlanId,
        SaveStrategyMapRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current strategy map for a business plan
    /// </summary>
    Task<Result<StrategyMapResponse>> GetStrategyMapAsync(
        Guid businessPlanId,
        CancellationToken cancellationToken = default);
}

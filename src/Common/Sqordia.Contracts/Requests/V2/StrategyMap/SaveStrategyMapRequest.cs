using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.V2.StrategyMap;

/// <summary>
/// Request to save a strategy map for a business plan
/// </summary>
public class SaveStrategyMapRequest
{
    /// <summary>
    /// The React Flow node/edge data as JSON string
    /// </summary>
    [Required]
    public required string StrategyMapJson { get; set; }

    /// <summary>
    /// Whether to trigger a recalculation of financial projections
    /// </summary>
    public bool TriggerRecalculation { get; set; } = true;
}

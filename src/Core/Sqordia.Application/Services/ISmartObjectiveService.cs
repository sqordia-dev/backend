using Sqordia.Contracts.Requests.SmartObjective;
using Sqordia.Contracts.Responses.SmartObjective;

namespace Sqordia.Application.Services;

/// <summary>
/// Service for SMART objectives management
/// </summary>
public interface ISmartObjectiveService
{
    /// <summary>
    /// Generate SMART objectives for a business plan using AI
    /// </summary>
    Task<GenerateSmartObjectivesResponse> GenerateSmartObjectivesAsync(
        GenerateSmartObjectivesRequest request,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all SMART objectives for a business plan
    /// </summary>
    Task<List<SmartObjectiveResponse>> GetObjectivesAsync(
        Guid businessPlanId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Update objective progress
    /// </summary>
    Task<SmartObjectiveResponse> UpdateProgressAsync(
        Guid objectiveId,
        decimal progressPercentage,
        CancellationToken cancellationToken = default);
}


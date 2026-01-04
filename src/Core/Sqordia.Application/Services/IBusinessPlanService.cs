using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.BusinessPlan;
using Sqordia.Contracts.Responses.BusinessPlan;

namespace Sqordia.Application.Services;

public interface IBusinessPlanService
{
    /// <summary>
    /// Create a new business plan
    /// </summary>
    Task<Result<BusinessPlanResponse>> CreateBusinessPlanAsync(CreateBusinessPlanRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get a business plan by ID
    /// </summary>
    Task<Result<BusinessPlanResponse>> GetBusinessPlanAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all business plans for the current user
    /// </summary>
    Task<Result<IEnumerable<BusinessPlanResponse>>> GetUserBusinessPlansAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all business plans for an organization
    /// </summary>
    Task<Result<IEnumerable<BusinessPlanResponse>>> GetOrganizationBusinessPlansAsync(Guid organizationId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Update business plan metadata (title, description)
    /// </summary>
    Task<Result<BusinessPlanResponse>> UpdateBusinessPlanAsync(Guid id, UpdateBusinessPlanRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Delete a business plan
    /// </summary>
    Task<Result> DeleteBusinessPlanAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Archive a business plan
    /// </summary>
    Task<Result<BusinessPlanResponse>> ArchiveBusinessPlanAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Unarchive a business plan
    /// </summary>
    Task<Result<BusinessPlanResponse>> UnarchiveBusinessPlanAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Duplicate a business plan
    /// </summary>
    Task<Result<BusinessPlanResponse>> DuplicateBusinessPlanAsync(Guid id, string? newTitle = null, CancellationToken cancellationToken = default);
}


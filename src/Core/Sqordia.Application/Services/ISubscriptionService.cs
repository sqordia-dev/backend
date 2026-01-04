using Sqordia.Application.Common.Models;
using Sqordia.Application.Contracts.Requests;
using Sqordia.Application.Contracts.Responses;

namespace Sqordia.Application.Services;

/// <summary>
/// Subscription service interface
/// </summary>
public interface ISubscriptionService
{
    /// <summary>
    /// Get all available subscription plans
    /// </summary>
    Task<Result<List<SubscriptionPlanDto>>> GetPlansAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get current user's active subscription
    /// </summary>
    Task<Result<SubscriptionDto>> GetCurrentSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get organization's active subscription
    /// </summary>
    Task<Result<SubscriptionDto>> GetOrganizationSubscriptionAsync(Guid organizationId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Subscribe to a plan
    /// </summary>
    Task<Result<SubscriptionDto>> SubscribeAsync(Guid userId, SubscribeRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Change subscription plan
    /// </summary>
    Task<Result<SubscriptionDto>> ChangePlanAsync(Guid userId, ChangePlanRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Cancel subscription
    /// </summary>
    Task<Result<bool>> CancelSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get user's invoices
    /// </summary>
    Task<Result<List<InvoiceDto>>> GetInvoicesAsync(Guid userId, CancellationToken cancellationToken = default);
}


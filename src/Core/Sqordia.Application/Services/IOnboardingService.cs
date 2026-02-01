using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.Onboarding;
using Sqordia.Contracts.Responses.Onboarding;

namespace Sqordia.Application.Services;

/// <summary>
/// Service for managing user onboarding progress
/// </summary>
public interface IOnboardingService
{
    /// <summary>
    /// Get the current user's onboarding progress
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Onboarding progress information</returns>
    Task<Result<OnboardingProgressDto>> GetProgressAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Save onboarding step progress
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="request">The progress update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated onboarding progress</returns>
    Task<Result<OnboardingProgressDto>> SaveProgressAsync(
        Guid userId,
        OnboardingProgressRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark onboarding as complete and optionally create an initial business plan
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="request">The completion request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Completion result with optional business plan ID</returns>
    Task<Result<OnboardingCompleteResponse>> CompleteAsync(
        Guid userId,
        OnboardingCompleteRequest request,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Response for onboarding completion
/// </summary>
public class OnboardingCompleteResponse
{
    /// <summary>
    /// Whether onboarding was successfully completed
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The created business plan ID (if CreateInitialPlan was true)
    /// </summary>
    public Guid? BusinessPlanId { get; set; }

    /// <summary>
    /// Message about the completion
    /// </summary>
    public string? Message { get; set; }
}

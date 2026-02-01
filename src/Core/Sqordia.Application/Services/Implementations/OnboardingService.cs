using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.Onboarding;
using Sqordia.Contracts.Responses.Onboarding;
using Sqordia.Domain.Enums;

namespace Sqordia.Application.Services.Implementations;

/// <summary>
/// Service for managing user onboarding progress
/// </summary>
public class OnboardingService : IOnboardingService
{
    private readonly IApplicationDbContext _context;
    private readonly IBusinessPlanService _businessPlanService;
    private readonly ILogger<OnboardingService> _logger;

    private const int TotalOnboardingSteps = 5;

    public OnboardingService(
        IApplicationDbContext context,
        IBusinessPlanService businessPlanService,
        ILogger<OnboardingService> logger)
    {
        _context = context;
        _businessPlanService = businessPlanService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<OnboardingProgressDto>> GetProgressAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null)
            {
                return Result.Failure<OnboardingProgressDto>(Error.NotFound(
                    "Onboarding.Error.UserNotFound",
                    "User not found"));
            }

            var progress = new OnboardingProgressDto
            {
                UserId = user.Id,
                IsCompleted = user.OnboardingCompleted,
                CurrentStep = user.OnboardingStep,
                TotalSteps = TotalOnboardingSteps,
                CompletionPercentage = CalculateCompletionPercentage(user.OnboardingCompleted, user.OnboardingStep),
                Persona = user.Persona?.ToString(),
                Data = user.OnboardingData,
                LastUpdated = user.LastModified
            };

            return Result.Success(progress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get onboarding progress for user {UserId}", userId);
            return Result.Failure<OnboardingProgressDto>(Error.InternalServerError(
                "Onboarding.Error.GetProgressFailed",
                "Failed to get onboarding progress"));
        }
    }

    /// <inheritdoc />
    public async Task<Result<OnboardingProgressDto>> SaveProgressAsync(
        Guid userId,
        OnboardingProgressRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null)
            {
                return Result.Failure<OnboardingProgressDto>(Error.NotFound(
                    "Onboarding.Error.UserNotFound",
                    "User not found"));
            }

            // Update onboarding progress
            user.UpdateOnboardingProgress(request.Step, request.StepData);

            // Update persona if provided
            if (!string.IsNullOrWhiteSpace(request.Persona) &&
                Enum.TryParse<PersonaType>(request.Persona, true, out var persona))
            {
                user.SetPersona(persona);
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated onboarding progress for user {UserId} to step {Step}", userId, request.Step);

            var progress = new OnboardingProgressDto
            {
                UserId = user.Id,
                IsCompleted = user.OnboardingCompleted,
                CurrentStep = user.OnboardingStep,
                TotalSteps = TotalOnboardingSteps,
                CompletionPercentage = CalculateCompletionPercentage(user.OnboardingCompleted, user.OnboardingStep),
                Persona = user.Persona?.ToString(),
                Data = user.OnboardingData,
                LastUpdated = DateTime.UtcNow
            };

            return Result.Success(progress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save onboarding progress for user {UserId}", userId);
            return Result.Failure<OnboardingProgressDto>(Error.InternalServerError(
                "Onboarding.Error.SaveProgressFailed",
                "Failed to save onboarding progress"));
        }
    }

    /// <inheritdoc />
    public async Task<Result<OnboardingCompleteResponse>> CompleteAsync(
        Guid userId,
        OnboardingCompleteRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null)
            {
                return Result.Failure<OnboardingCompleteResponse>(Error.NotFound(
                    "Onboarding.Error.UserNotFound",
                    "User not found"));
            }

            // Save final data if provided
            if (!string.IsNullOrWhiteSpace(request.FinalData))
            {
                user.UpdateOnboardingProgress(TotalOnboardingSteps, request.FinalData);
            }

            // Mark onboarding as complete
            user.CompleteOnboarding();
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Completed onboarding for user {UserId}", userId);

            Guid? businessPlanId = null;

            // Create initial business plan if requested
            if (request.CreateInitialPlan && !string.IsNullOrWhiteSpace(request.PlanName))
            {
                try
                {
                    // Note: This would call BusinessPlanService.CreateAsync
                    // For now, we'll log the intent
                    _logger.LogInformation(
                        "Would create initial business plan for user {UserId}: {PlanName}",
                        userId,
                        request.PlanName);

                    // TODO: Implement when BusinessPlanService.CreateAsync is available
                    // var createResult = await _businessPlanService.CreateAsync(new CreateBusinessPlanRequest {...});
                    // if (createResult.IsSuccess)
                    //     businessPlanId = createResult.Value.Id;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create initial business plan for user {UserId}", userId);
                    // Don't fail the onboarding completion, just log the error
                }
            }

            return Result.Success(new OnboardingCompleteResponse
            {
                Success = true,
                BusinessPlanId = businessPlanId,
                Message = "Onboarding completed successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to complete onboarding for user {UserId}", userId);
            return Result.Failure<OnboardingCompleteResponse>(Error.InternalServerError(
                "Onboarding.Error.CompleteFailed",
                "Failed to complete onboarding"));
        }
    }

    private static double CalculateCompletionPercentage(bool isCompleted, int? currentStep)
    {
        if (isCompleted)
            return 100.0;

        if (!currentStep.HasValue || currentStep < 0)
            return 0.0;

        return Math.Min(100.0, (double)(currentStep.Value + 1) / TotalOnboardingSteps * 100);
    }
}

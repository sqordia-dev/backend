using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services;
using Sqordia.Contracts.Requests.Onboarding;
using Sqordia.Contracts.Responses.Onboarding;

namespace WebAPI.Controllers;

/// <summary>
/// Controller for managing user onboarding flow
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/onboarding")]
[Authorize]
public class OnboardingController : BaseApiController
{
    private readonly IOnboardingService _onboardingService;
    private readonly ILogger<OnboardingController> _logger;

    public OnboardingController(
        IOnboardingService onboardingService,
        ILogger<OnboardingController> logger)
    {
        _onboardingService = onboardingService;
        _logger = logger;
    }

    /// <summary>
    /// Get the current user's onboarding progress
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Onboarding progress information</returns>
    /// <remarks>
    /// Returns the current onboarding status and progress for the authenticated user.
    ///
    /// Sample response:
    ///     {
    ///         "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///         "isCompleted": false,
    ///         "currentStep": 2,
    ///         "totalSteps": 5,
    ///         "completionPercentage": 60.0,
    ///         "persona": "Entrepreneur",
    ///         "data": "{\"companyName\":\"My Startup\"}",
    ///         "lastUpdated": "2025-01-23T10:30:00Z"
    ///     }
    /// </remarks>
    /// <response code="200">Onboarding progress retrieved successfully</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">User not found</response>
    [HttpGet("progress")]
    [ProducesResponseType(typeof(OnboardingProgressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProgress(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        _logger.LogInformation("Getting onboarding progress for user {UserId}", userId);

        var result = await _onboardingService.GetProgressAsync(userId.Value, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Save onboarding step progress
    /// </summary>
    /// <param name="request">The progress update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated onboarding progress</returns>
    /// <remarks>
    /// Saves the current onboarding step and associated data for the authenticated user.
    ///
    /// Sample request:
    ///     POST /api/v1/onboarding/progress
    ///     {
    ///         "step": 2,
    ///         "stepData": "{\"companyName\":\"My Startup\",\"industry\":\"Technology\"}",
    ///         "persona": "Entrepreneur"
    ///     }
    ///
    /// Sample response:
    ///     {
    ///         "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///         "isCompleted": false,
    ///         "currentStep": 2,
    ///         "totalSteps": 5,
    ///         "completionPercentage": 60.0,
    ///         "persona": "Entrepreneur",
    ///         "lastUpdated": "2025-01-23T10:35:00Z"
    ///     }
    /// </remarks>
    /// <response code="200">Progress saved successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">User not found</response>
    [HttpPost("progress")]
    [ProducesResponseType(typeof(OnboardingProgressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SaveProgress(
        [FromBody] OnboardingProgressRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        _logger.LogInformation("Saving onboarding progress for user {UserId}, step {Step}", userId, request.Step);

        var result = await _onboardingService.SaveProgressAsync(userId.Value, request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Mark onboarding as complete and optionally create an initial business plan
    /// </summary>
    /// <param name="request">The completion request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Completion result with optional business plan ID</returns>
    /// <remarks>
    /// Marks the user's onboarding as complete. Optionally creates an initial business plan
    /// based on the information collected during onboarding.
    ///
    /// Sample request:
    ///     POST /api/v1/onboarding/complete
    ///     {
    ///         "createInitialPlan": true,
    ///         "planName": "My First Business Plan",
    ///         "industry": "Technology",
    ///         "templateId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///         "finalData": "{\"companyName\":\"My Startup\",\"industry\":\"Technology\",\"persona\":\"Entrepreneur\"}"
    ///     }
    ///
    /// Sample response:
    ///     {
    ///         "success": true,
    ///         "businessPlanId": "7a12b3c4-5678-90ab-cdef-123456789012",
    ///         "message": "Onboarding completed successfully"
    ///     }
    /// </remarks>
    /// <response code="200">Onboarding completed successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">User not found</response>
    [HttpPost("complete")]
    [ProducesResponseType(typeof(OnboardingCompleteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Complete(
        [FromBody] OnboardingCompleteRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        _logger.LogInformation("Completing onboarding for user {UserId}, createInitialPlan: {CreatePlan}",
            userId, request.CreateInitialPlan);

        var result = await _onboardingService.CompleteAsync(userId.Value, request, cancellationToken);
        return HandleResult(result);
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Contracts.Requests.AICoach;
using Sqordia.Contracts.Responses.AICoach;

namespace WebAPI.Controllers;

/// <summary>
/// AI Coach controller for multi-turn conversational coaching
/// </summary>
[Authorize]
[Route("api/v1/ai-coach")]
public class AICoachController : BaseApiController
{
    private readonly IAICoachService _aiCoachService;

    public AICoachController(IAICoachService aiCoachService)
    {
        _aiCoachService = aiCoachService;
    }

    /// <summary>
    /// Start a new AI Coach conversation for a specific question
    /// </summary>
    /// <param name="request">The conversation start request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The conversation with the initial AI response</returns>
    [HttpPost("conversations")]
    [ProducesResponseType(typeof(AICoachConversationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> StartConversation(
        [FromBody] StartCoachConversationRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var result = await _aiCoachService.StartConversationAsync(userId.Value, request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Send a message to an existing AI Coach conversation
    /// </summary>
    /// <param name="id">The conversation ID</param>
    /// <param name="request">The message request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The AI's response message</returns>
    [HttpPost("conversations/{id:guid}/messages")]
    [ProducesResponseType(typeof(AICoachMessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendMessage(
        [FromRoute] Guid id,
        [FromBody] SendCoachMessageRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        // Ensure the request ConversationId matches the route
        request.ConversationId = id;

        var result = await _aiCoachService.SendMessageAsync(userId.Value, request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get an AI Coach conversation by ID
    /// </summary>
    /// <param name="id">The conversation ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The conversation with all messages</returns>
    [HttpGet("conversations/{id:guid}")]
    [ProducesResponseType(typeof(AICoachConversationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetConversation(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var result = await _aiCoachService.GetConversationAsync(userId.Value, id, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get an existing AI Coach conversation by question
    /// </summary>
    /// <param name="businessPlanId">The business plan ID</param>
    /// <param name="questionId">The question ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The conversation if it exists, or null</returns>
    [HttpGet("conversations/by-question")]
    [ProducesResponseType(typeof(AICoachConversationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetConversationByQuestion(
        [FromQuery] Guid businessPlanId,
        [FromQuery] string questionId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var result = await _aiCoachService.GetConversationByQuestionAsync(
            userId.Value, businessPlanId, questionId, cancellationToken);

        if (result.IsFailure)
            return HandleResult(result);

        // Return null as 200 OK with null body (conversation doesn't exist yet)
        return Ok(result.Value);
    }

    /// <summary>
    /// Get the current user's AI Coach token usage
    /// </summary>
    /// <param name="organizationId">Optional organization ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Token usage information</returns>
    [HttpGet("usage")]
    [ProducesResponseType(typeof(AICoachTokenUsageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTokenUsage(
        [FromQuery] Guid? organizationId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var result = await _aiCoachService.GetTokenUsageAsync(userId.Value, organizationId, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Check if the current user has access to AI Coach
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Access information</returns>
    [HttpGet("access")]
    [ProducesResponseType(typeof(AICoachAccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CheckAccess(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var result = await _aiCoachService.CheckAccessAsync(userId.Value, cancellationToken);
        return HandleResult(result);
    }
}

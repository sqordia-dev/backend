using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.AICoach;
using Sqordia.Contracts.Responses.AICoach;

namespace Sqordia.Application.Common.Interfaces;

/// <summary>
/// Interface for AI Coach conversational service
/// </summary>
public interface IAICoachService
{
    /// <summary>
    /// Starts a new AI Coach conversation for a specific question
    /// </summary>
    /// <param name="userId">The user's ID</param>
    /// <param name="request">The conversation start request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The conversation with the initial AI response</returns>
    Task<Result<AICoachConversationResponse>> StartConversationAsync(
        Guid userId,
        StartCoachConversationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a message to an existing AI Coach conversation
    /// </summary>
    /// <param name="userId">The user's ID</param>
    /// <param name="request">The message request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The AI's response message</returns>
    Task<Result<AICoachMessageResponse>> SendMessageAsync(
        Guid userId,
        SendCoachMessageRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a conversation by ID
    /// </summary>
    /// <param name="userId">The user's ID</param>
    /// <param name="conversationId">The conversation ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The conversation with all messages</returns>
    Task<Result<AICoachConversationResponse>> GetConversationAsync(
        Guid userId,
        Guid conversationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an existing conversation for a specific question
    /// </summary>
    /// <param name="userId">The user's ID</param>
    /// <param name="businessPlanId">The business plan ID</param>
    /// <param name="questionId">The question ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The conversation if it exists, or null</returns>
    Task<Result<AICoachConversationResponse?>> GetConversationByQuestionAsync(
        Guid userId,
        Guid businessPlanId,
        string questionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the user's current AI Coach token usage
    /// </summary>
    /// <param name="userId">The user's ID</param>
    /// <param name="organizationId">Optional organization ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Token usage information</returns>
    Task<Result<AICoachTokenUsageResponse>> GetTokenUsageAsync(
        Guid userId,
        Guid? organizationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the user has access to AI Coach feature
    /// </summary>
    /// <param name="userId">The user's ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Access information</returns>
    Task<Result<AICoachAccessResponse>> CheckAccessAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}

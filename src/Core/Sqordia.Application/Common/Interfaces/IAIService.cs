using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.Questionnaire;
using Sqordia.Contracts.Responses.Questionnaire;
using Sqordia.Contracts.Requests.Sections;
using Sqordia.Contracts.Responses.Sections;
using Sqordia.Contracts.Requests.AI;
using Sqordia.Contracts.Responses.AI;

namespace Sqordia.Application.Common.Interfaces;

/// <summary>
/// Interface for AI-powered content generation
/// </summary>
public interface IAIService
{
    /// <summary>
    /// Generates content based on a prompt
    /// </summary>
    /// <param name="systemPrompt">The system instructions for the AI</param>
    /// <param name="userPrompt">The user's request or context</param>
    /// <param name="maxTokens">Maximum number of tokens to generate</param>
    /// <param name="temperature">Controls randomness (0.0 = deterministic, 1.0 = creative)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Generated content</returns>
    Task<string> GenerateContentAsync(
        string systemPrompt,
        string userPrompt,
        int maxTokens = 2000,
        float temperature = 0.7f,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates content with retry logic for better reliability
    /// </summary>
    Task<string> GenerateContentWithRetryAsync(
        string systemPrompt,
        string userPrompt,
        int maxTokens = 2000,
        float temperature = 0.7f,
        int maxRetries = 3,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates content with retry logic and returns metadata (tokens, latency, model).
    /// Use this for observable AI calls in the generation pipeline.
    /// </summary>
    Task<AICallResult> GenerateContentWithMetadataAsync(
        string systemPrompt,
        string userPrompt,
        int maxTokens = 2000,
        float temperature = 0.7f,
        int maxRetries = 3,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that the AI service is configured and accessible
    /// </summary>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates AI suggestions for questionnaire questions
    /// </summary>
    /// <param name="request">The question suggestion request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AI-generated suggestions for the question</returns>
    Task<QuestionSuggestionResponse> GenerateQuestionSuggestionsAsync(
        QuestionSuggestionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Improves a business plan section using AI
    /// </summary>
    /// <param name="request">The section improvement request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AI-improved section content</returns>
    Task<SectionImprovementResponse> ImproveSectionAsync(
        SectionImprovementRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Expands a business plan section using AI
    /// </summary>
    /// <param name="request">The section improvement request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AI-expanded section content</returns>
    Task<SectionExpansionResponse> ExpandSectionAsync(
        SectionImprovementRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Simplifies a business plan section using AI
    /// </summary>
    /// <param name="request">The section improvement request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AI-simplified section content</returns>
    Task<SectionSimplificationResponse> SimplifySectionAsync(
        SectionImprovementRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates strategy suggestions for a business plan
    /// </summary>
    /// <param name="request">The strategy suggestion request</param>
    /// <param name="businessPlanContext">Context from the business plan (questionnaire responses, sections, etc.)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AI-generated strategy suggestions</returns>
    Task<StrategySuggestionResponse> GenerateStrategySuggestionsAsync(
        StrategySuggestionRequest request,
        string businessPlanContext,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs risk mitigation analysis for a business plan
    /// </summary>
    /// <param name="request">The risk mitigation request</param>
    /// <param name="businessPlanContext">Context from the business plan</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AI-generated risk analysis with mitigation strategies</returns>
    Task<RiskMitigationResponse> AnalyzeRisksAsync(
        RiskMitigationRequest request,
        string businessPlanContext,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs comprehensive business mentor analysis
    /// </summary>
    /// <param name="request">The business mentor request</param>
    /// <param name="businessPlanContext">Context from the business plan</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Comprehensive business analysis with opportunities and weaknesses</returns>
    Task<BusinessMentorResponse> PerformBusinessMentorAnalysisAsync(
        BusinessMentorRequest request,
        string businessPlanContext,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a chat response for multi-turn conversations
    /// </summary>
    /// <param name="systemPrompt">The system instructions for the AI</param>
    /// <param name="conversationHistory">List of previous messages in the conversation</param>
    /// <param name="maxTokens">Maximum number of tokens to generate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A tuple containing the response content and token count</returns>
    Task<(string Content, int TokenCount)> GenerateChatResponseAsync(
        string systemPrompt,
        List<AIChatMessage> conversationHistory,
        int maxTokens = 2000,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams a chat response for real-time display (SSE)
    /// </summary>
    /// <param name="systemPrompt">The system instructions for the AI</param>
    /// <param name="conversationHistory">List of previous messages in the conversation</param>
    /// <param name="maxTokens">Maximum number of tokens to generate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Async enumerable of text chunks</returns>
    IAsyncEnumerable<string> StreamChatResponseAsync(
        string systemPrompt,
        List<AIChatMessage> conversationHistory,
        int maxTokens = 2000,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a message in an AI chat conversation
/// </summary>
public class AIChatMessage
{
    /// <summary>
    /// The role of the message sender: "user" or "assistant"
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// The message content
    /// </summary>
    public string Content { get; set; } = string.Empty;
}


using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Services;
using Sqordia.Contracts.Requests.AICoach;
using Sqordia.Contracts.Responses.AICoach;
using Sqordia.Domain.Entities.AICoach;
using Sqordia.Domain.Enums;
using Sqordia.Domain.Constants;
using System.Text;
using System.Text.Json;

namespace Sqordia.Infrastructure.Services;

public class AICoachService : IAICoachService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IAIService _aiService;
    private readonly ISettingsService _settingsService;
    private readonly ISubscriptionService _subscriptionService;
    private readonly IFeatureGateService _featureGate;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AICoachService> _logger;

    // Default config values (can be overridden by feature flag)
    private const string FeatureFlagKey = "Features:AICoach";
    private const int DefaultMaxMonthlyTokensPro = 100000;
    private const int DefaultMaxMonthlyTokensEnterprise = 500000;
    private const int DefaultWarningThresholdPercent = 80;
    private const int DefaultMaxConversationTokens = 8000;

    public AICoachService(
        IApplicationDbContext dbContext,
        IAIService aiService,
        ISettingsService settingsService,
        ISubscriptionService subscriptionService,
        IFeatureGateService featureGate,
        IServiceScopeFactory scopeFactory,
        ILogger<AICoachService> logger)
    {
        _dbContext = dbContext;
        _aiService = aiService;
        _settingsService = settingsService;
        _subscriptionService = subscriptionService;
        _featureGate = featureGate;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task<Result<AICoachConversationResponse>> StartConversationAsync(
        Guid userId,
        StartCoachConversationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check access
            var accessResult = await CheckAccessAsync(userId, cancellationToken);
            if (accessResult.IsFailure)
                return Result.Failure<AICoachConversationResponse>(accessResult.Error!);

            if (!accessResult.Value!.HasAccess)
                return Result.Failure<AICoachConversationResponse>(
                    new Error("AICoach.AccessDenied", accessResult.Value.DenialReason ?? "Access denied"));

            // Check monthly message limit via feature gate
            var orgId = await _dbContext.BusinessPlans
                .Where(bp => bp.Id == request.BusinessPlanId && !bp.IsDeleted)
                .Select(bp => bp.OrganizationId)
                .FirstOrDefaultAsync(cancellationToken);

            if (orgId != Guid.Empty)
            {
                var usageCheck = await _featureGate.CheckUsageLimitAsync(
                    orgId, PlanFeatures.MaxAiCoachMessagesMonthly, cancellationToken);
                if (usageCheck.IsSuccess && !usageCheck.Value!.Allowed)
                {
                    return Result.Failure<AICoachConversationResponse>(
                        Error.Failure("AICoach.MessageLimitReached",
                            usageCheck.Value.DenialReason ?? "Monthly AI Coach message limit reached. Upgrade your plan for more."));
                }
            }

            // Check if conversation already exists for this question
            var existingConversation = await _dbContext.AICoachConversations
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c =>
                    c.UserId == userId &&
                    c.BusinessPlanId == request.BusinessPlanId &&
                    c.QuestionId == request.QuestionId &&
                    c.IsActive,
                    cancellationToken);

            if (existingConversation != null)
            {
                // Add new message to existing conversation
                var sendRequest = new SendCoachMessageRequest
                {
                    ConversationId = existingConversation.Id,
                    Message = request.InitialMessage
                };
                var messageResult = await SendMessageAsync(userId, sendRequest, cancellationToken);
                if (messageResult.IsFailure)
                    return Result.Failure<AICoachConversationResponse>(messageResult.Error!);

                // Return the updated conversation
                return await GetConversationAsync(userId, existingConversation.Id, cancellationToken);
            }

            // Build questionnaire context
            var questionnaireContext = await BuildQuestionnaireContextAsync(request.BusinessPlanId, cancellationToken);

            // Create system prompt
            var systemPrompt = BuildSystemPrompt(request, questionnaireContext);

            // Create conversation
            var conversation = new AICoachConversation(
                userId,
                request.BusinessPlanId,
                request.QuestionId,
                request.QuestionNumber,
                request.QuestionText,
                request.Language,
                request.Persona);

            // Add user message (tokenCount = 0 for user messages)
            var userMessage = AICoachMessage.CreateUserMessage(
                conversation.Id,
                request.InitialMessage,
                0, // User messages don't count towards token usage
                conversation.Messages.Count + 1);
            conversation.AddMessage(userMessage);

            // Generate AI response
            var conversationHistory = new List<AIChatMessage>
            {
                new AIChatMessage { Role = "user", Content = request.InitialMessage }
            };

            var config = await GetAICoachConfigAsync(cancellationToken);
            var (aiResponse, tokenCount) = await _aiService.GenerateChatResponseAsync(
                systemPrompt,
                conversationHistory,
                config.MaxConversationTokens,
                cancellationToken);

            // Add assistant message
            var assistantMessage = AICoachMessage.CreateAssistantMessage(
                conversation.Id,
                aiResponse,
                tokenCount,
                conversation.Messages.Count + 1);
            conversation.AddMessage(assistantMessage);

            // Update token usage
            await UpdateTokenUsageAsync(userId, null, tokenCount, cancellationToken);

            // Save to database
            _dbContext.AICoachConversations.Add(conversation);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Record feature gate usage
            if (orgId != Guid.Empty)
                await _featureGate.RecordUsageAsync(orgId, PlanFeatures.MaxAiCoachMessagesMonthly, 1, cancellationToken);

            _logger.LogInformation(
                "Started AI Coach conversation {ConversationId} for user {UserId}, question {QuestionId}",
                conversation.Id, userId, request.QuestionId);

            return Result.Success(MapToResponse(conversation));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting AI Coach conversation for user {UserId}", userId);
            return Result.Failure<AICoachConversationResponse>(
                new Error("AICoach.Error", "An error occurred while starting the conversation"));
        }
    }

    public async Task<Result<AICoachMessageResponse>> SendMessageAsync(
        Guid userId,
        SendCoachMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check access
            var accessResult = await CheckAccessAsync(userId, cancellationToken);
            if (accessResult.IsFailure)
                return Result.Failure<AICoachMessageResponse>(accessResult.Error!);

            if (!accessResult.Value!.HasAccess)
                return Result.Failure<AICoachMessageResponse>(
                    new Error("AICoach.AccessDenied", accessResult.Value.DenialReason ?? "Access denied"));

            // Get conversation (load early so we can resolve orgId for feature gate)
            var conversation = await _dbContext.AICoachConversations
                .Include(c => c.Messages.OrderBy(m => m.Sequence))
                .FirstOrDefaultAsync(c =>
                    c.Id == request.ConversationId &&
                    c.UserId == userId &&
                    c.IsActive,
                    cancellationToken);

            if (conversation == null)
                return Result.Failure<AICoachMessageResponse>(
                    new Error("AICoach.ConversationNotFound", "Conversation not found"));

            // Check monthly message limit via feature gate
            var sendOrgId = await _dbContext.BusinessPlans
                .Where(bp => bp.Id == conversation.BusinessPlanId && !bp.IsDeleted)
                .Select(bp => bp.OrganizationId)
                .FirstOrDefaultAsync(cancellationToken);

            if (sendOrgId != Guid.Empty)
            {
                var msgCheck = await _featureGate.CheckUsageLimitAsync(
                    sendOrgId, PlanFeatures.MaxAiCoachMessagesMonthly, cancellationToken);
                if (msgCheck.IsSuccess && !msgCheck.Value!.Allowed)
                {
                    return Result.Failure<AICoachMessageResponse>(
                        Error.Failure("AICoach.MessageLimitReached",
                            msgCheck.Value.DenialReason ?? "Monthly AI Coach message limit reached. Upgrade your plan for more."));
                }
            }

            // Build questionnaire context
            var questionnaireContext = await BuildQuestionnaireContextAsync(conversation.BusinessPlanId, cancellationToken);

            // Rebuild system prompt
            var systemPrompt = BuildSystemPrompt(
                new StartCoachConversationRequest
                {
                    BusinessPlanId = conversation.BusinessPlanId,
                    QuestionId = conversation.QuestionId,
                    QuestionNumber = conversation.QuestionNumber,
                    QuestionText = conversation.QuestionText,
                    Language = conversation.Language,
                    Persona = conversation.Persona,
                    InitialMessage = ""
                },
                questionnaireContext);

            // Add user message (tokenCount = 0 for user messages)
            var userMessage = AICoachMessage.CreateUserMessage(
                conversation.Id,
                request.Message,
                0, // User messages don't count towards token usage
                conversation.Messages.Count + 1);
            conversation.AddMessage(userMessage);
            _dbContext.AICoachMessages.Add(userMessage);

            // Build conversation history
            var conversationHistory = conversation.Messages
                .OrderBy(m => m.Sequence)
                .Select(m => new AIChatMessage
                {
                    Role = m.Role,
                    Content = m.Content
                })
                .ToList();

            // Generate AI response
            var config = await GetAICoachConfigAsync(cancellationToken);
            var (aiResponse, tokenCount) = await _aiService.GenerateChatResponseAsync(
                systemPrompt,
                conversationHistory,
                config.MaxConversationTokens,
                cancellationToken);

            // Add assistant message
            var assistantMessage = AICoachMessage.CreateAssistantMessage(
                conversation.Id,
                aiResponse,
                tokenCount,
                conversation.Messages.Count + 1);
            conversation.AddMessage(assistantMessage);
            _dbContext.AICoachMessages.Add(assistantMessage);

            // Update token usage
            await UpdateTokenUsageAsync(userId, null, tokenCount, cancellationToken);

            // Save changes
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Record feature gate usage
            if (sendOrgId != Guid.Empty)
                await _featureGate.RecordUsageAsync(sendOrgId, PlanFeatures.MaxAiCoachMessagesMonthly, 1, cancellationToken);

            _logger.LogInformation(
                "Sent message to AI Coach conversation {ConversationId} for user {UserId}",
                conversation.Id, userId);

            // Fire-and-forget notification (new scope to avoid disposed DbContext)
            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                    var preview = aiResponse.Length > 100 ? aiResponse[..100] + "..." : aiResponse;
                    await notificationService.CreateNotificationAsync(
                        new CreateNotificationCommand(
                            userId,
                            NotificationType.AICoachReply,
                            NotificationCategory.AI,
                            "Nouveau message du coach IA",
                            "New AI Coach message",
                            preview,
                            preview,
                            ActionUrl: $"/ai-coach/{conversation.Id}",
                            RelatedEntityId: conversation.Id,
                            GroupKey: $"ai-coach-{conversation.Id}"),
                        CancellationToken.None);
                }
                catch { /* Non-critical */ }
            }, CancellationToken.None);

            return Result.Success(MapMessageToResponse(assistantMessage));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to AI Coach conversation {ConversationId}", request.ConversationId);
            return Result.Failure<AICoachMessageResponse>(
                new Error("AICoach.Error", "An error occurred while sending the message"));
        }
    }

    public async Task<Result<AICoachConversationResponse>> GetConversationAsync(
        Guid userId,
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var conversation = await _dbContext.AICoachConversations
                .Include(c => c.Messages.OrderBy(m => m.Sequence))
                .FirstOrDefaultAsync(c =>
                    c.Id == conversationId &&
                    c.UserId == userId,
                    cancellationToken);

            if (conversation == null)
                return Result.Failure<AICoachConversationResponse>(
                    new Error("AICoach.ConversationNotFound", "Conversation not found"));

            return Result.Success(MapToResponse(conversation));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting AI Coach conversation {ConversationId}", conversationId);
            return Result.Failure<AICoachConversationResponse>(
                new Error("AICoach.Error", "An error occurred while retrieving the conversation"));
        }
    }

    public async Task<Result<AICoachConversationResponse?>> GetConversationByQuestionAsync(
        Guid userId,
        Guid businessPlanId,
        string questionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var conversation = await _dbContext.AICoachConversations
                .Include(c => c.Messages.OrderBy(m => m.Sequence))
                .FirstOrDefaultAsync(c =>
                    c.UserId == userId &&
                    c.BusinessPlanId == businessPlanId &&
                    c.QuestionId == questionId &&
                    c.IsActive,
                    cancellationToken);

            if (conversation == null)
                return Result.Success<AICoachConversationResponse?>(null);

            return Result.Success<AICoachConversationResponse?>(MapToResponse(conversation));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting AI Coach conversation by question {QuestionId}", questionId);
            return Result.Failure<AICoachConversationResponse?>(
                new Error("AICoach.Error", "An error occurred while retrieving the conversation"));
        }
    }

    public async Task<Result<AICoachTokenUsageResponse>> GetTokenUsageAsync(
        Guid userId,
        Guid? organizationId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var currentMonth = int.Parse(DateTime.UtcNow.ToString("yyyyMM"));
            var config = await GetAICoachConfigAsync(cancellationToken);

            // Get subscription to determine token limit
            var subscriptionResult = await _subscriptionService.GetCurrentSubscriptionAsync(userId, cancellationToken);
            var subscriptionTier = SubscriptionPlanType.Free;
            if (subscriptionResult.IsSuccess && subscriptionResult.Value != null)
            {
                Enum.TryParse<SubscriptionPlanType>(subscriptionResult.Value.Plan?.PlanType, out subscriptionTier);
            }

            var tokenLimit = GetTokenLimitForTier(subscriptionTier, config);

            // Get or create usage record
            var usage = await _dbContext.AICoachUsages
                .FirstOrDefaultAsync(u =>
                    u.UserId == userId &&
                    u.OrganizationId == organizationId &&
                    u.Month == currentMonth,
                    cancellationToken);

            var tokensUsed = usage?.TotalTokensUsed ?? 0;
            var usagePercent = tokenLimit > 0 ? (double)tokensUsed / tokenLimit * 100 : 0;
            var isNearLimit = usagePercent >= config.WarningThresholdPercent;

            string? warningMessage = null;
            if (isNearLimit && usagePercent < 100)
            {
                warningMessage = $"You have used {usagePercent:F0}% of your monthly AI Coach tokens. Consider upgrading for more.";
            }
            else if (usagePercent >= 100)
            {
                warningMessage = "You have reached your monthly AI Coach token limit.";
            }

            return Result.Success(new AICoachTokenUsageResponse
            {
                TokensUsed = tokensUsed,
                TokenLimit = tokenLimit,
                UsagePercent = Math.Min(usagePercent, 100),
                IsNearLimit = isNearLimit,
                WarningMessage = warningMessage,
                TokensRemaining = Math.Max(0, tokenLimit - tokensUsed),
                CurrentMonth = currentMonth
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting AI Coach token usage for user {UserId}", userId);
            return Result.Failure<AICoachTokenUsageResponse>(
                new Error("AICoach.Error", "An error occurred while retrieving token usage"));
        }
    }

    public async Task<Result<AICoachAccessResponse>> CheckAccessAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if feature is enabled
            var featureEnabledResult = await _settingsService.IsFeatureEnabledAsync(FeatureFlagKey, cancellationToken);
            var featureEnabled = featureEnabledResult.IsSuccess && featureEnabledResult.Value;

            if (!featureEnabled)
            {
                return Result.Success(new AICoachAccessResponse
                {
                    HasAccess = false,
                    FeatureEnabled = false,
                    DenialReason = "AI Coach feature is not available at this time."
                });
            }

            // Check subscription tier
            var subscriptionResult = await _subscriptionService.GetCurrentSubscriptionAsync(userId, cancellationToken);
            var subscriptionTier = SubscriptionPlanType.Free;
            string? tierName = "Free";

            if (subscriptionResult.IsSuccess && subscriptionResult.Value != null)
            {
                Enum.TryParse<SubscriptionPlanType>(subscriptionResult.Value.Plan?.PlanType, out subscriptionTier);
                tierName = subscriptionResult.Value.Plan?.PlanType;
            }

            // AI Coach is available for Pro and Enterprise only
            if (subscriptionTier == SubscriptionPlanType.Free)
            {
                return Result.Success(new AICoachAccessResponse
                {
                    HasAccess = false,
                    FeatureEnabled = true,
                    SubscriptionTier = tierName,
                    DenialReason = "AI Coach is available for Pro and Enterprise subscribers. Upgrade your plan to access this feature.",
                    UpgradeUrl = "/pricing"
                });
            }

            // Check token limits
            var usageResult = await GetTokenUsageAsync(userId, null, cancellationToken);
            if (usageResult.IsFailure)
                return Result.Failure<AICoachAccessResponse>(usageResult.Error!);

            if (usageResult.Value!.UsagePercent >= 100)
            {
                return Result.Success(new AICoachAccessResponse
                {
                    HasAccess = false,
                    FeatureEnabled = true,
                    SubscriptionTier = tierName,
                    DenialReason = "You have reached your monthly AI Coach token limit. Your limit resets at the beginning of next month.",
                    UpgradeUrl = subscriptionTier != SubscriptionPlanType.Enterprise ? "/pricing" : null
                });
            }

            return Result.Success(new AICoachAccessResponse
            {
                HasAccess = true,
                FeatureEnabled = true,
                SubscriptionTier = tierName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking AI Coach access for user {UserId}", userId);
            return Result.Failure<AICoachAccessResponse>(
                new Error("AICoach.Error", "An error occurred while checking access"));
        }
    }

    private async Task<string> BuildQuestionnaireContextAsync(Guid businessPlanId, CancellationToken cancellationToken)
    {
        var responses = await _dbContext.QuestionnaireResponses
            .Where(r => r.BusinessPlanId == businessPlanId && !r.IsDeleted)
            .OrderBy(r => r.Created)
            .ToListAsync(cancellationToken);

        if (!responses.Any())
            return "No previous questionnaire responses available.";

        var sb = new StringBuilder();
        sb.AppendLine("## Previous Questionnaire Responses:");
        foreach (var response in responses)
        {
            var questionId = response.QuestionTemplateId?.ToString() ?? "Unknown";
            sb.AppendLine($"- **{questionId}**: {response.ResponseText}");
        }

        return sb.ToString();
    }

    private string BuildSystemPrompt(StartCoachConversationRequest request, string questionnaireContext)
    {
        var language = request.Language?.ToLowerInvariant() == "fr" ? "French" : "English";
        var persona = request.Persona ?? "Entrepreneur";

        return $@"You are Sqordia, an expert business plan coach helping entrepreneurs create compelling business plans.

CONTEXT:
- Current Question: {request.QuestionText ?? "Not specified"}
- Question Number: {request.QuestionNumber?.ToString() ?? "Not specified"}
- User Persona: {persona}
- Language: {language}

BUSINESS CONTEXT:
{questionnaireContext}

CURRENT ANSWER (if provided by user):
{request.CurrentAnswer ?? "No answer provided yet."}

INSTRUCTIONS:
1. Help the user answer this specific question through coaching
2. Ask clarifying questions to understand their business better
3. Provide specific, actionable suggestions based on their persona
4. Reference their previous answers for personalization
5. Respond in {language}
6. Keep responses concise (max 3-4 paragraphs)
7. If asked to generate content, provide a draft they can customize
8. Focus on making the plan bank-ready and compelling
9. Be encouraging but honest about areas that need improvement
10. For OBNL/Non-profit personas, focus on mission alignment and social impact

Remember: You are a coach, not just an answer generator. Guide the user to develop their own insights.";
    }

    private async Task UpdateTokenUsageAsync(
        Guid userId,
        Guid? organizationId,
        int tokensUsed,
        CancellationToken cancellationToken)
    {
        var currentMonth = int.Parse(DateTime.UtcNow.ToString("yyyyMM"));

        var usage = await _dbContext.AICoachUsages
            .FirstOrDefaultAsync(u =>
                u.UserId == userId &&
                u.OrganizationId == organizationId &&
                u.Month == currentMonth,
                cancellationToken);

        if (usage == null)
        {
            usage = new AICoachUsage(userId, organizationId, currentMonth);
            _dbContext.AICoachUsages.Add(usage);
        }

        usage.IncrementUsage(tokensUsed);
    }

    private async Task<AICoachConfig> GetAICoachConfigAsync(CancellationToken cancellationToken)
    {
        try
        {
            var settingResult = await _settingsService.GetSettingAsync(FeatureFlagKey, cancellationToken);
            if (settingResult.IsSuccess && !string.IsNullOrEmpty(settingResult.Value))
            {
                var config = JsonSerializer.Deserialize<AICoachConfig>(settingResult.Value);
                if (config != null)
                    return config;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse AI Coach config, using defaults");
        }

        return new AICoachConfig
        {
            MaxMonthlyTokensPro = DefaultMaxMonthlyTokensPro,
            MaxMonthlyTokensEnterprise = DefaultMaxMonthlyTokensEnterprise,
            WarningThresholdPercent = DefaultWarningThresholdPercent,
            MaxConversationTokens = DefaultMaxConversationTokens
        };
    }

    private int GetTokenLimitForTier(SubscriptionPlanType tier, AICoachConfig config)
    {
        return tier switch
        {
            SubscriptionPlanType.Enterprise => config.MaxMonthlyTokensEnterprise,
            SubscriptionPlanType.Professional => config.MaxMonthlyTokensPro,
            SubscriptionPlanType.Starter => config.MaxMonthlyTokensPro / 2,
            SubscriptionPlanType.Free => config.MaxMonthlyTokensPro / 10,
            _ => config.MaxMonthlyTokensPro
        };
    }

    private AICoachConversationResponse MapToResponse(AICoachConversation conversation)
    {
        return new AICoachConversationResponse
        {
            Id = conversation.Id,
            BusinessPlanId = conversation.BusinessPlanId,
            QuestionId = conversation.QuestionId,
            QuestionNumber = conversation.QuestionNumber,
            QuestionText = conversation.QuestionText,
            Language = conversation.Language,
            Persona = conversation.Persona,
            TotalTokensUsed = conversation.TotalTokensUsed,
            LastMessageAt = conversation.LastMessageAt,
            IsActive = conversation.IsActive,
            CreatedAt = conversation.Created,
            Messages = conversation.Messages
                .OrderBy(m => m.Sequence)
                .Select(MapMessageToResponse)
                .ToList()
        };
    }

    private AICoachMessageResponse MapMessageToResponse(AICoachMessage message)
    {
        return new AICoachMessageResponse
        {
            Id = message.Id,
            Role = message.Role,
            Content = message.Content,
            TokenCount = message.TokenCount,
            Sequence = message.Sequence,
            CreatedAt = message.Created
        };
    }
}

internal class AICoachConfig
{
    public int MaxMonthlyTokensPro { get; set; } = 100000;
    public int MaxMonthlyTokensEnterprise { get; set; } = 500000;
    public int WarningThresholdPercent { get; set; } = 80;
    public int MaxConversationTokens { get; set; } = 8000;
}

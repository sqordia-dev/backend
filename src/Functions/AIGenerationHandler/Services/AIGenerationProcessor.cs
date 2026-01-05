using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using Sqordia.Functions.AIGenerationHandler.Configuration;
using Sqordia.Functions.AIGenerationHandler.Models;
using System.Text.Json;

namespace Sqordia.Functions.AIGenerationHandler.Services;

/// <summary>
/// Service implementation for processing AI business plan generation jobs (Azure version)
/// </summary>
public class AIGenerationProcessor : IAIGenerationProcessor
{
    private readonly SecretClient _secretClient;
    private readonly ILogger<AIGenerationProcessor> _logger;
    private readonly AIGenerationConfiguration _config;

    public AIGenerationProcessor(
        SecretClient secretClient,
        ILogger<AIGenerationProcessor> logger,
        IOptions<AIGenerationConfiguration> config)
    {
        _secretClient = secretClient;
        _logger = logger;
        _config = config.Value;
    }

    public async Task<bool> ProcessGenerationJobAsync(AIGenerationJobMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Processing AI generation job {JobId} for business plan {BusinessPlanId}",
                message.JobId,
                message.BusinessPlanId);

            // Get AI API key from Secret Manager
            var apiKey = await GetApiKeyAsync(message.AiProvider ?? _config.DefaultAiProvider, cancellationToken);
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogError("Failed to retrieve API key for provider {Provider}", message.AiProvider);
                return false;
            }

            // TODO: Implement actual AI generation logic
            // This is a placeholder - the actual implementation would:
            // 1. Update business plan status to "Generating"
            // 2. Generate content for each section using the AI provider
            // 3. Update business plan sections with generated content
            // 4. Update business plan status to "Completed"

            _logger.LogInformation(
                "Successfully processed AI generation job {JobId} for business plan {BusinessPlanId}",
                message.JobId,
                message.BusinessPlanId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing AI generation job {JobId} for business plan {BusinessPlanId}",
                message.JobId,
                message.BusinessPlanId);
            throw;
        }
    }

    private async Task<string?> GetApiKeyAsync(string provider, CancellationToken cancellationToken)
    {
        try
        {
            var secretName = provider.ToLowerInvariant() switch
            {
                "openai" => _config.OpenAISecretName,
                "claude" => _config.ClaudeSecretName,
                "gemini" => _config.GeminiSecretName,
                _ => _config.OpenAISecretName
            };

            var secret = await _secretClient.GetSecretAsync(secretName, cancellationToken: cancellationToken);
            return secret.Value.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve API key for provider {Provider} from Key Vault", provider);
            return null;
        }
    }
}


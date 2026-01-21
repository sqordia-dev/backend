using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sqordia.Functions.AIGenerationHandler.Configuration;
using Sqordia.Functions.AIGenerationHandler.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Sqordia.Functions.AIGenerationHandler.Services;

/// <summary>
/// Service implementation for processing AI business plan generation jobs (Azure version)
/// Delegates to WebAPI for actual generation to keep the function lightweight
/// </summary>
public class AIGenerationProcessor : IAIGenerationProcessor
{
    private readonly SecretClient _secretClient;
    private readonly ILogger<AIGenerationProcessor> _logger;
    private readonly AIGenerationConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;

    public AIGenerationProcessor(
        SecretClient secretClient,
        ILogger<AIGenerationProcessor> logger,
        IOptions<AIGenerationConfiguration> config,
        IHttpClientFactory httpClientFactory)
    {
        _secretClient = secretClient;
        _logger = logger;
        _config = config.Value;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<bool> ProcessGenerationJobAsync(AIGenerationJobMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Processing AI generation job {JobId} for business plan {BusinessPlanId}",
                message.JobId,
                message.BusinessPlanId);

            // Validate business plan ID
            if (!Guid.TryParse(message.BusinessPlanId, out var planId))
            {
                _logger.LogError("Invalid business plan ID format: {BusinessPlanId}", message.BusinessPlanId);
                return false;
            }

            // Get system API key for authenticating with WebAPI
            var systemApiKey = await GetSystemApiKeyAsync(cancellationToken);
            if (string.IsNullOrEmpty(systemApiKey))
            {
                _logger.LogError("Failed to retrieve system API key from Key Vault");
                return false;
            }

            // Call WebAPI to generate business plan
            var success = await CallGenerationApiAsync(planId, message.Language, systemApiKey, cancellationToken);

            if (success)
            {
                _logger.LogInformation(
                    "Successfully processed AI generation job {JobId} for business plan {BusinessPlanId}",
                    message.JobId,
                    message.BusinessPlanId);
            }
            else
            {
                _logger.LogError(
                    "Failed to process AI generation job {JobId} for business plan {BusinessPlanId}",
                    message.JobId,
                    message.BusinessPlanId);
            }

            return success;
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

    private async Task<bool> CallGenerationApiAsync(Guid businessPlanId, string language, string apiKey, CancellationToken cancellationToken)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("WebApi");

            // Set authorization header with system API key
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            // Build the request URL
            var requestUrl = $"api/v1/business-plans/{businessPlanId}/generate?language={Uri.EscapeDataString(language)}";

            _logger.LogInformation("Calling WebAPI generation endpoint: {Url}", requestUrl);

            // Make the POST request
            var response = await client.PostAsync(requestUrl, null, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("WebAPI generation request successful for business plan {BusinessPlanId}", businessPlanId);
                return true;
            }

            // Log error details
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "WebAPI generation request failed with status {StatusCode}. Response: {Response}",
                response.StatusCode,
                errorContent);

            return false;
        }
        catch (TaskCanceledException ex) when (ex.CancellationToken != cancellationToken)
        {
            // HTTP timeout
            _logger.LogError(ex, "HTTP request to WebAPI timed out after {Timeout} seconds", _config.HttpTimeoutSeconds);
            return false;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request to WebAPI failed");
            return false;
        }
    }

    private async Task<string?> GetSystemApiKeyAsync(CancellationToken cancellationToken)
    {
        try
        {
            var secret = await _secretClient.GetSecretAsync(_config.SystemApiKeySecretName, cancellationToken: cancellationToken);
            return secret.Value.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve system API key from Key Vault");
            return null;
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


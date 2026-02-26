using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Services;
using Sqordia.Contracts.Requests.AI;
using Sqordia.Contracts.Responses.AI;
using Sqordia.Domain.Enums;
using Sqordia.Infrastructure.Services;
using System.Diagnostics;
using System.Text.Json;

namespace WebAPI.Controllers;

/// <summary>
/// AI Provider Configuration API for managing AI models
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/ai-config")]
[Authorize(Roles = "Admin")]
public class AIConfigController : BaseApiController
{
    private readonly ISettingsService _settingsService;
    private readonly IAIProviderFactory _providerFactory;
    private readonly IConfiguration _configuration;
    private readonly IOptions<OpenAISettings> _openAISettings;
    private readonly IOptions<ClaudeSettings> _claudeSettings;
    private readonly IOptions<GeminiSettings> _geminiSettings;
    private readonly ILogger<AIConfigController> _logger;

    public AIConfigController(
        ISettingsService settingsService,
        IAIProviderFactory providerFactory,
        IConfiguration configuration,
        IOptions<OpenAISettings> openAISettings,
        IOptions<ClaudeSettings> claudeSettings,
        IOptions<GeminiSettings> geminiSettings,
        ILogger<AIConfigController> logger)
    {
        _settingsService = settingsService;
        _providerFactory = providerFactory;
        _configuration = configuration;
        _openAISettings = openAISettings;
        _claudeSettings = claudeSettings;
        _geminiSettings = geminiSettings;
        _logger = logger;
    }

    /// <summary>
    /// Get current AI provider configuration
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetConfiguration(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Admin requesting AI provider configuration");

        try
        {
            // Get active provider from database or default
            var activeProviderResult = await _settingsService.GetSettingAsync("AI.ActiveProvider");
            var activeProvider = activeProviderResult.IsSuccess && !string.IsNullOrEmpty(activeProviderResult.Value)
                ? activeProviderResult.Value
                : "OpenAI";

            // Get fallback providers
            var fallbackProvidersResult = await _settingsService.GetSettingAsync("AI.FallbackProviders");
            var fallbackProviders = new List<string>();

            if (fallbackProvidersResult.IsSuccess && !string.IsNullOrEmpty(fallbackProvidersResult.Value))
            {
                try
                {
                    fallbackProviders = JsonSerializer.Deserialize<List<string>>(fallbackProvidersResult.Value) ?? new List<string>();
                }
                catch
                {
                    fallbackProviders = new List<string> { "Claude", "Gemini" };
                }
            }
            else
            {
                fallbackProviders = new List<string> { "Claude", "Gemini" };
            }

            // Get provider information - check both database AND environment/config
            var providers = new Dictionary<string, ProviderInfo>();

            // OpenAI - check database first, then fall back to configured options
            var openAIDbKey = await _settingsService.GetSettingAsync("AI.OpenAI.ApiKey");
            var openAIDbModel = await _settingsService.GetSettingAsync("AI.OpenAI.Model");
            var openAIConfigured = _openAISettings.Value;

            var openAIHasKey = (openAIDbKey.IsSuccess && !string.IsNullOrEmpty(openAIDbKey.Value))
                || !string.IsNullOrEmpty(openAIConfigured.ApiKey);
            var openAIModel = (openAIDbModel.IsSuccess && !string.IsNullOrEmpty(openAIDbModel.Value))
                ? openAIDbModel.Value
                : openAIConfigured.Model ?? "gpt-4o";
            var openAIKeyPreview = openAIDbKey.IsSuccess && !string.IsNullOrEmpty(openAIDbKey.Value)
                ? MaskApiKey(openAIDbKey.Value)
                : MaskApiKey(openAIConfigured.ApiKey);

            providers["OpenAI"] = new ProviderInfo
            {
                IsConfigured = openAIHasKey,
                Model = openAIModel,
                ApiKeyPreview = openAIKeyPreview,
                Source = (openAIDbKey.IsSuccess && !string.IsNullOrEmpty(openAIDbKey.Value)) ? "Database" : "Environment"
            };

            // Claude
            var claudeDbKey = await _settingsService.GetSettingAsync("AI.Claude.ApiKey");
            var claudeDbModel = await _settingsService.GetSettingAsync("AI.Claude.Model");
            var claudeConfigured = _claudeSettings.Value;

            var claudeHasKey = (claudeDbKey.IsSuccess && !string.IsNullOrEmpty(claudeDbKey.Value))
                || !string.IsNullOrEmpty(claudeConfigured.ApiKey);
            var claudeModel = (claudeDbModel.IsSuccess && !string.IsNullOrEmpty(claudeDbModel.Value))
                ? claudeDbModel.Value
                : claudeConfigured.Model ?? "claude-3-5-sonnet-20241022";
            var claudeKeyPreview = claudeDbKey.IsSuccess && !string.IsNullOrEmpty(claudeDbKey.Value)
                ? MaskApiKey(claudeDbKey.Value)
                : MaskApiKey(claudeConfigured.ApiKey);

            providers["Claude"] = new ProviderInfo
            {
                IsConfigured = claudeHasKey,
                Model = claudeModel,
                ApiKeyPreview = claudeKeyPreview,
                Source = (claudeDbKey.IsSuccess && !string.IsNullOrEmpty(claudeDbKey.Value)) ? "Database" : "Environment"
            };

            // Gemini
            var geminiDbKey = await _settingsService.GetSettingAsync("AI.Gemini.ApiKey");
            var geminiDbModel = await _settingsService.GetSettingAsync("AI.Gemini.Model");
            var geminiConfigured = _geminiSettings.Value;

            var geminiHasKey = (geminiDbKey.IsSuccess && !string.IsNullOrEmpty(geminiDbKey.Value))
                || !string.IsNullOrEmpty(geminiConfigured.ApiKey);
            var geminiModel = (geminiDbModel.IsSuccess && !string.IsNullOrEmpty(geminiDbModel.Value))
                ? geminiDbModel.Value
                : geminiConfigured.Model ?? "gemini-1.5-pro";
            var geminiKeyPreview = geminiDbKey.IsSuccess && !string.IsNullOrEmpty(geminiDbKey.Value)
                ? MaskApiKey(geminiDbKey.Value)
                : MaskApiKey(geminiConfigured.ApiKey);

            providers["Gemini"] = new ProviderInfo
            {
                IsConfigured = geminiHasKey,
                Model = geminiModel,
                ApiKeyPreview = geminiKeyPreview,
                Source = (geminiDbKey.IsSuccess && !string.IsNullOrEmpty(geminiDbKey.Value)) ? "Database" : "Environment"
            };

            var response = new AIConfigurationResponse
            {
                ActiveProvider = activeProvider,
                FallbackProviders = fallbackProviders,
                Providers = providers
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving AI configuration");
            return StatusCode(500, new { message = "Error retrieving AI configuration", error = ex.Message });
        }
    }

    /// <summary>
    /// Update AI provider configuration
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> UpdateConfiguration(
        [FromBody] AIConfigurationRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Admin updating AI provider configuration");

        try
        {
            // Validate that active provider is configured
            if (request.Providers.TryGetValue(request.ActiveProvider, out var activeProviderSettings))
            {
                // Check if we're updating the API key or if one already exists
                bool hasApiKey = false;

                if (!string.IsNullOrEmpty(activeProviderSettings.ApiKey))
                {
                    hasApiKey = true;
                }
                else
                {
                    // Check if API key already exists in settings
                    var existingKeyResult = await _settingsService.GetSettingAsync($"AI.{request.ActiveProvider}.ApiKey");
                    if (existingKeyResult.IsSuccess && !string.IsNullOrEmpty(existingKeyResult.Value))
                    {
                        hasApiKey = true;
                    }
                    else
                    {
                        // Check if configured via environment variables
                        var configuredKey = request.ActiveProvider switch
                        {
                            "OpenAI" => _openAISettings.Value.ApiKey,
                            "Claude" => _claudeSettings.Value.ApiKey,
                            "Gemini" => _geminiSettings.Value.ApiKey,
                            _ => null
                        };
                        hasApiKey = !string.IsNullOrEmpty(configuredKey);
                    }
                }

                if (!hasApiKey)
                {
                    return BadRequest(new { message = $"Active provider {request.ActiveProvider} must have an API key configured" });
                }
            }
            else
            {
                return BadRequest(new { message = $"Active provider {request.ActiveProvider} not found in provider settings" });
            }

            // Update active provider
            await _settingsService.UpsertSettingAsync(
                "AI.ActiveProvider",
                request.ActiveProvider,
                "AI",
                "Active AI provider",
                isPublic: false,
                settingType: SettingType.Config,
                dataType: SettingDataType.String,
                encrypt: false,
                isCritical: true
            );

            // Update fallback providers
            var fallbackJson = JsonSerializer.Serialize(request.FallbackProviders);
            await _settingsService.UpsertSettingAsync(
                "AI.FallbackProviders",
                fallbackJson,
                "AI",
                "Fallback AI providers in order",
                isPublic: false,
                settingType: SettingType.Config,
                dataType: SettingDataType.Json,
                encrypt: false,
                isCritical: true
            );

            // Update each provider's settings
            foreach (var providerKvp in request.Providers)
            {
                var providerName = providerKvp.Key;
                var providerSettings = providerKvp.Value;

                // Update API key if provided (not empty and not placeholder)
                if (!string.IsNullOrEmpty(providerSettings.ApiKey) && providerSettings.ApiKey != "existing")
                {
                    await _settingsService.UpsertSettingAsync(
                        $"AI.{providerName}.ApiKey",
                        providerSettings.ApiKey,
                        "AI",
                        $"{providerName} API key",
                        isPublic: false,
                        settingType: SettingType.Secret,
                        dataType: SettingDataType.String,
                        encrypt: true,
                        isCritical: false
                    );
                }

                // Update model
                await _settingsService.UpsertSettingAsync(
                    $"AI.{providerName}.Model",
                    providerSettings.Model,
                    "AI",
                    $"{providerName} model name",
                    isPublic: false,
                    settingType: SettingType.Config,
                    dataType: SettingDataType.String,
                    encrypt: false,
                    isCritical: false
                );
            }

            // Invalidate factory cache to pick up new settings
            _providerFactory.InvalidateCache();

            _logger.LogInformation("AI provider configuration updated successfully. Active provider: {ActiveProvider}",
                request.ActiveProvider);

            return Ok(new { message = "AI configuration updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating AI configuration");
            return StatusCode(500, new { message = "Error updating AI configuration", error = ex.Message });
        }
    }

    /// <summary>
    /// Test connection to an AI provider
    /// </summary>
    [HttpPost("test/{provider}")]
    public async Task<IActionResult> TestProvider(
        string provider,
        [FromBody] ProviderTestRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Testing AI provider: {Provider}", provider);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Determine API key to use
            string apiKeyToUse;

            if (!string.IsNullOrEmpty(request.ApiKey) && request.ApiKey != "existing")
            {
                // Use the provided API key
                apiKeyToUse = request.ApiKey;
            }
            else
            {
                // Try to get from database first
                var dbKeyResult = await _settingsService.GetSettingAsync($"AI.{provider}.ApiKey");
                if (dbKeyResult.IsSuccess && !string.IsNullOrEmpty(dbKeyResult.Value))
                {
                    apiKeyToUse = dbKeyResult.Value;
                }
                else
                {
                    // Fall back to environment/config
                    apiKeyToUse = provider switch
                    {
                        "OpenAI" => _openAISettings.Value.ApiKey ?? "",
                        "Claude" => _claudeSettings.Value.ApiKey ?? "",
                        "Gemini" => _geminiSettings.Value.ApiKey ?? "",
                        _ => ""
                    };
                }
            }

            if (string.IsNullOrEmpty(apiKeyToUse))
            {
                stopwatch.Stop();
                return Ok(new ProviderTestResponse
                {
                    Success = false,
                    Message = "No API key configured",
                    ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds,
                    ModelUsed = request.Model,
                    ErrorDetails = $"Please configure an API key for {provider}"
                });
            }

            // Make a simple test request to validate the API key and model
            var testResult = await TestProviderConnection(provider, apiKeyToUse, request.Model, cancellationToken);

            stopwatch.Stop();

            return Ok(new ProviderTestResponse
            {
                Success = testResult.success,
                Message = testResult.message,
                ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds,
                ModelUsed = request.Model,
                ErrorDetails = testResult.error
            });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error testing AI provider: {Provider}", provider);

            return Ok(new ProviderTestResponse
            {
                Success = false,
                Message = "Connection test failed",
                ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds,
                ModelUsed = request.Model,
                ErrorDetails = ex.Message
            });
        }
    }

    /// <summary>
    /// Get available models for a specific provider
    /// </summary>
    [HttpGet("models/{provider}")]
    public IActionResult GetAvailableModels(string provider)
    {
        var models = provider.ToLower() switch
        {
            "openai" => new[] { "gpt-4o", "gpt-4o-mini", "gpt-4-turbo", "gpt-4", "gpt-3.5-turbo" },
            "claude" => new[] { "claude-sonnet-4-20250514", "claude-3-5-sonnet-20241022", "claude-3-5-haiku-20241022", "claude-3-opus-20240229", "claude-3-sonnet-20240229", "claude-3-haiku-20240307" },
            "gemini" => new[] { "gemini-2.0-flash", "gemini-1.5-pro", "gemini-1.5-flash", "gemini-pro" },
            _ => Array.Empty<string>()
        };

        return Ok(models);
    }

    // Helper methods

    private string MaskApiKey(string? apiKey)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            return "Not configured";
        }

        if (apiKey.Length <= 10)
        {
            return "***";
        }

        var start = apiKey.Substring(0, 7);
        var end = apiKey.Substring(apiKey.Length - 4);
        return $"{start}...{end}";
    }

    private async Task<(bool success, string message, string? error)> TestProviderConnection(
        string provider,
        string apiKey,
        string model,
        CancellationToken cancellationToken)
    {
        try
        {
            // Make a simple test request to validate the API key and model
            var testPrompt = "Respond with 'OK' if you can read this message.";

            switch (provider.ToLower())
            {
                case "openai":
                    {
                        var client = new OpenAI.OpenAIClient(new System.ClientModel.ApiKeyCredential(apiKey));
                        var chatClient = client.GetChatClient(model);
                        var messages = new List<OpenAI.Chat.ChatMessage>
                        {
                            OpenAI.Chat.ChatMessage.CreateUserMessage(testPrompt)
                        };
                        var response = await chatClient.CompleteChatAsync(messages, cancellationToken: cancellationToken);
                        return (true, "Connection successful", null);
                    }

                case "claude":
                    {
                        var client = new Anthropic.SDK.AnthropicClient(apiKey);
                        var parameters = new Anthropic.SDK.Messaging.MessageParameters
                        {
                            Model = model,
                            Messages = new List<Anthropic.SDK.Messaging.Message>
                            {
                                new Anthropic.SDK.Messaging.Message
                                {
                                    Role = Anthropic.SDK.Messaging.RoleType.User,
                                    Content = new List<Anthropic.SDK.Messaging.ContentBase>
                                    {
                                        new Anthropic.SDK.Messaging.TextContent { Text = testPrompt }
                                    }
                                }
                            },
                            MaxTokens = 10
                        };
                        var response = await client.Messages.GetClaudeMessageAsync(parameters, cancellationToken);
                        return (true, "Connection successful", null);
                    }

                case "gemini":
                    {
                        var client = new Mscc.GenerativeAI.GoogleAI(apiKey);
                        var generativeModel = client.GenerativeModel(model);
                        var response = await generativeModel.GenerateContent(testPrompt);
                        return (true, "Connection successful", null);
                    }

                default:
                    return (false, $"Unknown provider: {provider}", null);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Provider test failed for {Provider}", provider);
            return (false, "Connection failed", ex.Message);
        }
    }
}

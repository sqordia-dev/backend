using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Sqordia.Functions.AIGenerationHandler.Configuration;
using Sqordia.Functions.AIGenerationHandler.Services;
using Sqordia.Functions.Common;

namespace Sqordia.Functions.AIGenerationHandler;

/// <summary>
/// Startup class for configuring dependency injection
/// </summary>
public static class Startup
{
    /// <summary>
    /// Configure services for dependency injection
    /// </summary>
    public static void ConfigureServices(IServiceCollection services)
    {
        // Configuration
        var configuration = StartupBase.BuildConfiguration();
        var databaseConfig = StartupBase.GetDatabaseConfiguration(configuration);

        var keyVaultUrl = configuration["AzureKeyVault__VaultUrl"]
                       ?? configuration["AzureKeyVault:VaultUrl"]
                       ?? throw new InvalidOperationException("Azure Key Vault URL is not configured");

        var webApiBaseUrl = configuration["WebApi__BaseUrl"]
                         ?? configuration["WebApi:BaseUrl"]
                         ?? throw new InvalidOperationException("WebAPI base URL is not configured");

        var httpTimeoutSeconds = int.TryParse(
            configuration["AI__HttpTimeoutSeconds"] ?? configuration["AI:HttpTimeoutSeconds"],
            out var timeout) ? timeout : 300;

        var aiConfig = new AIGenerationConfiguration
        {
            OpenAISecretName = configuration["AI__OpenAISecretName"] ?? configuration["AI:OpenAISecretName"] ?? "openai-api-key",
            ClaudeSecretName = configuration["AI__ClaudeSecretName"] ?? configuration["AI:ClaudeSecretName"] ?? "claude-api-key",
            GeminiSecretName = configuration["AI__GeminiSecretName"] ?? configuration["AI:GeminiSecretName"] ?? "gemini-api-key",
            DefaultAiProvider = configuration["AI__DefaultProvider"] ?? configuration["AI:DefaultProvider"] ?? "openai",
            KeyVaultUrl = keyVaultUrl,
            WebApiBaseUrl = webApiBaseUrl,
            SystemApiKeySecretName = configuration["AI__SystemApiKeySecretName"] ?? configuration["AI:SystemApiKeySecretName"] ?? "system-api-key",
            HttpTimeoutSeconds = httpTimeoutSeconds
        };

        services.AddSingleton(Options.Create(aiConfig));

        // Azure Key Vault Services
        services.AddSingleton(new SecretClient(new Uri(keyVaultUrl), new Azure.Identity.DefaultAzureCredential()));

        // HTTP Client for WebAPI calls
        services.AddHttpClient("WebApi", client =>
        {
            client.BaseAddress = new Uri(webApiBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(httpTimeoutSeconds);
        });

        // Common services
        StartupBase.ConfigureCommonServices(services, configuration);

        // Application Services
        services.AddScoped<IAIGenerationProcessor, AIGenerationProcessor>();
    }
}


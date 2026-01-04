using Google.Cloud.SecretManager.V1;
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
    public static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Configuration
        var configuration = StartupBase.BuildConfiguration();
        var databaseConfig = StartupBase.GetDatabaseConfiguration(configuration);

        var gcpProjectId = configuration["GCP__ProjectId"] 
                        ?? configuration["GCP:ProjectId"] 
                        ?? Environment.GetEnvironmentVariable("GOOGLE_CLOUD_PROJECT") 
                        ?? throw new InvalidOperationException("GCP ProjectId is not configured");

        var aiConfig = new AIGenerationConfiguration
        {
            OpenAISecretName = configuration["AI__OpenAISecretName"] ?? configuration["AI:OpenAISecretName"] ?? "openai-api-key",
            ClaudeSecretName = configuration["AI__ClaudeSecretName"] ?? configuration["AI:ClaudeSecretName"] ?? "claude-api-key",
            GeminiSecretName = configuration["AI__GeminiSecretName"] ?? configuration["AI:GeminiSecretName"] ?? "gemini-api-key",
            DefaultAiProvider = configuration["AI__DefaultProvider"] ?? configuration["AI:DefaultProvider"] ?? "openai",
            GcpProjectId = gcpProjectId
        };

        services.AddSingleton(Options.Create(aiConfig));

        // GCP Services
        services.AddSingleton(SecretManagerServiceClient.Create());

        // Common services
        StartupBase.ConfigureCommonServices(services, configuration);

        // Application Services
        services.AddScoped<IAIGenerationProcessor, AIGenerationProcessor>();

        return services.BuildServiceProvider();
    }
}


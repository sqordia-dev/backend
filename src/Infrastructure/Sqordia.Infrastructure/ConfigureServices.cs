using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Google.Cloud.Storage.V1;
using Google.Cloud.PubSub.V1;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Security;
using Sqordia.Application.Services;
using Sqordia.Application.Services.Implementations;
using Sqordia.Infrastructure.Services;
using Sqordia.Infrastructure.Identity;
using Sqordia.Infrastructure.Localization;
using IIdentityService = Sqordia.Application.Common.Interfaces.IIdentityService;
using IJwtTokenService = Sqordia.Application.Common.Interfaces.IJwtTokenService;

namespace Sqordia.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // HTTP Context for getting client IP address
        services.AddHttpContextAccessor();

        // Email service configuration - GCP Pub/Sub
        var gcpProjectId = Environment.GetEnvironmentVariable("GCP__ProjectId")
                          ?? configuration["GCP:ProjectId"]
                          ?? throw new InvalidOperationException("GCP ProjectId is not configured. Set GCP:ProjectId in configuration or GCP__ProjectId environment variable.");
        
        var emailTopic = Environment.GetEnvironmentVariable("PubSub__EmailTopic")
                      ?? configuration["PubSub:EmailTopic"]
                      ?? throw new InvalidOperationException("Pub/Sub Email Topic is not configured. Set PubSub:EmailTopic in configuration or PubSub__EmailTopic environment variable.");

        var topicName = TopicName.FromProjectTopic(gcpProjectId, emailTopic);
        
        // Create PublisherClient for Pub/Sub
        var publisherClient = PublisherClient.Create(topicName);
        services.AddSingleton(publisherClient);
        
        services.AddTransient<IEmailService>(sp =>
            new PubSubEmailService(
                publisherClient,
                emailTopic,
                sp.GetRequiredService<ILogger<PubSubEmailService>>(),
                sp.GetRequiredService<ILocalizationService>()));

        // Security service - Required for password hashing
        services.AddTransient<ISecurityService, SecurityService>();

        // Identity services - Required for authentication
        services.AddTransient<IIdentityService, IdentityService>();
        services.AddTransient<IJwtTokenService, JwtTokenService>();
        services.AddTransient<IAccountLockoutService, AccountLockoutService>();
        services.AddTransient<ITotpService, TotpService>();

        // Localization service - Required for bilingual support
        services.AddSingleton<ILocalizationService, LocalizationService>();

        // AI service - Required for business plan generation
        // Configure from both appsettings and environment variables
        // Priority: Environment variables > appsettings.json
        services.Configure<OpenAISettings>(configuration.GetSection("AI:OpenAI"));
        
        // Post-configure to allow environment variables to override appsettings
        // This runs AFTER the initial Configure, so env vars will override
        services.PostConfigure<OpenAISettings>(options =>
        {
            // Try environment variables first (highest priority)
            var envApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
                          ?? Environment.GetEnvironmentVariable("OpenAI__ApiKey")
                          ?? Environment.GetEnvironmentVariable("AI__OpenAI__ApiKey");
            
            // Then try configuration (appsettings.json)
            var configApiKey = configuration["AI:OpenAI:ApiKey"]
                            ?? configuration["OpenAI:ApiKey"];
            
            // Use first non-empty value found
            var apiKey = envApiKey ?? configApiKey;
            
            // Use first non-empty value found (skip placeholder values)
            if (!string.IsNullOrEmpty(apiKey) && !apiKey.Contains("TODO", StringComparison.OrdinalIgnoreCase))
            {
                options.ApiKey = apiKey;
            }
            
            // Same for model
            var envModel = Environment.GetEnvironmentVariable("OPENAI_MODEL")
                        ?? Environment.GetEnvironmentVariable("OpenAI__Model")
                        ?? Environment.GetEnvironmentVariable("AI__OpenAI__Model");
            
            var configModel = configuration["AI:OpenAI:Model"]
                           ?? configuration["OpenAI:Model"];
            
            var model = envModel ?? configModel;
            
            if (!string.IsNullOrEmpty(model))
            {
                options.Model = model;
            }
        });
        
        services.AddSingleton<IAIService, OpenAIService>();

        // Document export service - Required for PDF/Word export
        services.AddTransient<IDocumentExportService, DocumentExportService>();

        // Financial projection service - Required for financial calculations and projections
        services.AddTransient<IFinancialProjectionService, FinancialProjectionService>();

        // Admin dashboard service - Required for admin management and analytics
        services.AddTransient<IAdminDashboardService, AdminDashboardService>();
        
        // AI Analysis service - Required for strategy suggestions, risk analysis, and business mentor
        services.AddTransient<IAIAnalysisService, AIAnalysisService>();
        
        // Currency conversion service - Required for multi-currency support
        services.AddTransient<ICurrencyConversionService, CurrencyConversionService>();
        
        // Pricing analysis service - Required for pricing and market analysis
        services.AddTransient<IPricingAnalysisService, PricingAnalysisService>();
        
        // SMART objectives service - Required for SMART objectives generation
        services.AddTransient<ISmartObjectiveService, SmartObjectiveService>();
        
        // Plan comment service - Required for collaborative comments
        services.AddTransient<IPlanCommentService, PlanCommentService>();
        
        // Content management service - Required for admin CMS
        services.AddTransient<IContentManagementService, ContentManagementService>();
        
        // Subscription service
        services.AddScoped<Sqordia.Application.Services.ISubscriptionService, SubscriptionService>();

        // Storage service configuration - GCP Cloud Storage
        var storageGcpProjectId = Environment.GetEnvironmentVariable("GCP__ProjectId")
                                 ?? configuration["GCP:ProjectId"]
                                 ?? throw new InvalidOperationException("GCP ProjectId is not configured. Set GCP:ProjectId in configuration or GCP__ProjectId environment variable.");
        
        var bucketName = Environment.GetEnvironmentVariable("CloudStorage__BucketName")
                      ?? configuration["CloudStorage:BucketName"]
                      ?? throw new InvalidOperationException("Cloud Storage BucketName is not configured. Set CloudStorage:BucketName in configuration or CloudStorage__BucketName environment variable.");

        var cloudStorageSettings = new CloudStorageSettings
        {
            BucketName = bucketName,
            ProjectId = storageGcpProjectId
        };
        services.AddSingleton(Options.Create(cloudStorageSettings));
        
        // Create StorageClient for Cloud Storage
        var storageClient = StorageClient.Create();
        services.AddSingleton(storageClient);
        
        services.AddScoped<IStorageService, CloudStorageService>();

        // Memory cache for settings caching
        services.AddMemoryCache();

        // Settings encryption service
        services.AddSingleton<ISettingsEncryptionService, SettingsEncryptionService>();

        // Settings cache service
        services.AddScoped<ISettingsCacheService, SettingsCacheService>();

        return services;
    }
}

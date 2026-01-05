using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Azure.Storage.Blobs;
using Azure.Messaging.ServiceBus;
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

        // Email service configuration - Azure Service Bus
        // Check if we're in a test environment
        var isTestEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Testing"
                             || Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Test"
                             || configuration["ASPNETCORE_ENVIRONMENT"] == "Testing"
                             || configuration["ASPNETCORE_ENVIRONMENT"] == "Test"
                             || Environment.GetEnvironmentVariable("SKIP_AZURE_SERVICES") == "true"
                             || configuration["SkipAzureServices"] == "true";
        
        var serviceBusConnectionString = Environment.GetEnvironmentVariable("AzureServiceBus__ConnectionString")
                                      ?? configuration["AzureServiceBus:ConnectionString"];
        
        var emailTopic = Environment.GetEnvironmentVariable("AzureServiceBus__EmailTopic")
                      ?? configuration["AzureServiceBus:EmailTopic"];

        // Only initialize Azure Service Bus if not in test environment and configuration is available
        if (!isTestEnvironment && !string.IsNullOrEmpty(serviceBusConnectionString) && !string.IsNullOrEmpty(emailTopic))
        {
            try
            {
                // Create ServiceBusClient for Service Bus
                var serviceBusClient = new ServiceBusClient(serviceBusConnectionString);
                services.AddSingleton(serviceBusClient);
                
                services.AddTransient<IEmailService>(sp =>
                    new ServiceBusEmailService(
                        serviceBusClient,
                        emailTopic,
                        sp.GetRequiredService<ILogger<ServiceBusEmailService>>(),
                        sp.GetRequiredService<ILocalizationService>()));
            }
            catch (Exception ex) when (ex.Message.Contains("connection", StringComparison.OrdinalIgnoreCase) 
                                   || ex.Message.Contains("authentication", StringComparison.OrdinalIgnoreCase)
                                   || ex is InvalidOperationException)
            {
                // If connection is not available, fall back to mock email service
                services.AddTransient<IEmailService, MockEmailService>();
            }
        }
        else
        {
            // Use mock email service for tests or when Azure Service Bus is not configured
            services.AddTransient<IEmailService, MockEmailService>();
        }

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

        // Storage service configuration - Azure Blob Storage
        var storageConnectionString = Environment.GetEnvironmentVariable("AzureStorage__ConnectionString")
                                   ?? configuration["AzureStorage:ConnectionString"];
        
        var containerName = Environment.GetEnvironmentVariable("AzureStorage__ContainerName")
                          ?? configuration["AzureStorage:ContainerName"]
                          ?? "documents";

        // Only initialize Azure Blob Storage if not in test environment and configuration is available
        if (!isTestEnvironment && !string.IsNullOrEmpty(storageConnectionString))
        {
            try
            {
                var azureStorageSettings = new AzureStorageSettings
                {
                    ContainerName = containerName,
                    ConnectionString = storageConnectionString,
                    AccountName = Environment.GetEnvironmentVariable("AzureStorage__AccountName")
                               ?? configuration["AzureStorage:AccountName"]
                               ?? string.Empty
                };
                services.AddSingleton(Options.Create(azureStorageSettings));
                
                // Create BlobServiceClient for Azure Blob Storage
                var blobServiceClient = new BlobServiceClient(storageConnectionString);
                services.AddSingleton(blobServiceClient);
                
                services.AddScoped<IStorageService, AzureBlobStorageService>();
            }
            catch (Exception ex) when (ex.Message.Contains("connection", StringComparison.OrdinalIgnoreCase) 
                                   || ex.Message.Contains("authentication", StringComparison.OrdinalIgnoreCase)
                                   || ex is InvalidOperationException)
            {
                // If connection is not available, use in-memory storage service
                services.AddScoped<IStorageService, InMemoryStorageService>();
            }
        }
        else
        {
            // Use in-memory storage service for tests
            services.AddScoped<IStorageService, InMemoryStorageService>();
        }

        // Memory cache for settings caching
        services.AddMemoryCache();

        // Settings encryption service
        services.AddSingleton<ISettingsEncryptionService, SettingsEncryptionService>();

        // Settings cache service
        services.AddScoped<ISettingsCacheService, SettingsCacheService>();

        return services;
    }
}

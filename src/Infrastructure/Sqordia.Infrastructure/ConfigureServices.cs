using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Localization;
using Azure.Storage.Blobs;
using Azure.Messaging.ServiceBus;
using Azure.Communication.Email;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Security;
using Sqordia.Application.Services;
using Sqordia.Application.Services.Implementations;
using Sqordia.Application.Services.Cms;
using Sqordia.Application.Services.Implementations.Cms;
using Sqordia.Application.Services.V2;
using Sqordia.Application.Services.V2.Implementations;
using Sqordia.Application.Financial.Services;
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

        // Email service configuration
        // Priority: Azure Communication Services Email > Azure Service Bus > MockEmailService
        // Check if we're in a test environment
        var isTestEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Testing"
                             || Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Test"
                             || configuration["ASPNETCORE_ENVIRONMENT"] == "Testing"
                             || configuration["ASPNETCORE_ENVIRONMENT"] == "Test"
                             || Environment.GetEnvironmentVariable("SKIP_AZURE_SERVICES") == "true"
                             || configuration["SkipAzureServices"] == "true";
        
        // Try Azure Communication Services Email first (preferred)
        var azureCommConnectionString = Environment.GetEnvironmentVariable("AzureCommunicationServices__ConnectionString")
                                     ?? configuration["AzureCommunicationServices:ConnectionString"];
        
        var fromEmail = Environment.GetEnvironmentVariable("AzureCommunicationServices__FromEmail")
                     ?? configuration["AzureCommunicationServices:FromEmail"]
                     ?? Environment.GetEnvironmentVariable("Email__FromAddress")
                     ?? configuration["Email:FromAddress"];
        
        var fromName = Environment.GetEnvironmentVariable("AzureCommunicationServices__FromName")
                    ?? configuration["AzureCommunicationServices:FromName"]
                    ?? Environment.GetEnvironmentVariable("Email__FromName")
                    ?? configuration["Email:FromName"]
                    ?? "Sqordia";

        bool emailServiceRegistered = false;

        // Only initialize Azure Communication Services Email if not in test environment and configuration is available
        if (!isTestEnvironment && !string.IsNullOrEmpty(azureCommConnectionString) && !string.IsNullOrEmpty(fromEmail))
        {
            try
            {
                var frontendBaseUrl = Environment.GetEnvironmentVariable("FRONTEND_BASE_URL")
                                   ?? configuration["Frontend:BaseUrl"]
                                   ?? "https://sqordia.app";
                
                var emailSettings = new AzureCommunicationEmailSettings
                {
                    ConnectionString = azureCommConnectionString,
                    FromEmail = fromEmail,
                    FromName = fromName,
                    FrontendBaseUrl = frontendBaseUrl
                };
                
                services.Configure<AzureCommunicationEmailSettings>(options =>
                {
                    options.ConnectionString = emailSettings.ConnectionString;
                    options.FromEmail = emailSettings.FromEmail;
                    options.FromName = emailSettings.FromName;
                    options.FrontendBaseUrl = emailSettings.FrontendBaseUrl;
                });
                
                // Create EmailClient for Azure Communication Services
                var emailClient = new EmailClient(azureCommConnectionString);
                services.AddSingleton(emailClient);
                
                services.AddTransient<IEmailService>(sp =>
                    new AzureCommunicationEmailService(
                        emailClient,
                        sp.GetRequiredService<IOptions<AzureCommunicationEmailSettings>>(),
                        sp.GetRequiredService<ILogger<AzureCommunicationEmailService>>(),
                        sp.GetRequiredService<ILocalizationService>(),
                        sp.GetRequiredService<IStringLocalizerFactory>()));
                
                emailServiceRegistered = true;
            }
            catch (Exception ex) when (ex.Message.Contains("connection", StringComparison.OrdinalIgnoreCase) 
                                   || ex.Message.Contains("authentication", StringComparison.OrdinalIgnoreCase)
                                   || ex is InvalidOperationException)
            {
                // If connection is not available, fall back to Service Bus or Mock
            }
        }

        // Fallback to Azure Service Bus if Azure Communication Services is not configured
        if (!emailServiceRegistered)
        {
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
                    
                    emailServiceRegistered = true;
                }
                catch (Exception ex) when (ex.Message.Contains("connection", StringComparison.OrdinalIgnoreCase) 
                                       || ex.Message.Contains("authentication", StringComparison.OrdinalIgnoreCase)
                                       || ex is InvalidOperationException)
                {
                    // If connection is not available, fall back to mock email service
                }
            }
        }

        // Use mock email service for tests or when no email service is configured
        if (!emailServiceRegistered)
        {
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

        // Helper method for checking placeholders
        static bool IsPlaceholder(string? value) =>
            !string.IsNullOrEmpty(value) && (
                value.Contains("TODO", StringComparison.OrdinalIgnoreCase) ||
                value.StartsWith("${", StringComparison.OrdinalIgnoreCase) ||
                value.Contains("${", StringComparison.OrdinalIgnoreCase));

        // Configure OpenAI
        services.Configure<OpenAISettings>(configuration.GetSection("AI:OpenAI"));
        services.PostConfigure<OpenAISettings>(options =>
        {
            var envApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
                          ?? Environment.GetEnvironmentVariable("OpenAI__ApiKey")
                          ?? Environment.GetEnvironmentVariable("AI__OpenAI__ApiKey");

            var configApiKey = configuration["AI:OpenAI:ApiKey"]
                            ?? configuration["OpenAI:ApiKey"];

            var apiKey = envApiKey ?? configApiKey;

            if (!string.IsNullOrEmpty(apiKey) && !IsPlaceholder(apiKey))
            {
                options.ApiKey = apiKey;
            }

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

        // Configure Claude
        services.Configure<ClaudeSettings>(configuration.GetSection("AI:Claude"));
        services.PostConfigure<ClaudeSettings>(options =>
        {
            var envApiKey = Environment.GetEnvironmentVariable("CLAUDE_API_KEY")
                          ?? Environment.GetEnvironmentVariable("AI__Claude__ApiKey");

            var configApiKey = configuration["AI:Claude:ApiKey"];

            var apiKey = envApiKey ?? configApiKey;

            if (!string.IsNullOrEmpty(apiKey) && !IsPlaceholder(apiKey))
            {
                options.ApiKey = apiKey;
            }

            var envModel = Environment.GetEnvironmentVariable("CLAUDE_MODEL")
                        ?? Environment.GetEnvironmentVariable("AI__Claude__Model");

            var configModel = configuration["AI:Claude:Model"];

            var model = envModel ?? configModel;

            if (!string.IsNullOrEmpty(model))
            {
                options.Model = model;
            }
        });

        // Configure Gemini
        services.Configure<GeminiSettings>(configuration.GetSection("AI:Gemini"));
        services.PostConfigure<GeminiSettings>(options =>
        {
            var envApiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY")
                          ?? Environment.GetEnvironmentVariable("AI__Gemini__ApiKey");

            var configApiKey = configuration["AI:Gemini:ApiKey"];

            var apiKey = envApiKey ?? configApiKey;

            if (!string.IsNullOrEmpty(apiKey) && !IsPlaceholder(apiKey))
            {
                options.ApiKey = apiKey;
            }

            var envModel = Environment.GetEnvironmentVariable("GEMINI_MODEL")
                        ?? Environment.GetEnvironmentVariable("AI__Gemini__Model");

            var configModel = configuration["AI:Gemini:Model"];

            var model = envModel ?? configModel;

            if (!string.IsNullOrEmpty(model))
            {
                options.Model = model;
            }
        });

        // Register all AI provider implementations as singletons
        services.AddSingleton<OpenAIService>();
        services.AddSingleton<ClaudeService>();
        services.AddSingleton<GeminiService>();

        // Register AI provider factory
        services.AddSingleton<IAIProviderFactory, AIProviderFactory>();

        // Register the main IAIService as the fallback wrapper (replaces direct OpenAIService registration)
        services.AddSingleton<IAIService, AIProviderWithFallback>();

        // Document export service - Required for PDF/Word export
        services.AddTransient<IDocumentExportService, DocumentExportService>();

        // Financial projection service - Required for financial calculations and projections
        services.AddTransient<IFinancialProjectionService, FinancialProjectionService>();
        
        // Financial service - Required for consultant financials and location overhead
        services.AddTransient<IFinancialService, FinancialService>();

        // Formula engine - Required for spreadsheet-like cell calculations
        services.AddSingleton<Application.Financial.Services.IFormulaEngine, FormulaEngine>();

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

        // CMS Versioning services
        services.AddTransient<ICmsVersionService, CmsVersionService>();
        services.AddTransient<ICmsContentBlockService, CmsContentBlockService>();
        services.AddTransient<ICmsAssetService, CmsAssetService>();
        services.AddTransient<IPublishedContentService, PublishedContentService>();

        // V2 Services - Growth Architect Intelligence Layer
        services.AddTransient<IAuditService, AuditService>();
        services.AddTransient<IReadinessScoreService, ReadinessScoreService>();
        services.AddTransient<IQuestionPolishService, QuestionPolishService>();
        services.AddTransient<IStrategyMapService, StrategyMapService>();
        services.AddTransient<IQuestionnaireServiceV2, QuestionnaireServiceV2>();
        services.AddTransient<IFinancialBenchmarkService, FinancialBenchmarkService>();
        services.AddTransient<IVaultShareService, VaultShareService>();

        // Subscription service
        services.AddScoped<Sqordia.Application.Services.ISubscriptionService, SubscriptionService>();
        
        // Stripe service
        services.AddScoped<Sqordia.Application.Services.IStripeService, StripeService>();

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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Localization;
using Azure.Storage.Blobs;
using Resend;
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
using Sqordia.Infrastructure.Services.QualityAgents;
using Sqordia.Infrastructure.Identity;
using Sqordia.Infrastructure.Localization;
using Sqordia.Infrastructure.Settings;
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
        // Priority: Resend > MockEmailService
        // Check if we're in a test environment
        var isTestEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Testing"
                             || Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Test"
                             || configuration["ASPNETCORE_ENVIRONMENT"] == "Testing"
                             || configuration["ASPNETCORE_ENVIRONMENT"] == "Test"
                             || Environment.GetEnvironmentVariable("SKIP_AZURE_SERVICES") == "true"
                             || configuration["SkipAzureServices"] == "true";

        bool emailServiceRegistered = false;

        // Try Resend first (preferred)
        var resendApiKey = Environment.GetEnvironmentVariable("RESEND_API_KEY")
                        ?? Environment.GetEnvironmentVariable("Resend__ApiKey")
                        ?? configuration["Resend:ApiKey"];

        var resendFromEmail = Environment.GetEnvironmentVariable("RESEND_FROM_EMAIL")
                           ?? Environment.GetEnvironmentVariable("Resend__FromEmail")
                           ?? configuration["Resend:FromEmail"]
                           ?? Environment.GetEnvironmentVariable("Email__FromAddress")
                           ?? configuration["Email:FromAddress"];

        var resendFromName = Environment.GetEnvironmentVariable("RESEND_FROM_NAME")
                          ?? Environment.GetEnvironmentVariable("Resend__FromName")
                          ?? configuration["Resend:FromName"]
                          ?? "Sqordia";

        // Only initialize Resend if not in test environment and API key is available
        if (!isTestEnvironment && !string.IsNullOrEmpty(resendApiKey) && !string.IsNullOrEmpty(resendFromEmail))
        {
            try
            {
                var frontendBaseUrl = Environment.GetEnvironmentVariable("FRONTEND_BASE_URL")
                                   ?? configuration["Frontend:BaseUrl"]
                                   ?? "https://sqordia.app";

                var resendSettings = new ResendEmailSettings
                {
                    ApiKey = resendApiKey,
                    FromEmail = resendFromEmail,
                    FromName = resendFromName,
                    FrontendBaseUrl = frontendBaseUrl
                };

                services.Configure<ResendEmailSettings>(options =>
                {
                    options.ApiKey = resendSettings.ApiKey;
                    options.FromEmail = resendSettings.FromEmail;
                    options.FromName = resendSettings.FromName;
                    options.FrontendBaseUrl = resendSettings.FrontendBaseUrl;
                });

                // Register Resend client
                services.AddOptions();
                services.AddHttpClient<IResend, ResendClient>();
                services.Configure<ResendClientOptions>(o =>
                {
                    o.ApiToken = resendApiKey;
                });

                services.AddTransient<IEmailService>(sp =>
                    new ResendEmailService(
                        sp.GetRequiredService<IResend>(),
                        sp.GetRequiredService<IOptions<ResendEmailSettings>>(),
                        sp.GetRequiredService<ILogger<ResendEmailService>>(),
                        sp.GetRequiredService<ILocalizationService>(),
                        sp.GetRequiredService<IStringLocalizerFactory>()));

                emailServiceRegistered = true;
            }
            catch (Exception ex) when (ex.Message.Contains("connection", StringComparison.OrdinalIgnoreCase)
                                   || ex.Message.Contains("authentication", StringComparison.OrdinalIgnoreCase)
                                   || ex is InvalidOperationException)
            {
                // If Resend is not available, fall back to mock
            }
        }

        // Use mock email service for tests or when Resend is not configured
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
            // Clear placeholder values that may have been bound by Configure()
            if (IsPlaceholder(options.ApiKey))
            {
                options.ApiKey = string.Empty;
            }

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
            // Clear placeholder values that may have been bound by Configure()
            if (IsPlaceholder(options.ApiKey))
            {
                options.ApiKey = string.Empty;
            }

            var envApiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY")
                          ?? Environment.GetEnvironmentVariable("CLAUDE_API_KEY")
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
            // Clear placeholder values that may have been bound by Configure()
            if (IsPlaceholder(options.ApiKey))
            {
                options.ApiKey = string.Empty;
            }

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

        // AI key resolver — single source of truth for API keys (DB > env vars)
        services.AddSingleton<IAIKeyResolver, AIKeyResolver>();

        // Register AI provider factory
        services.AddSingleton<IAIProviderFactory, AIProviderFactory>();

        // Register the main IAIService as the fallback wrapper (replaces direct OpenAIService registration)
        services.AddSingleton<IAIService, AIProviderWithFallback>();

        // Model registry for runtime model switching via DB settings
        services.AddScoped<IModelRegistryService, ModelRegistryService>();

        // Structured extraction using Claude tool_use (SWOT, financials, risks)
        services.AddSingleton<IStructuredExtractionService, StructuredExtractionService>();

        // Document creation agent using Claude tool_use (Word/PPTX/Excel blueprints)
        services.AddSingleton<IDocumentAgentService, DocumentAgentService>();

        // Generation tool service for tool-augmented AI generation
        services.AddScoped<IGenerationToolService, GenerationToolService>();

        // Quality Agents for post-generation analysis
        services.AddScoped<IQualityAgent, WritingQualityAgent>();
        services.AddScoped<IQualityAgent, FinancialConsistencyAgent>();
        services.AddScoped<IQualityAgent, ComplianceAgent>();
        services.AddScoped<IQualityAgent, BankReadinessAgent>();
        services.AddScoped<IQualityAgentOrchestrator, QualityAgentOrchestrator>();

        // Python AI Microservice (LangChain, MLflow, RAGAS)
        services.Configure<PythonServiceSettings>(configuration.GetSection("AI:PythonService"));
        services.PostConfigure<PythonServiceSettings>(options =>
        {
            var envBaseUrl = Environment.GetEnvironmentVariable("AI__PythonService__BaseUrl");
            if (!string.IsNullOrEmpty(envBaseUrl))
            {
                options.BaseUrl = envBaseUrl;
            }
            var envServiceKey = Environment.GetEnvironmentVariable("AI__PythonService__ServiceKey");
            if (!string.IsNullOrEmpty(envServiceKey))
            {
                options.ServiceKey = envServiceKey;
            }
        });
        services.AddHttpClient<IAIPythonService, AIPythonServiceClient>((sp, client) =>
        {
            var pythonSettings = sp.GetRequiredService<IOptions<PythonServiceSettings>>().Value;
            client.BaseAddress = new Uri(pythonSettings.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(pythonSettings.GenerationTimeoutSeconds);
            if (!string.IsNullOrEmpty(pythonSettings.ServiceKey))
            {
                client.DefaultRequestHeaders.Add("X-Service-Key", pythonSettings.ServiceKey);
            }
        });

        // Register Prompt Improvement service for AI-powered prompt optimization
        services.AddHttpClient("Anthropic");
        services.AddScoped<IPromptImprovementService, PromptImprovementService>();

        // Document export service - Required for PDF/Word export
        services.AddTransient<IDocumentExportService, DocumentExportService>();

        // Themed PDF service — Puppeteer-based, selectable text, matches frontend preview
        services.AddSingleton<IHtmlToPdfRenderer, Sqordia.Infrastructure.Services.DocumentExport.PuppeteerPdfRenderer>();
        services.AddScoped<IThemedPdfService, Sqordia.Infrastructure.Services.DocumentExport.ThemedPdfService>();

        // AI content adaptation service — adapts content for PDF/Word/PowerPoint formats
        services.AddScoped<IContentAdaptationService, Sqordia.Infrastructure.Services.ContentAdaptation.ContentAdaptationService>();

        // Slide deck service - Required for PowerPoint slide deck generation
        services.AddScoped<ISlideDeckService, SlideDeckService>();

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

        // AI Coach service - Required for multi-turn conversational coaching
        services.AddScoped<IAICoachService, AICoachService>();
        
        // Currency conversion service - Required for multi-currency support
        services.AddTransient<ICurrencyConversionService, CurrencyConversionService>();
        
        // Pricing analysis service - Required for pricing and market analysis
        services.AddTransient<IPricingAnalysisService, PricingAnalysisService>();
        
        // SMART objectives service - Required for SMART objectives generation
        services.AddTransient<ISmartObjectiveService, SmartObjectiveService>();
        
        // Plan comment service - Required for collaborative comments
        services.AddTransient<IPlanCommentService, PlanCommentService>();
        
        // CMS Versioning services
        services.AddTransient<ICmsVersionService, CmsVersionService>();
        services.AddTransient<ICmsContentBlockService, CmsContentBlockService>();
        services.AddTransient<ICmsAssetService, CmsAssetService>();
        services.AddTransient<IPublishedContentService, PublishedContentService>();
        services.AddTransient<ICmsApprovalService, CmsApprovalService>();
        services.AddTransient<ICmsDiffService, CmsDiffService>();
        services.AddTransient<ICmsRegistryService, CmsRegistryService>();
        services.AddTransient<ICmsTemplateService, CmsTemplateService>();

        // AI Telemetry service - observability for AI calls
        // ML Data Collection & Prediction
        services.AddScoped<IMLDataCollector, MLDataCollector>();
        services.AddScoped<IAITelemetryService, AITelemetryService>();
        services.AddHttpClient<IMLPredictionService, MLPredictionService>((sp, client) =>
        {
            var pythonSettings = sp.GetRequiredService<IOptions<PythonServiceSettings>>().Value;
            client.BaseAddress = new Uri(pythonSettings.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(pythonSettings.EvaluationTimeoutSeconds);
            if (!string.IsNullOrEmpty(pythonSettings.ServiceKey))
            {
                client.DefaultRequestHeaders.Add("X-Service-Key", pythonSettings.ServiceKey);
            }
        });

        // Bug Report service
        services.AddScoped<IBugReportService, BugReportService>();

        // GitHub Issue service - for creating GitHub issues from admin panel
        services.Configure<GitHubSettings>(configuration.GetSection("GitHub"));
        services.PostConfigure<GitHubSettings>(options =>
        {
            var envToken = Environment.GetEnvironmentVariable("GitHub__PersonalAccessToken")
                        ?? Environment.GetEnvironmentVariable("GITHUB_TOKEN");
            if (!string.IsNullOrEmpty(envToken) && !IsPlaceholder(envToken))
            {
                options.PersonalAccessToken = envToken;
            }

            var envFrontendOwner = Environment.GetEnvironmentVariable("GitHub__FrontendRepoOwner");
            if (!string.IsNullOrEmpty(envFrontendOwner))
            {
                options.FrontendRepoOwner = envFrontendOwner;
            }

            var envFrontendRepo = Environment.GetEnvironmentVariable("GitHub__FrontendRepoName");
            if (!string.IsNullOrEmpty(envFrontendRepo))
            {
                options.FrontendRepoName = envFrontendRepo;
            }

            var envBackendOwner = Environment.GetEnvironmentVariable("GitHub__BackendRepoOwner");
            if (!string.IsNullOrEmpty(envBackendOwner))
            {
                options.BackendRepoOwner = envBackendOwner;
            }

            var envBackendRepo = Environment.GetEnvironmentVariable("GitHub__BackendRepoName");
            if (!string.IsNullOrEmpty(envBackendRepo))
            {
                options.BackendRepoName = envBackendRepo;
            }
        });
        services.AddHttpClient<IGitHubIssueService, GitHubIssueService>();

        // V2 Services - Growth Architect Intelligence Layer
        services.AddTransient<IAuditService, AuditService>();
        services.AddTransient<IReadinessScoreService, ReadinessScoreService>();
        services.AddTransient<IQuestionPolishService, QuestionPolishService>();
        services.AddTransient<IStrategyMapService, StrategyMapService>();
        services.AddTransient<IQuestionnaireServiceV2, QuestionnaireServiceV2>();
        services.AddTransient<IVaultShareService, VaultShareService>();

        // Subscription service
        services.AddScoped<Sqordia.Application.Services.ISubscriptionService, SubscriptionService>();

        // Feature gate service
        services.AddScoped<IFeatureGateService, FeatureGateService>();

        // Subscription intelligence service (ML-driven engagement, churn, promotions)
        services.AddHttpClient("AIService");
        services.AddScoped<ISubscriptionIntelligenceService, SubscriptionIntelligenceService>();

        // Stripe service — set API key once at startup to avoid race conditions
        var stripeSecretKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY")
                           ?? Environment.GetEnvironmentVariable("Stripe__SecretKey")
                           ?? configuration["Stripe:SecretKey"];
        if (!string.IsNullOrEmpty(stripeSecretKey) && !IsPlaceholder(stripeSecretKey))
        {
            Stripe.StripeConfiguration.ApiKey = stripeSecretKey;
        }
        services.AddScoped<Sqordia.Application.Services.IStripeService, StripeService>();

        // Invoice PDF service
        services.AddScoped<Sqordia.Application.Services.IInvoicePdfService, InvoicePdfService>();

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
                                   || ex is InvalidOperationException
                                   || ex is FormatException)
            {
                // If connection is not available or invalid, use in-memory storage service
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

        // Organization membership cache - high-frequency permission checks
        services.AddScoped<IOrganizationMembershipCache, OrganizationMembershipCacheService>();

        // Admin AI Assistant - tool-use agentic assistant for admin queries
        services.AddScoped<IAdminAIAssistantService, AdminAIAssistantService>();

        // Email Template service - CRUD + AI generation + rendering
        services.AddScoped<IEmailTemplateService, EmailTemplateService>();

        // Analytics Batch service - AI-powered insights pipeline
        services.AddScoped<IAnalyticsBatchService, AnalyticsBatchService>();

        // CMS AI Content service - AI content generation for CMS
        services.AddScoped<ICmsAiContentService, CmsAiContentService>();

        return services;
    }
}

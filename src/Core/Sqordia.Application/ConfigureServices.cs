using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Financial.Services.Previsio;
using Sqordia.Application.Services;
using Sqordia.Application.Services.Implementations;
using Sqordia.Application.Services.Implementations.Cms;
using Sqordia.Application.Services.Implementations.Financial;
using Sqordia.Application.OBNL.Services;
using Sqordia.Application.Services.Cms;
using Sqordia.Application.Services.Questionnaire;
using Sqordia.Application.Services.V3;
using Sqordia.Application.Services.V3.Implementations;
using System.Reflection;

namespace Sqordia.Application;

public static class ConfigureServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register AutoMapper
        services.AddAutoMapper(typeof(ConfigureServices).Assembly);

        // Register FluentValidation
        services.AddValidatorsFromAssembly(typeof(ConfigureServices).Assembly);

        // Register Application Services
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IUserProfileService, UserProfileService>();
            services.AddScoped<IPrivacyService, PrivacyService>();
            services.AddScoped<IRoleManagementService, RoleManagementService>();
            services.AddScoped<ITwoFactorService, TwoFactorService>();
            services.AddScoped<ISecurityManagementService, SecurityManagementService>();
            services.AddScoped<IActivityLogService, ActivityLogService>();
            services.AddScoped<IOrganizationService, OrganizationService>();

            // Business Plan services
            services.AddScoped<IBusinessPlanService, BusinessPlanService>();
            services.AddScoped<IQuestionnaireService, QuestionnaireService>();
            services.AddScoped<IQuestionnaireContextService, QuestionnaireContextService>();
            services.AddScoped<IBusinessBriefService, BusinessBriefService>();
            services.AddScoped<IGenerationPipelineService, GenerationPipelineService>();
            services.AddScoped<IBusinessPlanGenerationService, BusinessPlanGenerationService>();
            services.AddScoped<ISectionService, SectionService>();
            services.AddScoped<IBusinessPlanShareService, BusinessPlanShareService>();
            services.AddScoped<IBusinessPlanVersionService, BusinessPlanVersionService>();
            services.AddScoped<ICoverPageService, CoverPageService>();
            services.AddScoped<ITableOfContentsService, TableOfContentsService>();

            // OBNL services
            services.AddScoped<IOBNLPlanService, OBNLPlanService>();

            // Current user service
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            // Authorization helper for common permission checks
            services.AddScoped<IAuthorizationHelper, AuthorizationHelper>();

            // AI Prompt management service
            services.AddScoped<IAIPromptService, AIPromptService>();
            services.AddScoped<IPromptMigrationService, PromptMigrationService>();
            services.AddScoped<IPromptSelectorService, PromptSelectorService>();

            // Prompt Registry service (admin management)
            services.AddScoped<IPromptRegistryService, PromptRegistryService>();

            // Admin Question Template management service
            services.AddScoped<IAdminQuestionTemplateService, AdminQuestionTemplateService>();

            // Questionnaire versioning service
            services.AddScoped<IQuestionnaireVersionService, QuestionnaireVersionService>();

            // Enhanced content generation service (with visual elements)
            services.AddScoped<IEnhancedContentGenerationService, EnhancedContentGenerationService>();

            // Settings service
            services.AddScoped<ISettingsService, SettingsService>();

            // Feature flags service
            services.AddScoped<IFeatureFlagsService, FeatureFlagsService>();

            // Export theme service
            services.AddScoped<IExportThemeService, ExportThemeService>();

            // OAuth service
            services.AddScoped<IOAuthService, OAuthService>();

            // Onboarding service
            services.AddScoped<IOnboardingService, OnboardingService>();

            // Adaptive Interview service
            services.AddScoped<IConditionalLogicEvaluator, ConditionalLogicEvaluator>();
            services.AddScoped<IAdaptiveInterviewService, AdaptiveInterviewService>();

            // CMS services
            services.AddScoped<ICmsRegistryService, CmsRegistryService>();
            services.AddScoped<ICmsApprovalService, CmsApprovalService>();
            services.AddScoped<ICmsTemplateService, CmsTemplateService>();

            // V3 Structure Finale services
            services.AddScoped<ISectionHierarchyService, SectionHierarchyService>();
            services.AddScoped<ISectionPromptService, SectionPromptService>();
            services.AddScoped<IQuestionnaireServiceV3, QuestionnaireServiceV3>();
            services.AddScoped<IQuestionSectionMappingService, QuestionSectionMappingService>();

            // Notification services
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<INotificationPreferenceService, NotificationPreferenceService>();
            services.AddScoped<INotificationAnalyticsService, NotificationAnalyticsService>();

            // Newsletter service
            services.AddScoped<INewsletterSubscriberService, NewsletterSubscriberService>();

            // Previsio Financial Projections services
            services.AddScoped<IFinancialForecastingService, FinancialForecastingService>();
            services.AddScoped<IFinancialPlanService, FinancialPlanServiceImpl>();
            services.AddScoped<ISalesModuleService, SalesModuleServiceImpl>();
            services.AddScoped<ICOGSModuleService, COGSModuleServiceImpl>();
            services.AddScoped<IPayrollModuleService, PayrollModuleServiceImpl>();
            services.AddScoped<IExpenseModuleService, ExpenseModuleServiceImpl>();
            services.AddScoped<ICapexModuleService, CapexModuleServiceImpl>();
            services.AddScoped<IFinancingModuleService, FinancingModuleServiceImpl>();
            services.AddScoped<IProjectCostService, ProjectCostServiceImpl>();
            services.AddScoped<IFinancialStatementsService, FinancialStatementsServiceImpl>();

            return services;
    }
}

using Microsoft.EntityFrameworkCore;
using Sqordia.Domain.Entities;
using Sqordia.Domain.Entities.AICoach;
using Sqordia.Domain.Entities.BusinessPlan;
using Sqordia.Domain.Entities.Cms;
using Sqordia.Domain.Entities.Financial;
using Sqordia.Domain.Entities.Identity;
using Sqordia.Domain.Entities.ML;

namespace Sqordia.Application.Common.Interfaces;

/// <summary>
/// Database context interface for authentication-related entities
/// </summary>
public interface IApplicationDbContext
{
    // User management
    DbSet<User> Users { get; }
    
    // Role and Permission management
    DbSet<Role> Roles { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<RolePermission> RolePermissions { get; }
    
    // Token management
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<EmailVerificationToken> EmailVerificationTokens { get; }
    DbSet<PasswordResetToken> PasswordResetTokens { get; }
    
    // Two-Factor Authentication
    DbSet<TwoFactorAuth> TwoFactorAuths { get; }
    
    // Security and Session Management
    DbSet<LoginHistory> LoginHistories { get; }
    DbSet<ActiveSession> ActiveSessions { get; }

    // Privacy and Consent Management (Quebec Bill 25)
    DbSet<UserConsent> UserConsents { get; }

    // Organization Management
    DbSet<Organization> Organizations { get; }
    DbSet<OrganizationMember> OrganizationMembers { get; }
    DbSet<OrganizationInvitation> OrganizationInvitations { get; }
    
    // Business Plan Management
    DbSet<Domain.Entities.BusinessPlan.BusinessPlan> BusinessPlans { get; }
    DbSet<QuestionnaireResponse> QuestionnaireResponses { get; }
    DbSet<Domain.Entities.BusinessPlan.FinancialProjection> BusinessPlanFinancialProjections { get; }
    DbSet<BusinessPlanShare> BusinessPlanShares { get; }
    DbSet<BusinessPlanVersion> BusinessPlanVersions { get; }
    DbSet<SmartObjective> SmartObjectives { get; }
    DbSet<PlanSectionComment> PlanSectionComments { get; }
    DbSet<QuestionnaireVersion> QuestionnaireVersions { get; }
    DbSet<QuestionnaireStep> QuestionnaireSteps { get; }
    DbSet<CoverPageSettings> CoverPageSettings { get; }
    DbSet<TableOfContentsSettings> TableOfContentsSettings { get; }

    // OBNL Management
    DbSet<OBNLBusinessPlan> OBNLBusinessPlans { get; }
    DbSet<OBNLCompliance> OBNLCompliances { get; }
    DbSet<GrantApplication> GrantApplications { get; }
    DbSet<ImpactMeasurement> ImpactMeasurements { get; }
    
    // Financial Management
    DbSet<Currency> Currencies { get; }
    DbSet<ExchangeRate> ExchangeRates { get; }
    DbSet<TaxRule> TaxRules { get; }
    DbSet<Domain.Entities.FinancialProjectionItem> FinancialProjectionItems { get; }
    DbSet<TaxCalculation> TaxCalculations { get; }
    DbSet<FinancialKPI> FinancialKPIs { get; }
    DbSet<InvestmentAnalysis> InvestmentAnalyses { get; }
    DbSet<LocationOverheadRate> LocationOverheadRates { get; }
    DbSet<FinancialCell> FinancialCells { get; }

    // Previsio Financial Projections
    DbSet<FinancialPlan> FinancialPlansPrevisio { get; }
    DbSet<SalesProduct> SalesProducts { get; }
    DbSet<SalesVolume> SalesVolumes { get; }
    DbSet<CostOfGoodsSoldItem> CostOfGoodsSoldItems { get; }
    DbSet<PayrollItem> PayrollItems { get; }
    DbSet<SalesExpenseItem> SalesExpenseItems { get; }
    DbSet<AdminExpenseItem> AdminExpenseItems { get; }
    DbSet<CapexAsset> CapexAssets { get; }
    DbSet<ProjectCost> ProjectCosts { get; }
    DbSet<FinancingSource> FinancingSources { get; }
    DbSet<AmortizationEntry> AmortizationEntries { get; }

    // AI Prompt Management
    DbSet<AIPrompt> AIPrompts { get; }
    DbSet<AIPromptVersion> AIPromptVersions { get; }
    DbSet<PromptTemplate> PromptTemplates { get; }
    DbSet<PromptPerformance> PromptPerformance { get; }

    // Structure Finale - Section Hierarchy & V3 Questionnaire
    DbSet<MainSection> MainSections { get; }
    DbSet<SubSection> SubSections { get; }
    DbSet<SectionPrompt> SectionPrompts { get; }
    DbSet<QuestionTemplate> QuestionTemplates { get; }
    DbSet<QuestionSectionMapping> QuestionSectionMappings { get; }
    
    // Audit logging
    DbSet<AuditLog> AuditLogs { get; }
    
    // Subscription Management
    DbSet<SubscriptionPlan> SubscriptionPlans { get; }
    DbSet<Subscription> Subscriptions { get; }
    DbSet<PlanFeatureLimit> PlanFeatureLimits { get; }
    DbSet<OrganizationUsage> OrganizationUsages { get; }
    DbSet<Coupon> Coupons { get; }
    DbSet<CouponRedemption> CouponRedemptions { get; }
    
    // Application Settings
    DbSet<Settings> Settings { get; }
    
    // CMS Versioning
    DbSet<CmsVersion> CmsVersions { get; }
    DbSet<CmsContentBlock> CmsContentBlocks { get; }
    DbSet<CmsAsset> CmsAssets { get; }
    DbSet<CmsVersionHistory> CmsVersionHistory { get; }

    // CMS Registry (Page/Section/Block definitions)
    DbSet<CmsPage> CmsPages { get; }
    DbSet<CmsSection> CmsSections { get; }
    DbSet<CmsBlockDefinition> CmsBlockDefinitions { get; }

    // CMS Content Templates
    DbSet<CmsContentTemplate> CmsContentTemplates { get; }

    // Bug Reports
    DbSet<BugReport> BugReports { get; }
    DbSet<BugReportAttachment> BugReportAttachments { get; }

    // AI Coach
    DbSet<AICoachConversation> AICoachConversations { get; }
    DbSet<AICoachMessage> AICoachMessages { get; }
    DbSet<AICoachUsage> AICoachUsages { get; }

    // Email Templates
    DbSet<EmailTemplate> EmailTemplates { get; }

    // Analytics Insights
    DbSet<AnalyticsInsight> AnalyticsInsights { get; }

    // Notifications
    DbSet<Notification> Notifications { get; }
    DbSet<NotificationPreference> NotificationPreferences { get; }

    // ML Training Data
    DbSet<AICallTelemetryRecord> AICallTelemetryRecords { get; }
    DbSet<SectionEditHistory> SectionEditHistories { get; }
    DbSet<LearnedPreference> LearnedPreferences { get; }

    // Newsletter
    DbSet<NewsletterSubscriber> NewsletterSubscribers { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default);
}

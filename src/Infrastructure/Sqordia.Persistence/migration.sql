CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE TABLE "AIPrompts" (
        "Id" uuid NOT NULL,
        "Name" text NOT NULL,
        "Description" text NOT NULL,
        "Category" text NOT NULL,
        "PlanType" text NOT NULL,
        "Language" text NOT NULL,
        "SystemPrompt" text NOT NULL,
        "UserPromptTemplate" text NOT NULL,
        "Variables" text NOT NULL,
        "IsActive" boolean NOT NULL,
        "Version" integer NOT NULL,
        "ParentPromptId" text,
        "Notes" text,
        "UsageCount" integer NOT NULL,
        "LastUsedAt" timestamp with time zone,
        "AverageRating" double precision NOT NULL,
        "RatingCount" integer NOT NULL,
        "Created" timestamp with time zone NOT NULL,
        "CreatedBy" text,
        "LastModified" timestamp with time zone,
        "LastModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "DeletedBy" text,
        CONSTRAINT "PK_AIPrompts" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE TABLE "Currencies" (
        "Id" uuid NOT NULL,
        "Code" character varying(10) NOT NULL,
        "Name" character varying(100) NOT NULL,
        "Symbol" character varying(10) NOT NULL,
        "Country" character varying(100) NOT NULL,
        "Region" character varying(100) NOT NULL,
        "IsActive" boolean NOT NULL,
        "DecimalPlaces" integer NOT NULL,
        "ExchangeRate" numeric(18,6) NOT NULL,
        "LastUpdated" timestamp with time zone NOT NULL,
        "Source" character varying(100) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "CreatedBy" text NOT NULL,
        "UpdatedBy" text NOT NULL,
        CONSTRAINT "PK_Currencies" PRIMARY KEY ("Id"),
        CONSTRAINT "AK_Currencies_Code" UNIQUE ("Code")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE TABLE "Organizations" (
        "Id" uuid NOT NULL,
        "Name" character varying(200) NOT NULL,
        "Description" character varying(1000),
        "Website" character varying(500),
        "LogoUrl" character varying(500),
        "OrganizationType" character varying(50) NOT NULL,
        "IsActive" boolean NOT NULL,
        "DeactivatedAt" timestamp with time zone,
        "MaxMembers" integer NOT NULL DEFAULT 10,
        "AllowMemberInvites" boolean NOT NULL DEFAULT TRUE,
        "RequireEmailVerification" boolean NOT NULL DEFAULT TRUE,
        "Created" timestamp with time zone NOT NULL,
        "CreatedBy" text,
        "LastModified" timestamp with time zone,
        "LastModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "DeletedBy" text,
        CONSTRAINT "PK_Organizations" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE TABLE "Permissions" (
        "Id" uuid NOT NULL,
        "Name" character varying(100) NOT NULL,
        "Description" character varying(500) NOT NULL,
        "Category" character varying(50) NOT NULL,
        CONSTRAINT "PK_Permissions" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE TABLE "QuestionnaireTemplates" (
        "Id" uuid NOT NULL,
        "Name" character varying(200) NOT NULL,
        "Description" character varying(1000),
        "PlanType" character varying(50) NOT NULL,
        "IsActive" boolean NOT NULL DEFAULT TRUE,
        "Version" integer NOT NULL DEFAULT 1,
        "Created" timestamp with time zone NOT NULL,
        "CreatedBy" text,
        "LastModified" timestamp with time zone,
        "LastModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "DeletedBy" text,
        CONSTRAINT "PK_QuestionnaireTemplates" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE TABLE "Roles" (
        "Id" uuid NOT NULL,
        "Name" character varying(100) NOT NULL,
        "Description" character varying(500) NOT NULL,
        "IsSystemRole" boolean NOT NULL,
        CONSTRAINT "PK_Roles" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE TABLE "Settings" (
        "Id" uuid NOT NULL,
        "Key" character varying(255) NOT NULL,
        "Value" character varying(8000) NOT NULL,
        "Category" character varying(100) NOT NULL DEFAULT '',
        "Description" character varying(500),
        "IsPublic" boolean NOT NULL DEFAULT FALSE,
        "SettingType" integer NOT NULL DEFAULT 1,
        "DataType" integer NOT NULL DEFAULT 1,
        "IsEncrypted" boolean NOT NULL DEFAULT FALSE,
        "CacheDurationMinutes" integer,
        "IsCritical" boolean NOT NULL DEFAULT FALSE,
        "Created" timestamp with time zone NOT NULL,
        "CreatedBy" text,
        "LastModified" timestamp with time zone,
        "LastModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "DeletedBy" text,
        CONSTRAINT "PK_Settings" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE TABLE "SubscriptionPlans" (
        "Id" uuid NOT NULL,
        "PlanType" character varying(50) NOT NULL,
        "Name" character varying(100) NOT NULL,
        "Description" character varying(500) NOT NULL,
        "Price" numeric(18,2) NOT NULL,
        "Currency" character varying(10) NOT NULL DEFAULT 'CAD',
        "BillingCycle" character varying(20) NOT NULL,
        "MaxUsers" integer NOT NULL,
        "MaxBusinessPlans" integer NOT NULL,
        "MaxStorageGB" integer NOT NULL,
        "Features" text NOT NULL DEFAULT '[]',
        "IsActive" boolean NOT NULL DEFAULT TRUE,
        "Created" timestamp with time zone NOT NULL,
        "CreatedBy" text,
        "LastModified" timestamp with time zone,
        "LastModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "DeletedBy" text,
        CONSTRAINT "PK_SubscriptionPlans" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE TABLE "Templates" (
        "Id" uuid NOT NULL,
        "Name" character varying(200) NOT NULL,
        "Description" character varying(1000) NOT NULL,
        "Content" text NOT NULL,
        "Category" integer NOT NULL,
        "Type" integer NOT NULL,
        "Status" integer NOT NULL,
        "Industry" character varying(100) NOT NULL,
        "TargetAudience" character varying(200) NOT NULL,
        "Language" character varying(10) NOT NULL,
        "Country" character varying(100) NOT NULL,
        "IsPublic" boolean NOT NULL,
        "IsDefault" boolean NOT NULL,
        "UsageCount" integer NOT NULL,
        "Rating" numeric(3,2) NOT NULL,
        "RatingCount" integer NOT NULL,
        "Tags" text NOT NULL,
        "PreviewImage" text NOT NULL,
        "Author" character varying(200) NOT NULL,
        "AuthorEmail" character varying(256) NOT NULL,
        "Version" character varying(50) NOT NULL,
        "Changelog" text NOT NULL,
        "LastUsed" timestamp with time zone NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "CreatedBy" text NOT NULL,
        "UpdatedBy" text NOT NULL,
        CONSTRAINT "PK_Templates" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE TABLE "Users" (
        "Id" uuid NOT NULL,
        "FirstName" character varying(100) NOT NULL,
        "LastName" character varying(100) NOT NULL,
        "Email" character varying(256) NOT NULL,
        "UserName" character varying(256) NOT NULL,
        "PasswordHash" text NOT NULL,
        "IsEmailConfirmed" boolean NOT NULL,
        "EmailConfirmedAt" timestamp with time zone,
        "IsActive" boolean NOT NULL,
        "LastLoginAt" timestamp with time zone,
        "AccessFailedCount" integer NOT NULL,
        "LockoutEnd" timestamp with time zone,
        "LockoutEnabled" boolean NOT NULL,
        "PhoneNumber" text,
        "PhoneNumberVerified" boolean NOT NULL,
        "ProfilePictureUrl" text,
        "PasswordLastChangedAt" timestamp with time zone,
        "RequirePasswordChange" boolean NOT NULL,
        "UserType" character varying(50) NOT NULL,
        "GoogleId" character varying(100),
        "Provider" character varying(50) NOT NULL DEFAULT 'local',
        "Created" timestamp with time zone NOT NULL,
        "CreatedBy" text,
        "LastModified" timestamp with time zone,
        "LastModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "DeletedBy" text,
        CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE TABLE "ExchangeRates" (
        "Id" uuid NOT NULL,
        "FromCurrencyId" uuid NOT NULL,
        "ToCurrencyId" uuid NOT NULL,
        "Rate" numeric(18,6) NOT NULL,
        "InverseRate" numeric(18,6) NOT NULL,
        "EffectiveDate" timestamp with time zone NOT NULL,
        "ExpiryDate" timestamp with time zone NOT NULL,
        "Source" character varying(100) NOT NULL,
        "IsActive" boolean NOT NULL,
        "Provider" character varying(100) NOT NULL,
        "Spread" numeric(18,6) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "CreatedBy" text NOT NULL,
        "UpdatedBy" text NOT NULL,
        CONSTRAINT "PK_ExchangeRates" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ExchangeRates_Currencies_FromCurrencyId" FOREIGN KEY ("FromCurrencyId") REFERENCES "Currencies" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_ExchangeRates_Currencies_ToCurrencyId" FOREIGN KEY ("ToCurrencyId") REFERENCES "Currencies" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE TABLE "TaxRules" (
        "Id" uuid NOT NULL,
        "Name" character varying(200) NOT NULL,
        "Description" character varying(1000) NOT NULL,
        "Country" character varying(100) NOT NULL,
        "Region" character varying(100) NOT NULL,
        "TaxType" character varying(50) NOT NULL,
        "Rate" numeric(5,2) NOT NULL,
        "MinAmount" numeric(18,2) NOT NULL,
        "MaxAmount" numeric(18,2) NOT NULL,
        "IsPercentage" boolean NOT NULL,
        "CalculationMethod" character varying(50) NOT NULL,
        "ApplicableTo" character varying(200) NOT NULL,
        "IsActive" boolean NOT NULL,
        "EffectiveDate" timestamp with time zone NOT NULL,
        "ExpiryDate" timestamp with time zone,
        "CurrencyCode" character varying(10) NOT NULL,
        "LegalReference" character varying(200) NOT NULL,
        "Notes" text NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "CreatedBy" text NOT NULL,
        "UpdatedBy" text NOT NULL,
        CONSTRAINT "PK_TaxRules" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_TaxRules_Currencies_CurrencyCode" FOREIGN KEY ("CurrencyCode") REFERENCES "Currencies" ("Code") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE TABLE "BusinessPlans" (
        "Id" uuid NOT NULL,
        "Title" character varying(200) NOT NULL,
        "Description" character varying(1000),
        "PlanType" character varying(50) NOT NULL,
        "Status" character varying(50) NOT NULL,
        "OrganizationId" uuid NOT NULL,
        "Version" integer NOT NULL,
        "TotalQuestions" integer NOT NULL,
        "CompletedQuestions" integer NOT NULL,
        "CompletionPercentage" numeric(5,2) NOT NULL,
        "QuestionnaireCompletedAt" timestamp with time zone,
        "GenerationStartedAt" timestamp with time zone,
        "GenerationCompletedAt" timestamp with time zone,
        "GenerationModel" character varying(100),
        "ExecutiveSummary" text,
        "ProblemStatement" text,
        "Solution" text,
        "MarketAnalysis" text,
        "CompetitiveAnalysis" text,
        "SwotAnalysis" text,
        "BusinessModel" text,
        "MarketingStrategy" text,
        "BrandingStrategy" text,
        "OperationsPlan" text,
        "ManagementTeam" text,
        "FinancialProjections" text,
        "FundingRequirements" text,
        "RiskAnalysis" text,
        "ExitStrategy" text,
        "AppendixData" text,
        "MissionStatement" text,
        "SocialImpact" text,
        "BeneficiaryProfile" text,
        "GrantStrategy" text,
        "SustainabilityPlan" text,
        "FinalizedAt" timestamp with time zone,
        "ArchivedAt" timestamp with time zone,
        "IsTemplate" boolean NOT NULL,
        "Created" timestamp with time zone NOT NULL,
        "CreatedBy" text,
        "LastModified" timestamp with time zone,
        "LastModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "DeletedBy" text,
        CONSTRAINT "PK_BusinessPlans" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_BusinessPlans_Organizations_OrganizationId" FOREIGN KEY ("OrganizationId") REFERENCES "Organizations" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE TABLE "OBNLBusinessPlans" (
        "Id" uuid NOT NULL,
        "OrganizationId" uuid NOT NULL,
        "OBNLType" character varying(50) NOT NULL,
        "Mission" text NOT NULL,
        "Vision" text NOT NULL,
        "Values" text NOT NULL,
        "FundingRequirements" numeric(18,2) NOT NULL,
        "FundingPurpose" text NOT NULL,
        "ComplianceStatus" character varying(50) NOT NULL,
        "ComplianceLevel" character varying(50) NOT NULL,
        "ComplianceLastUpdated" timestamp with time zone NOT NULL,
        "ComplianceNotes" character varying(500) NOT NULL,
        "LegalStructure" character varying(100) NOT NULL,
        "RegistrationNumber" character varying(100) NOT NULL,
        "RegistrationDate" timestamp with time zone NOT NULL,
        "GoverningBody" character varying(200) NOT NULL,
        "BoardComposition" text NOT NULL,
        "StakeholderEngagement" text NOT NULL,
        "ImpactMeasurement" text NOT NULL,
        "SustainabilityStrategy" text NOT NULL,
        "GrantApplications" text NOT NULL,
        "ReportingRequirements" text NOT NULL,
        "RiskManagement" text NOT NULL,
        "SuccessMetrics" text NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "CreatedBy" text NOT NULL,
        "UpdatedBy" text NOT NULL,
        CONSTRAINT "PK_OBNLBusinessPlans" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_OBNLBusinessPlans_Organizations_OrganizationId" FOREIGN KEY ("OrganizationId") REFERENCES "Organizations" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE TABLE "QuestionTemplates" (
        "Id" uuid NOT NULL,
        "QuestionnaireTemplateId" uuid NOT NULL,
        "QuestionText" character varying(1000) NOT NULL,
        "HelpText" character varying(500),
        "QuestionType" character varying(50) NOT NULL,
        "Order" integer NOT NULL,
        "IsRequired" boolean NOT NULL,
        "Section" character varying(100),
        "QuestionTextEN" text,
        "HelpTextEN" text,
        "OptionsEN" text,
        "Options" text,
        "ValidationRules" text,
        "ConditionalLogic" text,
        CONSTRAINT "PK_QuestionTemplates" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_QuestionTemplates_QuestionnaireTemplates_QuestionnaireTempl~" FOREIGN KEY ("QuestionnaireTemplateId") REFERENCES "QuestionnaireTemplates" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE TABLE "RolePermissions" (
        "Id" uuid NOT NULL,
        "RoleId" uuid NOT NULL,
        "PermissionId" uuid NOT NULL,
        CONSTRAINT "PK_RolePermissions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_RolePermissions_Permissions_PermissionId" FOREIGN KEY ("PermissionId") REFERENCES "Permissions" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_RolePermissions_Roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "Roles" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE TABLE "TemplateSections" (
        "Id" uuid NOT NULL,
        "TemplateId" uuid NOT NULL,
        "Name" text NOT NULL,
        "Title" text NOT NULL,
        "Content" text NOT NULL,
        "Description" text NOT NULL,
        "Order" integer NOT NULL,
        "IsRequired" boolean NOT NULL,
        "IsVisible" boolean NOT NULL,
        "SectionType" text NOT NULL,
        "Placeholder" text NOT NULL,
        "ValidationRules" text NOT NULL,
        "HelpText" text NOT NULL,
        "Icon" text NOT NULL,
        "Color" text NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "CreatedBy" text NOT NULL,
        "UpdatedBy" text NOT NULL,
        CONSTRAINT "PK_TemplateSections" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_TemplateSections_Templates_TemplateId" FOREIGN KEY ("TemplateId") REFERENCES "Templates" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE TABLE "ActiveSessions" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "SessionToken" character varying(500) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "LastActivityAt" timestamp with time zone NOT NULL,
        "ExpiresAt" timestamp with time zone NOT NULL,
        "IsActive" boolean NOT NULL DEFAULT TRUE,
        "IpAddress" character varying(45) NOT NULL,
        "UserAgent" character varying(500),
        "DeviceType" character varying(50),
        "Browser" character varying(100),
        "OperatingSystem" character varying(100),
        "Country" character varying(100),
        "City" character varying(100),
        "RevokedAt" timestamp with time zone,
        "RevokedByIp" character varying(45),
        CONSTRAINT "PK_ActiveSessions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ActiveSessions_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE TABLE "AuditLogs" (
        "Id" uuid NOT NULL,
        "UserId" uuid,
        "Action" text NOT NULL,
        "EntityType" text NOT NULL,
        "EntityId" text,
        "OldValues" text,
        "NewValues" text,
        "IpAddress" text,
        "UserAgent" text,
        "Timestamp" timestamp with time zone NOT NULL,
        "Success" boolean NOT NULL,
        "ErrorMessage" text,
        "AdditionalData" text,
        CONSTRAINT "PK_AuditLogs" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_AuditLogs_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE TABLE "EmailVerificationTokens" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "Token" text NOT NULL,
        "ExpiresAt" timestamp with time zone NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "IsUsed" boolean NOT NULL,
        "UsedAt" timestamp with time zone,
        "UsedByIp" text,
        CONSTRAINT "PK_EmailVerificationTokens" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_EmailVerificationTokens_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE TABLE "LoginHistories" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "LoginAttemptAt" timestamp with time zone NOT NULL,
        "IsSuccessful" boolean NOT NULL,
        "FailureReason" character varying(500),
        "IpAddress" character varying(45) NOT NULL,
        "UserAgent" character varying(500),
        "DeviceType" character varying(50),
        "Browser" character varying(100),
        "OperatingSystem" character varying(100),
        "Country" character varying(100),
        "City" character varying(100),
        "Latitude" double precision,
        "Longitude" double precision,
        CONSTRAINT "PK_LoginHistories" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_LoginHistories_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE TABLE "OrganizationMembers" (
        "Id" uuid NOT NULL,
        "OrganizationId" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "Role" character varying(50) NOT NULL,
        "IsActive" boolean NOT NULL DEFAULT TRUE,
        "JoinedAt" timestamp with time zone NOT NULL,
        "LeftAt" timestamp with time zone,
        "InvitedBy" uuid,
        "Created" timestamp with time zone NOT NULL,
        "CreatedBy" text,
        "LastModified" timestamp with time zone,
        "LastModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "DeletedBy" text,
        CONSTRAINT "PK_OrganizationMembers" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_OrganizationMembers_Organizations_OrganizationId" FOREIGN KEY ("OrganizationId") REFERENCES "Organizations" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_OrganizationMembers_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE TABLE "PasswordResetTokens" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "Token" text NOT NULL,
        "ExpiresAt" timestamp with time zone NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "IsUsed" boolean NOT NULL,
        "UsedAt" timestamp with time zone,
        "UsedByIp" text,
        CONSTRAINT "PK_PasswordResetTokens" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_PasswordResetTokens_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE TABLE "RefreshTokens" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "Token" text NOT NULL,
        "ExpiresAt" timestamp with time zone NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "RevokedAt" timestamp with time zone,
        "RevokedByIp" text,
        "ReplacedByToken" text,
        "CreatedByIp" text NOT NULL,
        CONSTRAINT "PK_RefreshTokens" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_RefreshTokens_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE TABLE "Subscriptions" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "OrganizationId" uuid NOT NULL,
        "SubscriptionPlanId" uuid NOT NULL,
        "Status" character varying(50) NOT NULL,
        "StartDate" timestamp with time zone NOT NULL,
        "EndDate" timestamp with time zone NOT NULL,
        "CancelledAt" timestamp with time zone,
        "CancelledEffectiveDate" timestamp with time zone,
        "IsYearly" boolean NOT NULL,
        "Amount" numeric(18,2) NOT NULL,
        "Currency" character varying(10) NOT NULL DEFAULT 'CAD',
        "IsTrial" boolean NOT NULL,
        "TrialEndDate" timestamp with time zone,
        "Created" timestamp with time zone NOT NULL,
        "CreatedBy" text,
        "LastModified" timestamp with time zone,
        "LastModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "DeletedBy" text,
        CONSTRAINT "PK_Subscriptions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Subscriptions_Organizations_OrganizationId" FOREIGN KEY ("OrganizationId") REFERENCES "Organizations" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_Subscriptions_SubscriptionPlans_SubscriptionPlanId" FOREIGN KEY ("SubscriptionPlanId") REFERENCES "SubscriptionPlans" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_Subscriptions_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE TABLE "TemplateCustomizations" (
        "Id" uuid NOT NULL,
        "TemplateId" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "Name" character varying(200) NOT NULL,
        "Description" character varying(1000) NOT NULL,
        "Customizations" text NOT NULL,
        "IsPublic" boolean NOT NULL,
        "IsDefault" boolean NOT NULL,
        "UsageCount" integer NOT NULL,
        "Rating" numeric(3,2) NOT NULL,
        "RatingCount" integer NOT NULL,
        "Tags" text NOT NULL,
        "Version" character varying(50) NOT NULL,
        "Changelog" text NOT NULL,
        "LastUsed" timestamp with time zone NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "CreatedBy" text NOT NULL,
        "UpdatedBy" text NOT NULL,
        CONSTRAINT "PK_TemplateCustomizations" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_TemplateCustomizations_Templates_TemplateId" FOREIGN KEY ("TemplateId") REFERENCES "Templates" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_TemplateCustomizations_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE TABLE "TemplateRatings" (
        "Id" uuid NOT NULL,
        "TemplateId" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "Rating" integer NOT NULL,
        "Comment" text NOT NULL,
        "IsVerified" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "CreatedBy" text NOT NULL,
        "UpdatedBy" text NOT NULL,
        CONSTRAINT "PK_TemplateRatings" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_TemplateRatings_Templates_TemplateId" FOREIGN KEY ("TemplateId") REFERENCES "Templates" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_TemplateRatings_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE TABLE "TwoFactorAuths" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "SecretKey" character varying(500) NOT NULL,
        "IsEnabled" boolean NOT NULL,
        "EnabledAt" timestamp with time zone,
        "BackupCodes" character varying(2000),
        "FailedAttempts" integer NOT NULL,
        "LastAttemptAt" timestamp with time zone,
        "Created" timestamp with time zone NOT NULL,
        "CreatedBy" text,
        "LastModified" timestamp with time zone,
        "LastModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "DeletedBy" text,
        CONSTRAINT "PK_TwoFactorAuths" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_TwoFactorAuths_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE TABLE "UserRoles" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "RoleId" uuid NOT NULL,
        CONSTRAINT "PK_UserRoles" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_UserRoles_Roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "Roles" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_UserRoles_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE TABLE "BusinessPlanShares" (
        "Id" uuid NOT NULL,
        "BusinessPlanId" uuid NOT NULL,
        "SharedWithUserId" uuid,
        "SharedWithEmail" character varying(256),
        "Permission" character varying(50) NOT NULL,
        "IsPublic" boolean NOT NULL,
        "PublicToken" character varying(50),
        "ExpiresAt" timestamp with time zone,
        "IsActive" boolean NOT NULL,
        "LastAccessedAt" timestamp with time zone,
        "AccessCount" integer NOT NULL,
        "Created" timestamp with time zone NOT NULL,
        "CreatedBy" text,
        "LastModified" timestamp with time zone,
        "LastModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "DeletedBy" text,
        CONSTRAINT "PK_BusinessPlanShares" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_BusinessPlanShares_BusinessPlans_BusinessPlanId" FOREIGN KEY ("BusinessPlanId") REFERENCES "BusinessPlans" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_BusinessPlanShares_Users_SharedWithUserId" FOREIGN KEY ("SharedWithUserId") REFERENCES "Users" ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE TABLE "BusinessPlanVersions" (
        "Id" uuid NOT NULL,
        "BusinessPlanId" uuid NOT NULL,
        "VersionNumber" integer NOT NULL,
        "Comment" character varying(1000),
        "ExecutiveSummary" text,
        "ProblemStatement" text,
        "Solution" text,
        "MarketAnalysis" text,
        "CompetitiveAnalysis" text,
        "SwotAnalysis" text,
        "BusinessModel" text,
        "MarketingStrategy" text,
        "BrandingStrategy" text,
        "OperationsPlan" text,
        "ManagementTeam" text,
        "FinancialProjections" text,
        "FundingRequirements" text,
        "RiskAnalysis" text,
        "ExitStrategy" text,
        "AppendixData" text,
        "MissionStatement" text,
        "SocialImpact" text,
        "BeneficiaryProfile" text,
        "GrantStrategy" text,
        "SustainabilityPlan" text,
        "Title" character varying(200) NOT NULL,
        "Description" character varying(1000),
        "PlanType" character varying(50) NOT NULL,
        "Status" character varying(50) NOT NULL,
        "Created" timestamp with time zone NOT NULL,
        "CreatedBy" text,
        "LastModified" timestamp with time zone,
        "LastModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "DeletedBy" text,
        CONSTRAINT "PK_BusinessPlanVersions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_BusinessPlanVersions_BusinessPlans_BusinessPlanId" FOREIGN KEY ("BusinessPlanId") REFERENCES "BusinessPlans" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE TABLE "FinancialKPIs" (
        "Id" uuid NOT NULL,
        "BusinessPlanId" uuid NOT NULL,
        "Name" character varying(200) NOT NULL,
        "Description" character varying(1000) NOT NULL,
        "Category" character varying(100) NOT NULL,
        "MetricType" character varying(50) NOT NULL,
        "Value" numeric(18,2) NOT NULL,
        "Unit" character varying(20) NOT NULL,
        "CurrencyCode" character varying(10) NOT NULL,
        "Year" integer NOT NULL,
        "Month" integer NOT NULL,
        "TargetValue" numeric(18,2) NOT NULL,
        "PreviousValue" numeric(18,2) NOT NULL,
        "ChangePercentage" numeric(5,2) NOT NULL,
        "Trend" character varying(20) NOT NULL,
        "Benchmark" character varying(200) NOT NULL,
        "Status" character varying(20) NOT NULL,
        "Notes" text NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "CreatedBy" text NOT NULL,
        "UpdatedBy" text NOT NULL,
        "CurrencyId" uuid,
        CONSTRAINT "PK_FinancialKPIs" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_FinancialKPIs_BusinessPlans_BusinessPlanId" FOREIGN KEY ("BusinessPlanId") REFERENCES "BusinessPlans" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_FinancialKPIs_Currencies_CurrencyId" FOREIGN KEY ("CurrencyId") REFERENCES "Currencies" ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE TABLE "FinancialProjectionItems" (
        "Id" uuid NOT NULL,
        "BusinessPlanId" uuid NOT NULL,
        "Name" character varying(200) NOT NULL,
        "Description" character varying(1000) NOT NULL,
        "ProjectionType" character varying(50) NOT NULL,
        "Scenario" character varying(50) NOT NULL,
        "Year" integer NOT NULL,
        "Month" integer NOT NULL,
        "Amount" numeric(18,2) NOT NULL,
        "CurrencyCode" character varying(10) NOT NULL,
        "ExchangeRate" numeric(18,6) NOT NULL,
        "BaseAmount" numeric(18,2) NOT NULL,
        "Category" character varying(100) NOT NULL,
        "SubCategory" character varying(100) NOT NULL,
        "IsRecurring" boolean NOT NULL,
        "Frequency" character varying(50) NOT NULL,
        "GrowthRate" numeric(5,2) NOT NULL,
        "Assumptions" text NOT NULL,
        "Notes" text NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "CreatedBy" text NOT NULL,
        "UpdatedBy" text NOT NULL,
        CONSTRAINT "PK_FinancialProjectionItems" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_FinancialProjectionItems_BusinessPlans_BusinessPlanId" FOREIGN KEY ("BusinessPlanId") REFERENCES "BusinessPlans" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_FinancialProjectionItems_Currencies_CurrencyCode" FOREIGN KEY ("CurrencyCode") REFERENCES "Currencies" ("Code") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE TABLE "FinancialProjections" (
        "Id" uuid NOT NULL,
        "BusinessPlanId" uuid NOT NULL,
        "Year" integer NOT NULL,
        "Month" integer,
        "Quarter" integer,
        "Revenue" numeric(18,2),
        "RevenueGrowthRate" numeric(5,2),
        "CostOfGoodsSold" numeric(18,2),
        "OperatingExpenses" numeric(18,2),
        "MarketingExpenses" numeric(18,2),
        "RAndDExpenses" numeric(18,2),
        "AdministrativeExpenses" numeric(18,2),
        "OtherExpenses" numeric(18,2),
        "GrossProfit" numeric(18,2),
        "NetIncome" numeric(18,2),
        "EBITDA" numeric(18,2),
        "CashFlow" numeric(18,2),
        "CashBalance" numeric(18,2),
        "Employees" integer,
        "Customers" integer,
        "UnitsSold" integer,
        "AverageRevenuePerCustomer" numeric(18,2),
        "Notes" character varying(1000),
        "Assumptions" text,
        CONSTRAINT "PK_FinancialProjections" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_FinancialProjections_BusinessPlans_BusinessPlanId" FOREIGN KEY ("BusinessPlanId") REFERENCES "BusinessPlans" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE TABLE "InvestmentAnalyses" (
        "Id" uuid NOT NULL,
        "BusinessPlanId" uuid NOT NULL,
        "AnalysisType" character varying(50) NOT NULL,
        "Name" character varying(200) NOT NULL,
        "Description" character varying(1000) NOT NULL,
        "InitialInvestment" numeric(18,2) NOT NULL,
        "ExpectedReturn" numeric(18,2) NOT NULL,
        "NetPresentValue" numeric(18,2) NOT NULL,
        "InternalRateOfReturn" numeric(5,2) NOT NULL,
        "PaybackPeriod" numeric(10,2) NOT NULL,
        "ReturnOnInvestment" numeric(5,2) NOT NULL,
        "CurrencyCode" character varying(10) NOT NULL,
        "DiscountRate" numeric(5,2) NOT NULL,
        "AnalysisPeriod" integer NOT NULL,
        "RiskLevel" character varying(20) NOT NULL,
        "InvestmentType" character varying(50) NOT NULL,
        "InvestorType" character varying(50) NOT NULL,
        "Valuation" numeric(18,2) NOT NULL,
        "EquityOffering" numeric(5,2) NOT NULL,
        "FundingRequired" numeric(18,2) NOT NULL,
        "FundingStage" character varying(50) NOT NULL,
        "Assumptions" text NOT NULL,
        "Notes" text NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "CreatedBy" text NOT NULL,
        "UpdatedBy" text NOT NULL,
        "CurrencyId" uuid,
        CONSTRAINT "PK_InvestmentAnalyses" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_InvestmentAnalyses_BusinessPlans_BusinessPlanId" FOREIGN KEY ("BusinessPlanId") REFERENCES "BusinessPlans" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_InvestmentAnalyses_Currencies_CurrencyId" FOREIGN KEY ("CurrencyId") REFERENCES "Currencies" ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE TABLE "TemplateUsages" (
        "Id" uuid NOT NULL,
        "TemplateId" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "BusinessPlanId" uuid,
        "UsageType" text NOT NULL,
        "UserAgent" text NOT NULL,
        "IpAddress" text NOT NULL,
        "Country" text NOT NULL,
        "City" text NOT NULL,
        "UsedAt" timestamp with time zone NOT NULL,
        "Duration" integer NOT NULL,
        "Referrer" text NOT NULL,
        "SessionId" text NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "CreatedBy" text NOT NULL,
        "UpdatedBy" text NOT NULL,
        CONSTRAINT "PK_TemplateUsages" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_TemplateUsages_BusinessPlans_BusinessPlanId" FOREIGN KEY ("BusinessPlanId") REFERENCES "BusinessPlans" ("Id"),
        CONSTRAINT "FK_TemplateUsages_Templates_TemplateId" FOREIGN KEY ("TemplateId") REFERENCES "Templates" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_TemplateUsages_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE TABLE "GrantApplications" (
        "Id" uuid NOT NULL,
        "OBNLBusinessPlanId" uuid NOT NULL,
        "GrantName" character varying(200) NOT NULL,
        "GrantingOrganization" character varying(200) NOT NULL,
        "GrantType" character varying(100) NOT NULL,
        "RequestedAmount" numeric(18,2) NOT NULL,
        "MatchingFunds" numeric(18,2) NOT NULL,
        "ProjectDescription" text NOT NULL,
        "Objectives" text NOT NULL,
        "ExpectedOutcomes" text NOT NULL,
        "TargetPopulation" text NOT NULL,
        "GeographicScope" text NOT NULL,
        "Timeline" text NOT NULL,
        "BudgetBreakdown" text NOT NULL,
        "EvaluationPlan" text NOT NULL,
        "SustainabilityPlan" text NOT NULL,
        "ApplicationDeadline" timestamp with time zone NOT NULL,
        "SubmissionDate" timestamp with time zone NOT NULL,
        "Status" character varying(50) NOT NULL,
        "Decision" character varying(50) NOT NULL,
        "Notes" text NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_GrantApplications" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_GrantApplications_OBNLBusinessPlans_OBNLBusinessPlanId" FOREIGN KEY ("OBNLBusinessPlanId") REFERENCES "OBNLBusinessPlans" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE TABLE "ImpactMeasurements" (
        "Id" uuid NOT NULL,
        "OBNLBusinessPlanId" uuid NOT NULL,
        "MetricName" character varying(200) NOT NULL,
        "Description" character varying(1000) NOT NULL,
        "MeasurementType" character varying(100) NOT NULL,
        "UnitOfMeasurement" character varying(50) NOT NULL,
        "BaselineValue" numeric(18,2) NOT NULL,
        "TargetValue" numeric(18,2) NOT NULL,
        "CurrentValue" numeric(18,2) NOT NULL,
        "DataSource" character varying(200) NOT NULL,
        "CollectionMethod" character varying(200) NOT NULL,
        "Frequency" character varying(50) NOT NULL,
        "ResponsibleParty" character varying(200) NOT NULL,
        "LastMeasurement" timestamp with time zone NOT NULL,
        "NextMeasurement" timestamp with time zone NOT NULL,
        "Status" character varying(50) NOT NULL,
        "Notes" text NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_ImpactMeasurements" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ImpactMeasurements_OBNLBusinessPlans_OBNLBusinessPlanId" FOREIGN KEY ("OBNLBusinessPlanId") REFERENCES "OBNLBusinessPlans" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE TABLE "OBNLCompliances" (
        "Id" uuid NOT NULL,
        "OBNLBusinessPlanId" uuid NOT NULL,
        "RequirementType" text NOT NULL,
        "Description" text NOT NULL,
        "Jurisdiction" text NOT NULL,
        "RegulatoryBody" text NOT NULL,
        "ComplianceLevel" text NOT NULL,
        "IsRequired" boolean NOT NULL,
        "DueDate" timestamp with time zone NOT NULL,
        "Status" text NOT NULL,
        "Documentation" text NOT NULL,
        "Notes" text NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_OBNLCompliances" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_OBNLCompliances_OBNLBusinessPlans_OBNLBusinessPlanId" FOREIGN KEY ("OBNLBusinessPlanId") REFERENCES "OBNLBusinessPlans" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE TABLE "QuestionnaireResponses" (
        "Id" uuid NOT NULL,
        "BusinessPlanId" uuid NOT NULL,
        "QuestionTemplateId" uuid NOT NULL,
        "ResponseText" text NOT NULL,
        "NumericValue" numeric(18,2),
        "DateValue" timestamp with time zone,
        "BooleanValue" boolean,
        "SelectedOptions" text,
        "AiInsights" text,
        "Created" timestamp with time zone NOT NULL,
        "CreatedBy" text,
        "LastModified" timestamp with time zone,
        "LastModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "DeletedBy" text,
        CONSTRAINT "PK_QuestionnaireResponses" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_QuestionnaireResponses_BusinessPlans_BusinessPlanId" FOREIGN KEY ("BusinessPlanId") REFERENCES "BusinessPlans" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_QuestionnaireResponses_QuestionTemplates_QuestionTemplateId" FOREIGN KEY ("QuestionTemplateId") REFERENCES "QuestionTemplates" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE TABLE "TemplateFields" (
        "Id" uuid NOT NULL,
        "TemplateSectionId" uuid NOT NULL,
        "Name" character varying(200) NOT NULL,
        "Label" character varying(200) NOT NULL,
        "FieldType" character varying(50) NOT NULL,
        "Value" text NOT NULL,
        "DefaultValue" text NOT NULL,
        "Placeholder" text NOT NULL,
        "Description" text NOT NULL,
        "IsRequired" boolean NOT NULL,
        "IsReadOnly" boolean NOT NULL,
        "IsVisible" boolean NOT NULL,
        "Order" integer NOT NULL,
        "ValidationRules" text NOT NULL,
        "Options" text NOT NULL,
        "HelpText" text NOT NULL,
        "Format" character varying(50) NOT NULL,
        "MinLength" integer NOT NULL,
        "MaxLength" integer NOT NULL,
        "MinValue" numeric(18,2) NOT NULL,
        "MaxValue" numeric(18,2) NOT NULL,
        "Pattern" character varying(200) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "CreatedBy" text NOT NULL,
        "UpdatedBy" text NOT NULL,
        CONSTRAINT "PK_TemplateFields" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_TemplateFields_TemplateSections_TemplateSectionId" FOREIGN KEY ("TemplateSectionId") REFERENCES "TemplateSections" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE TABLE "TaxCalculations" (
        "Id" uuid NOT NULL,
        "FinancialProjectionId" uuid NOT NULL,
        "TaxRuleId" uuid NOT NULL,
        "TaxName" character varying(200) NOT NULL,
        "TaxType" character varying(50) NOT NULL,
        "TaxableAmount" numeric(18,2) NOT NULL,
        "TaxRate" numeric(5,2) NOT NULL,
        "TaxAmount" numeric(18,2) NOT NULL,
        "CurrencyCode" character varying(10) NOT NULL,
        "CalculationMethod" character varying(50) NOT NULL,
        "Country" character varying(100) NOT NULL,
        "Region" character varying(100) NOT NULL,
        "TaxPeriod" timestamp with time zone NOT NULL,
        "IsPaid" boolean NOT NULL,
        "PaymentDate" timestamp with time zone,
        "PaymentReference" character varying(200) NOT NULL,
        "Notes" text NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "CreatedBy" text NOT NULL,
        "UpdatedBy" text NOT NULL,
        CONSTRAINT "PK_TaxCalculations" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_TaxCalculations_FinancialProjectionItems_FinancialProjectio~" FOREIGN KEY ("FinancialProjectionId") REFERENCES "FinancialProjectionItems" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_TaxCalculations_TaxRules_TaxRuleId" FOREIGN KEY ("TaxRuleId") REFERENCES "TaxRules" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_ActiveSessions_ExpiresAt" ON "ActiveSessions" ("ExpiresAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_ActiveSessions_SessionToken" ON "ActiveSessions" ("SessionToken");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_ActiveSessions_UserId" ON "ActiveSessions" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_ActiveSessions_UserId_IsActive" ON "ActiveSessions" ("UserId", "IsActive");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_AuditLogs_UserId" ON "AuditLogs" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_BusinessPlans_CreatedBy" ON "BusinessPlans" ("CreatedBy");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_BusinessPlans_OrganizationId" ON "BusinessPlans" ("OrganizationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_BusinessPlans_OrganizationId_Status" ON "BusinessPlans" ("OrganizationId", "Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_BusinessPlans_PlanType" ON "BusinessPlans" ("PlanType");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_BusinessPlans_Status" ON "BusinessPlans" ("Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_BusinessPlanShares_BusinessPlanId" ON "BusinessPlanShares" ("BusinessPlanId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_BusinessPlanShares_BusinessPlanId_IsActive" ON "BusinessPlanShares" ("BusinessPlanId", "IsActive");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_BusinessPlanShares_PublicToken" ON "BusinessPlanShares" ("PublicToken") WHERE "PublicToken" IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_BusinessPlanShares_SharedWithUserId" ON "BusinessPlanShares" ("SharedWithUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_BusinessPlanVersions_BusinessPlanId" ON "BusinessPlanVersions" ("BusinessPlanId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_BusinessPlanVersions_BusinessPlanId_VersionNumber" ON "BusinessPlanVersions" ("BusinessPlanId", "VersionNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_BusinessPlanVersions_Created" ON "BusinessPlanVersions" ("Created");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Currencies_Code" ON "Currencies" ("Code");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_EmailVerificationTokens_UserId" ON "EmailVerificationTokens" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_ExchangeRates_FromCurrencyId" ON "ExchangeRates" ("FromCurrencyId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_ExchangeRates_ToCurrencyId" ON "ExchangeRates" ("ToCurrencyId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_FinancialKPIs_BusinessPlanId" ON "FinancialKPIs" ("BusinessPlanId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_FinancialKPIs_CurrencyId" ON "FinancialKPIs" ("CurrencyId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_FinancialProjectionItems_BusinessPlanId" ON "FinancialProjectionItems" ("BusinessPlanId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_FinancialProjectionItems_CurrencyCode" ON "FinancialProjectionItems" ("CurrencyCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_FinancialProjections_BusinessPlanId" ON "FinancialProjections" ("BusinessPlanId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_FinancialProjections_BusinessPlanId_Year" ON "FinancialProjections" ("BusinessPlanId", "Year") WHERE "Month" IS NULL AND "Quarter" IS NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_FinancialProjections_BusinessPlanId_Year_Month" ON "FinancialProjections" ("BusinessPlanId", "Year", "Month") WHERE "Month" IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_FinancialProjections_BusinessPlanId_Year_Quarter" ON "FinancialProjections" ("BusinessPlanId", "Year", "Quarter") WHERE "Quarter" IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_GrantApplications_OBNLBusinessPlanId" ON "GrantApplications" ("OBNLBusinessPlanId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_ImpactMeasurements_OBNLBusinessPlanId" ON "ImpactMeasurements" ("OBNLBusinessPlanId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_InvestmentAnalyses_BusinessPlanId" ON "InvestmentAnalyses" ("BusinessPlanId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_InvestmentAnalyses_CurrencyId" ON "InvestmentAnalyses" ("CurrencyId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_LoginHistories_LoginAttemptAt" ON "LoginHistories" ("LoginAttemptAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_LoginHistories_UserId" ON "LoginHistories" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_LoginHistories_UserId_LoginAttemptAt" ON "LoginHistories" ("UserId", "LoginAttemptAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_OBNLBusinessPlans_OrganizationId" ON "OBNLBusinessPlans" ("OrganizationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_OBNLCompliances_OBNLBusinessPlanId" ON "OBNLCompliances" ("OBNLBusinessPlanId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_OrganizationMembers_IsActive" ON "OrganizationMembers" ("IsActive");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_OrganizationMembers_OrganizationId" ON "OrganizationMembers" ("OrganizationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_OrganizationMembers_OrganizationId_UserId" ON "OrganizationMembers" ("OrganizationId", "UserId") WHERE "IsActive" = true;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_OrganizationMembers_Role" ON "OrganizationMembers" ("Role");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_OrganizationMembers_UserId" ON "OrganizationMembers" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_Organizations_CreatedBy" ON "Organizations" ("CreatedBy");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_Organizations_IsActive" ON "Organizations" ("IsActive");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_Organizations_Name" ON "Organizations" ("Name");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_PasswordResetTokens_UserId" ON "PasswordResetTokens" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_Permissions_Category" ON "Permissions" ("Category");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Permissions_Name" ON "Permissions" ("Name");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_QuestionnaireResponses_BusinessPlanId" ON "QuestionnaireResponses" ("BusinessPlanId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_QuestionnaireResponses_BusinessPlanId_QuestionTemplateId" ON "QuestionnaireResponses" ("BusinessPlanId", "QuestionTemplateId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_QuestionnaireResponses_QuestionTemplateId" ON "QuestionnaireResponses" ("QuestionTemplateId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_QuestionnaireTemplates_IsActive" ON "QuestionnaireTemplates" ("IsActive");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_QuestionnaireTemplates_PlanType" ON "QuestionnaireTemplates" ("PlanType");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_QuestionnaireTemplates_PlanType_IsActive_Version" ON "QuestionnaireTemplates" ("PlanType", "IsActive", "Version");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_QuestionTemplates_QuestionnaireTemplateId" ON "QuestionTemplates" ("QuestionnaireTemplateId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_QuestionTemplates_QuestionnaireTemplateId_Order" ON "QuestionTemplates" ("QuestionnaireTemplateId", "Order");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_RefreshTokens_UserId" ON "RefreshTokens" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_RolePermissions_PermissionId" ON "RolePermissions" ("PermissionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_RolePermissions_RoleId" ON "RolePermissions" ("RoleId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_RolePermissions_RoleId_PermissionId" ON "RolePermissions" ("RoleId", "PermissionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_Roles_IsSystemRole" ON "Roles" ("IsSystemRole");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Roles_Name" ON "Roles" ("Name");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_Settings_Category" ON "Settings" ("Category");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_Settings_Critical_Type" ON "Settings" ("IsCritical", "SettingType");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_Settings_IsCritical" ON "Settings" ("IsCritical");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_Settings_IsPublic" ON "Settings" ("IsPublic");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Settings_Key" ON "Settings" ("Key");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_Settings_SettingType" ON "Settings" ("SettingType");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_SubscriptionPlans_IsActive" ON "SubscriptionPlans" ("IsActive");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_SubscriptionPlans_PlanType" ON "SubscriptionPlans" ("PlanType");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_Subscriptions_OrganizationId" ON "Subscriptions" ("OrganizationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_Subscriptions_OrganizationId_Status" ON "Subscriptions" ("OrganizationId", "Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_Subscriptions_Status" ON "Subscriptions" ("Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_Subscriptions_SubscriptionPlanId" ON "Subscriptions" ("SubscriptionPlanId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_Subscriptions_UserId" ON "Subscriptions" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_TaxCalculations_FinancialProjectionId" ON "TaxCalculations" ("FinancialProjectionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_TaxCalculations_TaxRuleId" ON "TaxCalculations" ("TaxRuleId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_TaxRules_CurrencyCode" ON "TaxRules" ("CurrencyCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_TemplateCustomizations_TemplateId" ON "TemplateCustomizations" ("TemplateId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_TemplateCustomizations_UserId" ON "TemplateCustomizations" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_TemplateFields_TemplateSectionId" ON "TemplateFields" ("TemplateSectionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_TemplateRatings_TemplateId" ON "TemplateRatings" ("TemplateId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_TemplateRatings_UserId" ON "TemplateRatings" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_TemplateSections_TemplateId" ON "TemplateSections" ("TemplateId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_TemplateUsages_BusinessPlanId" ON "TemplateUsages" ("BusinessPlanId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_TemplateUsages_TemplateId" ON "TemplateUsages" ("TemplateId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_TemplateUsages_UserId" ON "TemplateUsages" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_TwoFactorAuths_UserId" ON "TwoFactorAuths" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_UserRoles_RoleId" ON "UserRoles" ("RoleId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE INDEX "IX_UserRoles_UserId" ON "UserRoles" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Users_Email" ON "Users" ("Email");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Users_GoogleId" ON "Users" ("GoogleId") WHERE "GoogleId" IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Users_UserName" ON "Users" ("UserName");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251231183954_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251231183954_InitialCreate', '8.0.10');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104153401_AddSmartObjectivesPlanCommentsAndContentPages') THEN
    CREATE TABLE "ContentPages" (
        "Id" uuid NOT NULL,
        "PageKey" character varying(100) NOT NULL,
        "Title" character varying(200) NOT NULL,
        "Content" text NOT NULL,
        "Language" character varying(2) NOT NULL,
        "IsPublished" boolean NOT NULL,
        "PublishedAt" timestamp with time zone,
        "Version" integer NOT NULL,
        "Created" timestamp with time zone NOT NULL,
        "CreatedBy" text,
        "LastModified" timestamp with time zone,
        "LastModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "DeletedBy" text,
        CONSTRAINT "PK_ContentPages" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104153401_AddSmartObjectivesPlanCommentsAndContentPages') THEN
    CREATE TABLE "PlanSectionComments" (
        "Id" uuid NOT NULL,
        "BusinessPlanId" uuid NOT NULL,
        "SectionName" character varying(100) NOT NULL,
        "CommentText" character varying(2000) NOT NULL,
        "ParentCommentId" uuid,
        "IsResolved" boolean NOT NULL,
        "ResolvedAt" timestamp with time zone,
        "ResolvedByUserId" uuid,
        "Created" timestamp with time zone NOT NULL,
        "CreatedBy" text,
        "LastModified" timestamp with time zone,
        "LastModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "DeletedBy" text,
        CONSTRAINT "PK_PlanSectionComments" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_PlanSectionComments_BusinessPlans_BusinessPlanId" FOREIGN KEY ("BusinessPlanId") REFERENCES "BusinessPlans" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_PlanSectionComments_PlanSectionComments_ParentCommentId" FOREIGN KEY ("ParentCommentId") REFERENCES "PlanSectionComments" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104153401_AddSmartObjectivesPlanCommentsAndContentPages') THEN
    CREATE TABLE "SmartObjectives" (
        "Id" uuid NOT NULL,
        "BusinessPlanId" uuid NOT NULL,
        "Title" character varying(200) NOT NULL,
        "Description" character varying(2000) NOT NULL,
        "Specific" character varying(500) NOT NULL,
        "Measurable" character varying(500) NOT NULL,
        "Achievable" character varying(500) NOT NULL,
        "Relevant" character varying(500) NOT NULL,
        "TimeBound" character varying(500) NOT NULL,
        "TargetDate" timestamp with time zone NOT NULL,
        "CompletedDate" timestamp with time zone,
        "ProgressPercentage" numeric(5,2) NOT NULL,
        "Status" character varying(50) NOT NULL,
        "Category" character varying(100) NOT NULL,
        "Priority" integer NOT NULL,
        "Created" timestamp with time zone NOT NULL,
        "CreatedBy" text,
        "LastModified" timestamp with time zone,
        "LastModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "DeletedBy" text,
        CONSTRAINT "PK_SmartObjectives" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_SmartObjectives_BusinessPlans_BusinessPlanId" FOREIGN KEY ("BusinessPlanId") REFERENCES "BusinessPlans" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104153401_AddSmartObjectivesPlanCommentsAndContentPages') THEN
    CREATE INDEX "IX_ContentPages_IsPublished" ON "ContentPages" ("IsPublished");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104153401_AddSmartObjectivesPlanCommentsAndContentPages') THEN
    CREATE UNIQUE INDEX "IX_ContentPages_PageKey_Language" ON "ContentPages" ("PageKey", "Language");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104153401_AddSmartObjectivesPlanCommentsAndContentPages') THEN
    CREATE INDEX "IX_PlanSectionComments_BusinessPlanId" ON "PlanSectionComments" ("BusinessPlanId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104153401_AddSmartObjectivesPlanCommentsAndContentPages') THEN
    CREATE INDEX "IX_PlanSectionComments_BusinessPlanId_SectionName" ON "PlanSectionComments" ("BusinessPlanId", "SectionName");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104153401_AddSmartObjectivesPlanCommentsAndContentPages') THEN
    CREATE INDEX "IX_PlanSectionComments_ParentCommentId" ON "PlanSectionComments" ("ParentCommentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104153401_AddSmartObjectivesPlanCommentsAndContentPages') THEN
    CREATE INDEX "IX_SmartObjectives_BusinessPlanId" ON "SmartObjectives" ("BusinessPlanId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104153401_AddSmartObjectivesPlanCommentsAndContentPages') THEN
    CREATE INDEX "IX_SmartObjectives_BusinessPlanId_Category" ON "SmartObjectives" ("BusinessPlanId", "Category");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260104153401_AddSmartObjectivesPlanCommentsAndContentPages') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260104153401_AddSmartObjectivesPlanCommentsAndContentPages', '8.0.10');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260107041531_AddStripeFieldsToSubscription') THEN
    ALTER TABLE "Subscriptions" ADD "StripeCustomerId" character varying(255);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260107041531_AddStripeFieldsToSubscription') THEN
    ALTER TABLE "Subscriptions" ADD "StripePriceId" character varying(255);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260107041531_AddStripeFieldsToSubscription') THEN
    ALTER TABLE "Subscriptions" ADD "StripeSubscriptionId" character varying(255);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260107041531_AddStripeFieldsToSubscription') THEN
    CREATE INDEX "IX_Subscriptions_StripeCustomerId" ON "Subscriptions" ("StripeCustomerId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260107041531_AddStripeFieldsToSubscription') THEN
    CREATE INDEX "IX_Subscriptions_StripeSubscriptionId" ON "Subscriptions" ("StripeSubscriptionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260107041531_AddStripeFieldsToSubscription') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260107041531_AddStripeFieldsToSubscription', '8.0.10');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260111051559_RemoveSettingsEnumDefaults') THEN
    ALTER TABLE "Settings" ALTER COLUMN "SettingType" DROP DEFAULT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260111051559_RemoveSettingsEnumDefaults') THEN
    ALTER TABLE "Settings" ALTER COLUMN "DataType" DROP DEFAULT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260111051559_RemoveSettingsEnumDefaults') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260111051559_RemoveSettingsEnumDefaults', '8.0.10');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260111060335_MakePasswordHashNullable') THEN
    ALTER TABLE "Users" ALTER COLUMN "PasswordHash" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260111060335_MakePasswordHashNullable') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260111060335_MakePasswordHashNullable', '8.0.10');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260115001316_AddSectionNameToAIPrompt') THEN
    ALTER TABLE "AIPrompts" ADD "SectionName" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260115001316_AddSectionNameToAIPrompt') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260115001316_AddSectionNameToAIPrompt', '8.0.10');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260116041641_RecreateRefreshTokensTable') THEN
    ALTER TABLE "RefreshTokens" DROP CONSTRAINT "FK_RefreshTokens_Users_UserId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260116041641_RecreateRefreshTokensTable') THEN
    DROP TABLE "RefreshTokens";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260116041641_RecreateRefreshTokensTable') THEN
    CREATE TABLE "RefreshTokens" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "Token" text NOT NULL,
        "ExpiresAt" timestamp with time zone NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "RevokedAt" timestamp with time zone,
        "RevokedByIp" text,
        "ReplacedByToken" text,
        "CreatedByIp" text NOT NULL,
        CONSTRAINT "PK_RefreshTokens" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_RefreshTokens_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260116041641_RecreateRefreshTokensTable') THEN
    CREATE INDEX "IX_RefreshTokens_UserId" ON "RefreshTokens" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260116041641_RecreateRefreshTokensTable') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260116041641_RecreateRefreshTokensTable', '8.0.10');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260119155236_AddGrowthArchitectV2Features') THEN
    ALTER TABLE "BusinessPlans" ADD "CurrentGenerationSection" character varying(100);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260119155236_AddGrowthArchitectV2Features') THEN
    ALTER TABLE "BusinessPlans" ADD "GenerationProgress" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260119155236_AddGrowthArchitectV2Features') THEN
    ALTER TABLE "BusinessPlans" ADD "MonthlyBurnRate" numeric(18,2);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260119155236_AddGrowthArchitectV2Features') THEN
    ALTER TABLE "BusinessPlans" ADD "Persona" character varying(50);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260119155236_AddGrowthArchitectV2Features') THEN
    ALTER TABLE "BusinessPlans" ADD "PivotPointMonth" integer;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260119155236_AddGrowthArchitectV2Features') THEN
    ALTER TABLE "BusinessPlans" ADD "ReadinessScore" numeric(5,2);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260119155236_AddGrowthArchitectV2Features') THEN
    ALTER TABLE "BusinessPlans" ADD "RunwayMonths" integer;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260119155236_AddGrowthArchitectV2Features') THEN
    ALTER TABLE "BusinessPlans" ADD "StrategyMapJson" jsonb;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260119155236_AddGrowthArchitectV2Features') THEN
    ALTER TABLE "BusinessPlans" ADD "TargetCAC" numeric(18,2);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260119155236_AddGrowthArchitectV2Features') THEN
    CREATE TABLE "QuestionTemplatesV2" (
        "Id" uuid NOT NULL,
        "PersonaType" character varying(50),
        "StepNumber" integer NOT NULL,
        "QuestionText" character varying(1000) NOT NULL,
        "QuestionTextEN" character varying(1000),
        "HelpText" character varying(2000),
        "HelpTextEN" character varying(2000),
        "QuestionType" character varying(50) NOT NULL,
        "Order" integer NOT NULL,
        "IsRequired" boolean NOT NULL,
        "Section" character varying(100),
        "Options" jsonb,
        "OptionsEN" jsonb,
        "ValidationRules" jsonb,
        "ConditionalLogic" jsonb,
        "IsActive" boolean NOT NULL,
        "Icon" character varying(50),
        "Created" timestamp with time zone NOT NULL,
        "CreatedBy" text,
        "LastModified" timestamp with time zone,
        "LastModifiedBy" text,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "DeletedBy" text,
        CONSTRAINT "PK_QuestionTemplatesV2" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260119155236_AddGrowthArchitectV2Features') THEN
    CREATE INDEX "IX_QuestionTemplatesV2_IsActive" ON "QuestionTemplatesV2" ("IsActive");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260119155236_AddGrowthArchitectV2Features') THEN
    CREATE INDEX "IX_QuestionTemplatesV2_PersonaType" ON "QuestionTemplatesV2" ("PersonaType");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260119155236_AddGrowthArchitectV2Features') THEN
    CREATE INDEX "IX_QuestionTemplatesV2_PersonaType_StepNumber_Order" ON "QuestionTemplatesV2" ("PersonaType", "StepNumber", "Order");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260119155236_AddGrowthArchitectV2Features') THEN
    CREATE INDEX "IX_QuestionTemplatesV2_StepNumber" ON "QuestionTemplatesV2" ("StepNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260119155236_AddGrowthArchitectV2Features') THEN
    CREATE INDEX "IX_QuestionTemplatesV2_StepNumber_Order" ON "QuestionTemplatesV2" ("StepNumber", "Order");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260119155236_AddGrowthArchitectV2Features') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260119155236_AddGrowthArchitectV2Features', '8.0.10');
    END IF;
END $EF$;
COMMIT;


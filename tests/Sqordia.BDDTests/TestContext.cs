using AutoFixture;

namespace Sqordia.BDDTests;

public class TestContext
{
    private readonly Fixture _fixture;

    public TestContext()
    {
        _fixture = new Fixture();
        SetupAutoFixture();
    }

    // Application State
    public bool ApplicationStarted { get; set; }
    public bool RegistrationSystemAvailable { get; set; }
    public bool AIServiceAvailable { get; set; }
    public bool IsNewUser { get; set; }

    // Test Data
    public User? CurrentUser { get; set; }
    public Organization? CurrentOrganization { get; set; }
    public BusinessPlan? CurrentBusinessPlan { get; set; }
    public OBNLBusinessPlan? CurrentOBNLPlan { get; set; }
    public Template? SelectedTemplate { get; set; }
    public Template? OwnedTemplate { get; set; }
    public string? CurrentTemplateName { get; set; }

    // Financial Management
    public FinancialProjection? CurrentFinancialProjection { get; set; }
    public InvestmentAnalysis? CurrentInvestmentAnalysis { get; set; }
    public string CurrentCurrency { get; set; } = string.Empty;
    public string CurrentScenario { get; set; } = string.Empty;
    public string CurrentGrowthRate { get; set; } = string.Empty;
    public string CurrentCountry { get; set; } = string.Empty;
    public string CurrentRegion { get; set; } = string.Empty;
    public string BaseCurrency { get; set; } = string.Empty;
    public decimal ExpectedReturn { get; set; }
    public int AnalysisPeriod { get; set; }
    public decimal DiscountRate { get; set; }
    public decimal UpdatedGrowthRate { get; set; }
    public List<FinancialProjection> MultiCurrencyProjections { get; set; } = new();
    public List<User>? TeamMembers { get; set; }
    public string? CurrentIndustry { get; set; }

    // Test Results
    public TestResult<User>? RegistrationResult { get; set; }
    public TestResult<LoginResponse>? LoginResult { get; set; }
    public TestResult<BusinessPlan>? BusinessPlanGenerationResult { get; set; }
    public TestResult<AIEnhancementResponse>? AIEnhancementResult { get; set; }
    public TestResult<ExportResponse>? ExportResult { get; set; }
    public TestResult<Organization>? OrganizationCreationResult { get; set; }
    public TestResult<InvitationResponse>? InvitationResult { get; set; }
    public List<object>? AcceptanceResults { get; set; }
    public TestResult<object>? RoleUpdateResult { get; set; }
    public TestResult<object>? BusinessPlanUpdateResult { get; set; }
    public object[]? ConcurrentUpdateResults { get; set; }
    public TestResult<object>? SubscriptionUpgradeResult { get; set; }
    public TestResult<object>? SharingResult { get; set; }
    public TestResult<QuestionnaireResponse>? QuestionnaireResult { get; set; }
    
    // OBNL Test Results
    public TestResult<OBNLBusinessPlan>? OBNLPlanResult { get; set; }
    public TestResult<ComplianceAnalysis>? ComplianceAnalysisResult { get; set; }
    public TestResult<GrantApplication>? GrantApplicationResult { get; set; }
    public TestResult<ImpactMeasurement>? ImpactMeasurementResult { get; set; }
    public TestResult<SustainabilityPlan>? SustainabilityPlanningResult { get; set; }
    
    // Template Test Results
    public TestResult<Template>? TemplateResult { get; set; }
    public TestResult<List<Template>>? TemplateCategoryResult { get; set; }
    public TestResult<List<Template>>? TemplateSearchResult { get; set; }
    public TestResult<List<Template>>? PopularTemplatesResult { get; set; }
    public TestResult<List<Template>>? RecentTemplatesResult { get; set; }
    public TestResult<List<Template>>? AuthorTemplatesResult { get; set; }
    public TestResult<Template>? CloneResult { get; set; }
    public TestResult<Template>? PublishResult { get; set; }
    public TestResult<Template>? ArchiveResult { get; set; }
    public TestResult<TemplateUsage>? UsageResult { get; set; }
    public TestResult<TemplateRating>? RatingResult { get; set; }
    public TestResult<TemplateCustomization>? CustomizationResult { get; set; }
    public TestResult<TemplateAnalytics>? AnalyticsResult { get; set; }
    public TestResult<StakeholderEngagement>? StakeholderEngagementResult { get; set; }
    public TestResult<GovernanceStructure>? GovernanceStructureResult { get; set; }
    public TestResult<ComplianceReport>? ComplianceReportingResult { get; set; }

    // Financial Test Results
    public TestResult<FinancialProjection>? FinancialProjectionResult { get; set; }
    public TestResult<decimal>? CurrencyConversionResult { get; set; }
    public TestResult<TaxCalculation>? TaxCalculationResult { get; set; }
    public TestResult<List<FinancialKPI>>? FinancialKPIsResult { get; set; }
    public TestResult<InvestmentAnalysis>? InvestmentAnalysisResult { get; set; }
    public TestResult<CashFlowReport>? CashFlowReportResult { get; set; }
    public TestResult<ProfitLossReport>? ProfitLossReportResult { get; set; }
    public TestResult<BalanceSheetReport>? BalanceSheetReportResult { get; set; }
    public TestResult<List<ScenarioAnalysis>>? ScenarioAnalysisResult { get; set; }
    public TestResult<SensitivityAnalysis>? SensitivityAnalysisResult { get; set; }
    public TestResult<BreakEvenAnalysis>? BreakEvenAnalysisResult { get; set; }
    public TestResult<FinancialProjection>? FinancialProjectionUpdateResult { get; set; }
    public TestResult<bool>? FinancialProjectionDeleteResult { get; set; }
    public TestResult<List<FinancialProjection>>? ScenarioProjectionsResult { get; set; }
    public TestResult<List<FinancialProjection>>? CategoryProjectionsResult { get; set; }
    public TestResult<List<TaxCalculation>>? MultiCountryTaxResult { get; set; }
    public TestResult<List<FinancialKPI>>? PerformanceTrackingResult { get; set; }

    // Services (simplified for BDD testing)
    public IAuthenticationService AuthenticationService { get; set; } = new TestAuthenticationService();
    public IBusinessPlanService BusinessPlanService { get; set; } = new TestBusinessPlanService();
    public IOrganizationService OrganizationService { get; set; } = new TestOrganizationService();
    public IAIService AIService { get; set; } = new TestAIService();
    public IExportService ExportService { get; set; } = new TestExportService();
    public IQuestionnaireService QuestionnaireService { get; set; } = new TestQuestionnaireService();
    public IOBNLService OBNLService { get; set; } = new TestOBNLService();
    public ITemplateService TemplateService { get; set; } = new TestTemplateService();
    public IFinancialService FinancialService { get; set; } = new TestFinancialService();

    private void SetupAutoFixture()
    {
        _fixture.Customize<User>(composer => composer
            .With(u => u.Email, _fixture.Create<string>())
            .With(u => u.IsEmailVerified, true)
            .With(u => u.CreatedAt, DateTime.UtcNow));

        _fixture.Customize<Organization>(composer => composer
            .With(o => o.Name, _fixture.Create<string>())
            .With(o => o.CreatedAt, DateTime.UtcNow));

        _fixture.Customize<BusinessPlan>(composer => composer
            .With(bp => bp.Title, _fixture.Create<string>())
            .With(bp => bp.CreatedAt, DateTime.UtcNow));
    }

    public async Task CreateTestUserAsync()
    {
        CurrentUser = _fixture.Create<User>();
        CurrentUser.Id = Guid.NewGuid();
        CurrentUser.Email = "test@example.com";
    }

    public async Task CreateVerifiedUserAsync()
    {
        await CreateTestUserAsync();
        CurrentUser!.IsEmailVerified = true;
    }

    public async Task CreateOrganizationOwnerAsync()
    {
        await CreateVerifiedUserAsync();
        CurrentOrganization = _fixture.Create<Organization>();
        CurrentOrganization.OwnerId = CurrentUser!.Id;
    }

    public async Task CreateOrganizationMemberAsync()
    {
        await CreateVerifiedUserAsync();
        TeamMembers = _fixture.CreateMany<User>(3).ToList();
    }

    public async Task CreateOBNLOrganizationAsync()
    {
        CurrentUser = new User { Id = Guid.NewGuid(), Email = "obnl@example.com" };
        CurrentOrganization = new Organization { Id = Guid.NewGuid(), Name = "OBNL Test Org", OwnerId = CurrentUser.Id };
        await Task.CompletedTask;
    }

    public RegistrationRequest CreateValidRegistrationRequest()
    {
        return new RegistrationRequest
        {
            Email = "newuser@example.com",
            Password = "ValidPassword123!",
            FirstName = "Test",
            LastName = "User"
        };
    }

    public LoginRequest CreateValidLoginRequest()
    {
        return new LoginRequest
        {
            Email = CurrentUser?.Email ?? "test@example.com",
            Password = "ValidPassword123!"
        };
    }

    public LoginRequest CreateInvalidLoginRequest()
    {
        return new LoginRequest
        {
            Email = CurrentUser?.Email ?? "test@example.com",
            Password = "WrongPassword"
        };
    }

    public QuestionnaireData CreateValidQuestionnaireData()
    {
        return new QuestionnaireData
        {
            Industry = CurrentIndustry ?? "Technology",
            BusinessType = "Startup",
            TargetMarket = "B2B",
            RevenueModel = "Subscription",
            FundingNeeds = 100000,
            Timeline = "6 months"
        };
    }

    public QuestionnaireData CreateIndustrySpecificQuestionnaireData(string industry)
    {
        return new QuestionnaireData
        {
            Industry = industry,
            BusinessType = "Startup",
            TargetMarket = "B2B",
            RevenueModel = "Subscription",
            FundingNeeds = 100000,
            Timeline = "6 months"
        };
    }

    public BusinessPlan CreateBasicBusinessPlan()
    {
        return _fixture.Create<BusinessPlan>();
    }

    public BusinessPlan CreateCompletedBusinessPlan()
    {
        var plan = _fixture.Create<BusinessPlan>();
        plan.IsComplete = true;
        plan.CompletedAt = DateTime.UtcNow;
        return plan;
    }

    public BusinessPlan CreateIncompleteBusinessPlan()
    {
        var plan = _fixture.Create<BusinessPlan>();
        plan.IsComplete = false;
        plan.ExecutiveSummary = null;
        return plan;
    }

    public string GenerateOrganizationName()
    {
        return $"Test Organization {Guid.NewGuid().ToString("N")[..8]}";
    }
}

// Test Models
public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Organization
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class BusinessPlan
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ExecutiveSummary { get; set; }
    public string? MarketAnalysis { get; set; }
    public object? FinancialProjections { get; set; }
    public string? RiskAssessment { get; set; }
    public bool IsComplete { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Template
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public string TargetAudience { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public bool IsDefault { get; set; }
    public int UsageCount { get; set; }
    public decimal Rating { get; set; }
    public int RatingCount { get; set; }
    public string Tags { get; set; } = string.Empty;
    public string PreviewImage { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime LastUsed { get; set; }
}

public class TemplateUsage
{
    public Guid Id { get; set; }
    public Guid TemplateId { get; set; }
    public Guid UserId { get; set; }
    public string UsageType { get; set; } = string.Empty;
    public DateTime UsedAt { get; set; }
}

public class TemplateRating
{
    public Guid Id { get; set; }
    public Guid TemplateId { get; set; }
    public Guid UserId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class TemplateCustomization
{
    public Guid Id { get; set; }
    public Guid TemplateId { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Customizations { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class TemplateAnalytics
{
    public Guid TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public int TotalViews { get; set; }
    public int TotalDownloads { get; set; }
    public int TotalUses { get; set; }
    public decimal AverageRating { get; set; }
    public int UniqueUsers { get; set; }
    public List<string> TopCountries { get; set; } = new();
    public DateTime LastUsed { get; set; }
}

// Test Result Wrapper
public class TestResult<T>
{
    public bool IsSuccess { get; set; }
    public T? Value { get; set; }
    public string? ErrorMessage { get; set; }

    public static TestResult<T> Success(T value)
    {
        return new TestResult<T> { IsSuccess = true, Value = value };
    }

    public static TestResult<T> Failure(string errorMessage)
    {
        return new TestResult<T> { IsSuccess = false, ErrorMessage = errorMessage };
    }
}

// Test Service Interfaces (simplified for BDD testing)
public interface IAuthenticationService
{
    Task<TestResult<User>> RegisterAsync(RegistrationRequest request);
    Task<TestResult<LoginResponse>> LoginAsync(LoginRequest request);
}

public interface IBusinessPlanService
{
    Task<TestResult<BusinessPlan>> GenerateBusinessPlanAsync(GenerateBusinessPlanRequest request);
    Task<TestResult<object>> UpdateBusinessPlanAsync(UpdateBusinessPlanRequest request);
    Task<TestResult<object>> ShareBusinessPlanAsync(ShareBusinessPlanRequest request);
}

public interface IOrganizationService
{
    Task<TestResult<Organization>> CreateOrganizationAsync(CreateOrganizationRequest request);
    Task<TestResult<InvitationResponse>> InviteMembersAsync(InviteMembersRequest request);
    Task<TestResult<object>> AcceptInvitationAsync(Guid invitationId);
    Task<TestResult<object>> UpdateMemberRoleAsync(UpdateMemberRoleRequest request);
    Task<TestResult<object>> UpgradeSubscriptionAsync(UpgradeSubscriptionRequest request);
}

public interface IAIService
{
    Task<TestResult<AIEnhancementResponse>> EnhanceBusinessPlanAsync(EnhanceBusinessPlanRequest request);
}

public interface IExportService
{
    Task<TestResult<ExportResponse>> ExportBusinessPlanAsync(ExportBusinessPlanRequest request);
}

public interface IQuestionnaireService
{
    Task<TestResult<QuestionnaireResponse>> CompleteQuestionnaireAsync(QuestionnaireData data);
}

public interface ITemplateService
{
    Task<TestResult<Template>> CreateTemplateAsync(CreateTemplateRequest request);
    Task<TestResult<Template>> GetTemplateByIdAsync(Guid id);
    Task<TestResult<List<Template>>> GetTemplatesByCategoryAsync(string category);
    Task<TestResult<List<Template>>> SearchTemplatesByIndustryAsync(string industry);
    Task<TestResult<List<Template>>> GetPublicTemplatesAsync();
    Task<TestResult<List<Template>>> GetUserTemplatesAsync();
    Task<TestResult<Template>> CloneTemplateAsync(Guid templateId, string newName);
    Task<TestResult<Template>> PublishTemplateAsync(Guid templateId);
    Task<TestResult<Template>> ArchiveTemplateAsync(Guid templateId);
    Task<TestResult<TemplateUsage>> RecordTemplateUsageAsync(Guid templateId, string usageType);
    Task<TestResult<TemplateRating>> RateTemplateAsync(Guid templateId, int rating, string comment);
    Task<TestResult<TemplateCustomization>> CustomizeTemplateAsync(TemplateCustomizationRequest request);
    Task<TestResult<List<Template>>> GetPopularTemplatesAsync(int count);
    Task<TestResult<List<Template>>> GetRecentTemplatesAsync(int count);
    Task<TestResult<List<Template>>> GetTemplatesByAuthorAsync(string author);
    Task<TestResult<TemplateAnalytics>> GetTemplateAnalyticsAsync(Guid templateId);
}

// Test Service Implementations (simplified for BDD testing)
public class TestAuthenticationService : IAuthenticationService
{
    public async Task<TestResult<User>> RegisterAsync(RegistrationRequest request)
    {
        await Task.Delay(100); // Simulate async operation
        return TestResult<User>.Success(new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            IsEmailVerified = false,
            CreatedAt = DateTime.UtcNow
        });
    }

    public async Task<TestResult<LoginResponse>> LoginAsync(LoginRequest request)
    {
        await Task.Delay(100); // Simulate async operation
        
        if (request.Password == "WrongPassword")
        {
            return TestResult<LoginResponse>.Failure("Invalid credentials");
        }

        return TestResult<LoginResponse>.Success(new LoginResponse
        {
            Token = "test-jwt-token",
            UserId = Guid.NewGuid()
        });
    }
}

public class TestBusinessPlanService : IBusinessPlanService
{
    public async Task<TestResult<BusinessPlan>> GenerateBusinessPlanAsync(GenerateBusinessPlanRequest request)
    {
        await Task.Delay(200); // Simulate async operation
        return TestResult<BusinessPlan>.Success(new BusinessPlan
        {
            Id = Guid.NewGuid(),
            Title = "Generated Business Plan",
            ExecutiveSummary = "Executive summary content",
            MarketAnalysis = "Market analysis content",
            FinancialProjections = new { Revenue = 100000, Expenses = 50000 },
            RiskAssessment = "Risk assessment content",
            IsComplete = true,
            CompletedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        });
    }

    public async Task<TestResult<object>> UpdateBusinessPlanAsync(UpdateBusinessPlanRequest request)
    {
        await Task.Delay(100);
        return TestResult<object>.Success(new { Success = true });
    }

    public async Task<TestResult<object>> ShareBusinessPlanAsync(ShareBusinessPlanRequest request)
    {
        await Task.Delay(100);
        return TestResult<object>.Success(new { Success = true });
    }
}

public class TestOrganizationService : IOrganizationService
{
    public async Task<TestResult<Organization>> CreateOrganizationAsync(CreateOrganizationRequest request)
    {
        await Task.Delay(100);
        return TestResult<Organization>.Success(new Organization
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            OwnerId = request.OwnerId,
            CreatedAt = DateTime.UtcNow
        });
    }

    public async Task<TestResult<InvitationResponse>> InviteMembersAsync(InviteMembersRequest request)
    {
        await Task.Delay(100);
        return TestResult<InvitationResponse>.Success(new InvitationResponse
        {
            InvitationIds = request.Emails.Select(_ => Guid.NewGuid()).ToList()
        });
    }

    public async Task<TestResult<object>> AcceptInvitationAsync(Guid invitationId)
    {
        await Task.Delay(100);
        return TestResult<object>.Success(new { Success = true });
    }

    public async Task<TestResult<object>> UpdateMemberRoleAsync(UpdateMemberRoleRequest request)
    {
        await Task.Delay(100);
        return TestResult<object>.Success(new { Success = true });
    }

    public async Task<TestResult<object>> UpgradeSubscriptionAsync(UpgradeSubscriptionRequest request)
    {
        await Task.Delay(100);
        return TestResult<object>.Success(new { Success = true });
    }
}

public class TestAIService : IAIService
{
    public async Task<TestResult<AIEnhancementResponse>> EnhanceBusinessPlanAsync(EnhanceBusinessPlanRequest request)
    {
        await Task.Delay(300); // Simulate AI processing time
        return TestResult<AIEnhancementResponse>.Success(new AIEnhancementResponse
        {
            Recommendations = new List<string>
            {
                "Consider expanding your target market",
                "Implement a customer retention strategy",
                "Diversify your revenue streams"
            }
        });
    }
}

public class TestExportService : IExportService
{
    public async Task<TestResult<ExportResponse>> ExportBusinessPlanAsync(ExportBusinessPlanRequest request)
    {
        await Task.Delay(200);
        return TestResult<ExportResponse>.Success(new ExportResponse
        {
            FileFormat = request.Format,
            FileSize = 1024 * 1024, // 1MB
            IsEditable = request.Format == "DOCX"
        });
    }
}

public class TestQuestionnaireService : IQuestionnaireService
{
    public async Task<TestResult<QuestionnaireResponse>> CompleteQuestionnaireAsync(QuestionnaireData data)
    {
        await Task.Delay(100);
        return TestResult<QuestionnaireResponse>.Success(new QuestionnaireResponse
        {
            Id = Guid.NewGuid(),
            IsComplete = true
        });
    }
}

// Response Models
public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public Guid UserId { get; set; }
}

public class AIEnhancementResponse
{
    public List<string> Recommendations { get; set; } = new();
}

public class ExportResponse
{
    public string FileFormat { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public bool IsEditable { get; set; }
}

public class InvitationResponse
{
    public List<Guid> InvitationIds { get; set; } = new();
}

public class QuestionnaireResponse
{
    public Guid Id { get; set; }
    public bool IsComplete { get; set; }
}

// Request Models
public class RegistrationRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class GenerateBusinessPlanRequest
{
    public Guid? QuestionnaireId { get; set; }
    public Guid? BusinessPlanId { get; set; }
    public Guid UserId { get; set; }
}

public class UpdateBusinessPlanRequest
{
    public Guid BusinessPlanId { get; set; }
    public Dictionary<string, object> Updates { get; set; } = new();
}

public class ShareBusinessPlanRequest
{
    public Guid BusinessPlanId { get; set; }
    public string[] ShareWithEmails { get; set; } = Array.Empty<string>();
    public string PermissionLevel { get; set; } = string.Empty;
    public DateTime ExpirationDate { get; set; }
}

public class CreateOrganizationRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
}

public class InviteMembersRequest
{
    public Guid OrganizationId { get; set; }
    public string[] Emails { get; set; } = Array.Empty<string>();
    public string Role { get; set; } = string.Empty;
}

public class UpdateMemberRoleRequest
{
    public Guid OrganizationId { get; set; }
    public Guid MemberId { get; set; }
    public string NewRole { get; set; } = string.Empty;
}

public class UpgradeSubscriptionRequest
{
    public Guid OrganizationId { get; set; }
    public string SubscriptionType { get; set; } = string.Empty;
    public string BillingCycle { get; set; } = string.Empty;
}

public class EnhanceBusinessPlanRequest
{
    public Guid BusinessPlanId { get; set; }
    public string EnhancementType { get; set; } = string.Empty;
}

public class ExportBusinessPlanRequest
{
    public Guid BusinessPlanId { get; set; }
    public string Format { get; set; } = string.Empty;
    public bool IncludeCollaborationHistory { get; set; }
}

public class QuestionnaireData
{
    public string Industry { get; set; } = string.Empty;
    public string BusinessType { get; set; } = string.Empty;
    public string TargetMarket { get; set; } = string.Empty;
    public string RevenueModel { get; set; } = string.Empty;
    public decimal FundingNeeds { get; set; }
    public string Timeline { get; set; } = string.Empty;
}

// OBNL-related models
public record OBNLBusinessPlan
{
    public Guid Id { get; init; }
    public Guid OrganizationId { get; init; }
    public string OBNLType { get; init; } = string.Empty;
    public string Mission { get; init; } = string.Empty;
    public string Vision { get; init; } = string.Empty;
    public string Values { get; init; } = string.Empty;
    public decimal FundingRequirements { get; init; }
    public string FundingPurpose { get; init; } = string.Empty;
    public string ComplianceStatus { get; init; } = string.Empty;
    public string LegalStructure { get; init; } = string.Empty;
    public string RegistrationNumber { get; init; } = string.Empty;
    public DateTime RegistrationDate { get; init; }
    public string GoverningBody { get; init; } = string.Empty;
    public string BoardComposition { get; init; } = string.Empty;
    public string StakeholderEngagement { get; init; } = string.Empty;
    public string ImpactMeasurement { get; init; } = string.Empty;
    public string SustainabilityStrategy { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public string CreatedBy { get; init; } = string.Empty;
    public string UpdatedBy { get; init; } = string.Empty;
}

public record ComplianceAnalysis
{
    public string Status { get; init; } = string.Empty;
    public string Level { get; init; } = string.Empty;
    public List<string> Requirements { get; init; } = new();
    public List<string> Recommendations { get; init; } = new();
}

public record GrantApplication
{
    public Guid Id { get; init; }
    public Guid OBNLBusinessPlanId { get; init; }
    public string GrantName { get; init; } = string.Empty;
    public string GrantingOrganization { get; init; } = string.Empty;
    public string GrantType { get; init; } = string.Empty;
    public decimal RequestedAmount { get; init; }
    public decimal MatchingFunds { get; init; }
    public string ProjectDescription { get; init; } = string.Empty;
    public string Objectives { get; init; } = string.Empty;
    public string ExpectedOutcomes { get; init; } = string.Empty;
    public string TargetPopulation { get; init; } = string.Empty;
    public string GeographicScope { get; init; } = string.Empty;
    public string Timeline { get; init; } = string.Empty;
    public string BudgetBreakdown { get; init; } = string.Empty;
    public string EvaluationPlan { get; init; } = string.Empty;
    public string SustainabilityPlan { get; init; } = string.Empty;
    public DateTime ApplicationDeadline { get; init; }
    public DateTime SubmissionDate { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Decision { get; init; } = string.Empty;
    public string Notes { get; init; } = string.Empty;
}

public record ImpactMeasurement
{
    public Guid Id { get; init; }
    public Guid OBNLBusinessPlanId { get; init; }
    public string MetricName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string MeasurementType { get; init; } = string.Empty;
    public string UnitOfMeasurement { get; init; } = string.Empty;
    public decimal BaselineValue { get; init; }
    public decimal TargetValue { get; init; }
    public decimal CurrentValue { get; init; }
    public string DataSource { get; init; } = string.Empty;
    public string CollectionMethod { get; init; } = string.Empty;
    public string Frequency { get; init; } = string.Empty;
    public string ResponsibleParty { get; init; } = string.Empty;
    public DateTime LastMeasurement { get; init; }
    public DateTime NextMeasurement { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Notes { get; init; } = string.Empty;
}

public record SustainabilityPlan
{
    public string Initiatives { get; init; } = string.Empty;
    public string EnvironmentalImpact { get; init; } = string.Empty;
    public string ViabilityStrategies { get; init; } = string.Empty;
    public string StakeholderEngagement { get; init; } = string.Empty;
}

public record StakeholderEngagement
{
    public List<string> KeyStakeholders { get; init; } = new();
    public List<string> EngagementStrategies { get; init; } = new();
    public List<string> CommunicationPlans { get; init; } = new();
    public List<string> RelationshipTracking { get; init; } = new();
}

public record GovernanceStructure
{
    public string BoardComposition { get; init; } = string.Empty;
    public List<string> GovernancePolicies { get; init; } = new();
    public List<string> DecisionMakingFrameworks { get; init; } = new();
    public List<string> AccountabilityMeasures { get; init; } = new();
}

public record ComplianceReport
{
    public List<string> RegulatoryReports { get; init; } = new();
    public List<string> FinancialStatements { get; init; } = new();
    public List<string> ImpactReports { get; init; } = new();
    public string ComplianceStatus { get; init; } = string.Empty;
}

// OBNL Request/Response DTOs
public record CreateOBNLPlanRequest
{
    public Guid OrganizationId { get; init; }
    public string OBNLType { get; init; } = string.Empty;
    public string Mission { get; init; } = string.Empty;
    public string Vision { get; init; } = string.Empty;
    public string Values { get; init; } = string.Empty;
    public decimal FundingRequirements { get; init; }
    public string FundingPurpose { get; init; } = string.Empty;
    public string LegalStructure { get; init; } = string.Empty;
    public string RegistrationNumber { get; init; } = string.Empty;
    public DateTime RegistrationDate { get; init; }
    public string GoverningBody { get; init; } = string.Empty;
    public string BoardComposition { get; init; } = string.Empty;
    public string StakeholderEngagement { get; init; } = string.Empty;
    public string ImpactMeasurement { get; init; } = string.Empty;
    public string SustainabilityStrategy { get; init; } = string.Empty;
    public string CreatedBy { get; init; } = string.Empty;
}

public record CreateGrantApplicationRequest
{
    public Guid OBNLBusinessPlanId { get; init; }
    public string GrantName { get; init; } = string.Empty;
    public string GrantingOrganization { get; init; } = string.Empty;
    public string GrantType { get; init; } = string.Empty;
    public decimal RequestedAmount { get; init; }
    public decimal MatchingFunds { get; init; }
    public string ProjectDescription { get; init; } = string.Empty;
    public string Objectives { get; init; } = string.Empty;
    public string ExpectedOutcomes { get; init; } = string.Empty;
    public string TargetPopulation { get; init; } = string.Empty;
    public string GeographicScope { get; init; } = string.Empty;
    public string Timeline { get; init; } = string.Empty;
    public string BudgetBreakdown { get; init; } = string.Empty;
    public string EvaluationPlan { get; init; } = string.Empty;
    public string SustainabilityPlan { get; init; } = string.Empty;
    public DateTime ApplicationDeadline { get; init; }
    public DateTime SubmissionDate { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Notes { get; init; } = string.Empty;
}

public record CreateImpactMeasurementRequest
{
    public Guid OBNLBusinessPlanId { get; init; }
    public string MetricName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string MeasurementType { get; init; } = string.Empty;
    public string UnitOfMeasurement { get; init; } = string.Empty;
    public decimal BaselineValue { get; init; }
    public decimal TargetValue { get; init; }
    public decimal CurrentValue { get; init; }
    public string DataSource { get; init; } = string.Empty;
    public string CollectionMethod { get; init; } = string.Empty;
    public string Frequency { get; init; } = string.Empty;
    public string ResponsibleParty { get; init; } = string.Empty;
    public DateTime LastMeasurement { get; init; }
    public DateTime NextMeasurement { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Notes { get; init; } = string.Empty;
}

// OBNL Service Interface
public interface IOBNLService
{
    Task<TestResult<OBNLBusinessPlan>> CreateOBNLPlanAsync(CreateOBNLPlanRequest request);
    Task<TestResult<OBNLBusinessPlan>> GetOBNLPlanAsync(Guid planId);
    Task<TestResult<List<OBNLBusinessPlan>>> GetOBNLPlansByOrganizationAsync(Guid organizationId);
    Task<TestResult<ComplianceAnalysis>> AnalyzeComplianceAsync(Guid planId);
    Task<TestResult<GrantApplication>> CreateGrantApplicationAsync(CreateGrantApplicationRequest request);
    Task<TestResult<List<GrantApplication>>> GetGrantApplicationsAsync(Guid planId);
    Task<TestResult<ImpactMeasurement>> CreateImpactMeasurementAsync(CreateImpactMeasurementRequest request);
    Task<TestResult<List<ImpactMeasurement>>> GetImpactMeasurementsAsync(Guid planId);
    Task<TestResult<SustainabilityPlan>> GenerateSustainabilityPlanAsync(Guid planId);
    Task<TestResult<StakeholderEngagement>> ConfigureStakeholderEngagementAsync(Guid planId);
    Task<TestResult<GovernanceStructure>> SetUpGovernanceStructureAsync(Guid planId);
    Task<TestResult<ComplianceReport>> GenerateComplianceReportsAsync(Guid planId);
}

// OBNL Service Implementation
public class TestOBNLService : IOBNLService
{
    public Task<TestResult<OBNLBusinessPlan>> CreateOBNLPlanAsync(CreateOBNLPlanRequest request)
    {
        var plan = new OBNLBusinessPlan
        {
            Id = Guid.NewGuid(),
            OrganizationId = request.OrganizationId,
            OBNLType = request.OBNLType,
            Mission = request.Mission,
            Vision = request.Vision,
            Values = request.Values,
            FundingRequirements = request.FundingRequirements,
            FundingPurpose = request.FundingPurpose,
            LegalStructure = request.LegalStructure,
            RegistrationNumber = request.RegistrationNumber,
            RegistrationDate = request.RegistrationDate,
            GoverningBody = request.GoverningBody,
            BoardComposition = request.BoardComposition,
            StakeholderEngagement = request.StakeholderEngagement,
            ImpactMeasurement = request.ImpactMeasurement,
            SustainabilityStrategy = request.SustainabilityStrategy,
            ComplianceStatus = "Pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = request.CreatedBy,
            UpdatedBy = request.CreatedBy
        };
        return Task.FromResult(TestResult<OBNLBusinessPlan>.Success(plan));
    }

    public Task<TestResult<OBNLBusinessPlan>> GetOBNLPlanAsync(Guid planId)
    {
        var plan = new OBNLBusinessPlan
        {
            Id = planId,
            OrganizationId = Guid.NewGuid(),
            OBNLType = "Charitable Organization",
            Mission = "Test Mission",
            Vision = "Test Vision",
            Values = "Test Values",
            FundingRequirements = 100000,
            FundingPurpose = "Test Purpose",
            LegalStructure = "Non-Profit Corporation",
            RegistrationNumber = "123456789",
            RegistrationDate = DateTime.UtcNow,
            GoverningBody = "Board of Directors",
            BoardComposition = "5 members",
            StakeholderEngagement = "Community engagement plan",
            ImpactMeasurement = "Impact measurement framework",
            SustainabilityStrategy = "Sustainability strategy",
            ComplianceStatus = "Pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "test@example.com",
            UpdatedBy = "test@example.com"
        };
        return Task.FromResult(TestResult<OBNLBusinessPlan>.Success(plan));
    }

    public Task<TestResult<List<OBNLBusinessPlan>>> GetOBNLPlansByOrganizationAsync(Guid organizationId)
    {
        var plans = new List<OBNLBusinessPlan>
        {
            new OBNLBusinessPlan
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                OBNLType = "Charitable Organization",
                Mission = "Test Mission",
                Vision = "Test Vision",
                Values = "Test Values",
                FundingRequirements = 100000,
                FundingPurpose = "Test Purpose",
                LegalStructure = "Non-Profit Corporation",
                RegistrationNumber = "123456789",
                RegistrationDate = DateTime.UtcNow,
                GoverningBody = "Board of Directors",
                BoardComposition = "5 members",
                StakeholderEngagement = "Community engagement plan",
                ImpactMeasurement = "Impact measurement framework",
                SustainabilityStrategy = "Sustainability strategy",
                ComplianceStatus = "Pending",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "test@example.com",
                UpdatedBy = "test@example.com"
            }
        };
        return Task.FromResult(TestResult<List<OBNLBusinessPlan>>.Success(plans));
    }

    public Task<TestResult<ComplianceAnalysis>> AnalyzeComplianceAsync(Guid planId)
    {
        var analysis = new ComplianceAnalysis
        {
            Status = "Compliant",
            Level = "Full",
            Requirements = new List<string> { "Tax exemption", "Annual reporting", "Board governance" },
            Recommendations = new List<string> { "Maintain tax-exempt status", "Regular board meetings", "Financial transparency" }
        };
        return Task.FromResult(TestResult<ComplianceAnalysis>.Success(analysis));
    }

    public Task<TestResult<GrantApplication>> CreateGrantApplicationAsync(CreateGrantApplicationRequest request)
    {
        var application = new GrantApplication
        {
            Id = Guid.NewGuid(),
            OBNLBusinessPlanId = request.OBNLBusinessPlanId,
            GrantName = request.GrantName,
            GrantingOrganization = request.GrantingOrganization,
            GrantType = request.GrantType,
            RequestedAmount = request.RequestedAmount,
            MatchingFunds = request.MatchingFunds,
            ProjectDescription = request.ProjectDescription,
            Objectives = request.Objectives,
            ExpectedOutcomes = request.ExpectedOutcomes,
            TargetPopulation = request.TargetPopulation,
            GeographicScope = request.GeographicScope,
            Timeline = request.Timeline,
            BudgetBreakdown = request.BudgetBreakdown,
            EvaluationPlan = request.EvaluationPlan,
            SustainabilityPlan = request.SustainabilityPlan,
            ApplicationDeadline = request.ApplicationDeadline,
            SubmissionDate = request.SubmissionDate,
            Status = request.Status,
            Decision = "Pending",
            Notes = request.Notes
        };
        return Task.FromResult(TestResult<GrantApplication>.Success(application));
    }

    public Task<TestResult<List<GrantApplication>>> GetGrantApplicationsAsync(Guid planId)
    {
        var applications = new List<GrantApplication>
        {
            new GrantApplication
            {
                Id = Guid.NewGuid(),
                OBNLBusinessPlanId = planId,
                GrantName = "Community Development Grant",
                GrantingOrganization = "Government Foundation",
                GrantType = "Program Grant",
                RequestedAmount = 100000,
                MatchingFunds = 25000,
                ProjectDescription = "Community development project",
                Objectives = "Improve community services",
                ExpectedOutcomes = "Increased community engagement",
                TargetPopulation = "Local community members",
                GeographicScope = "Local area",
                Timeline = "12 months",
                BudgetBreakdown = "Detailed budget breakdown",
                EvaluationPlan = "Comprehensive evaluation plan",
                SustainabilityPlan = "Long-term sustainability strategy",
                ApplicationDeadline = DateTime.UtcNow.AddDays(30),
                SubmissionDate = DateTime.UtcNow,
                Status = "Draft",
                Decision = "Pending",
                Notes = "Grant application notes"
            }
        };
        return Task.FromResult(TestResult<List<GrantApplication>>.Success(applications));
    }

    public Task<TestResult<ImpactMeasurement>> CreateImpactMeasurementAsync(CreateImpactMeasurementRequest request)
    {
        var measurement = new ImpactMeasurement
        {
            Id = Guid.NewGuid(),
            OBNLBusinessPlanId = request.OBNLBusinessPlanId,
            MetricName = request.MetricName,
            Description = request.Description,
            MeasurementType = request.MeasurementType,
            UnitOfMeasurement = request.UnitOfMeasurement,
            BaselineValue = request.BaselineValue,
            TargetValue = request.TargetValue,
            CurrentValue = request.CurrentValue,
            DataSource = request.DataSource,
            CollectionMethod = request.CollectionMethod,
            Frequency = request.Frequency,
            ResponsibleParty = request.ResponsibleParty,
            LastMeasurement = request.LastMeasurement,
            NextMeasurement = request.NextMeasurement,
            Status = request.Status,
            Notes = request.Notes
        };
        return Task.FromResult(TestResult<ImpactMeasurement>.Success(measurement));
    }

    public Task<TestResult<List<ImpactMeasurement>>> GetImpactMeasurementsAsync(Guid planId)
    {
        var measurements = new List<ImpactMeasurement>
        {
            new ImpactMeasurement
            {
                Id = Guid.NewGuid(),
                OBNLBusinessPlanId = planId,
                MetricName = "Community Impact",
                Description = "Measure community impact",
                MeasurementType = "Quantitative",
                UnitOfMeasurement = "Number of people served",
                BaselineValue = 100,
                TargetValue = 500,
                CurrentValue = 150,
                DataSource = "Program records",
                CollectionMethod = "Monthly surveys",
                Frequency = "Monthly",
                ResponsibleParty = "Program Manager",
                LastMeasurement = DateTime.UtcNow.AddDays(-30),
                NextMeasurement = DateTime.UtcNow.AddDays(30),
                Status = "Active",
                Notes = "Impact measurement notes"
            }
        };
        return Task.FromResult(TestResult<List<ImpactMeasurement>>.Success(measurements));
    }

    public Task<TestResult<SustainabilityPlan>> GenerateSustainabilityPlanAsync(Guid planId)
    {
        var plan = new SustainabilityPlan
        {
            Initiatives = "Green energy, waste reduction, community partnerships",
            EnvironmentalImpact = "Carbon footprint reduction, waste minimization",
            ViabilityStrategies = "Diversified funding, volunteer engagement, community support",
            StakeholderEngagement = "Regular community meetings, volunteer programs, donor relations"
        };
        return Task.FromResult(TestResult<SustainabilityPlan>.Success(plan));
    }

    public Task<TestResult<StakeholderEngagement>> ConfigureStakeholderEngagementAsync(Guid planId)
    {
        var engagement = new StakeholderEngagement
        {
            KeyStakeholders = new List<string> { "Community members", "Donors", "Volunteers", "Board members" },
            EngagementStrategies = new List<string> { "Regular meetings", "Newsletters", "Social media", "Events" },
            CommunicationPlans = new List<string> { "Monthly updates", "Annual reports", "Newsletter", "Website" },
            RelationshipTracking = new List<string> { "Contact database", "Engagement history", "Feedback collection", "Relationship strength" }
        };
        return Task.FromResult(TestResult<StakeholderEngagement>.Success(engagement));
    }

    public Task<TestResult<GovernanceStructure>> SetUpGovernanceStructureAsync(Guid planId)
    {
        var structure = new GovernanceStructure
        {
            BoardComposition = "5 members with diverse backgrounds",
            GovernancePolicies = new List<string> { "Conflict of interest policy", "Financial oversight", "Strategic planning" },
            DecisionMakingFrameworks = new List<string> { "Consensus building", "Majority vote", "Executive decisions", "Committee structure" },
            AccountabilityMeasures = new List<string> { "Annual reviews", "Performance metrics", "Transparency reports", "Stakeholder feedback" }
        };
        return Task.FromResult(TestResult<GovernanceStructure>.Success(structure));
    }

    public Task<TestResult<ComplianceReport>> GenerateComplianceReportsAsync(Guid planId)
    {
        var report = new ComplianceReport
        {
            RegulatoryReports = new List<string> { "Annual tax return", "Charity registration", "Financial statements" },
            FinancialStatements = new List<string> { "Balance sheet", "Income statement", "Cash flow statement" },
            ImpactReports = new List<string> { "Program outcomes", "Community impact", "Success metrics" },
            ComplianceStatus = "Compliant"
        };
        return Task.FromResult(TestResult<ComplianceReport>.Success(report));
    }
}

// Template Service Implementation
public class TestTemplateService : ITemplateService
{
    public Task<TestResult<Template>> CreateTemplateAsync(CreateTemplateRequest request)
    {
        var template = new Template
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Content = "Template content",
            Category = request.Category,
            Type = request.Type,
            Industry = request.Industry,
            TargetAudience = request.TargetAudience,
            Language = request.Language,
            Country = request.Country,
            IsPublic = request.IsPublic,
            Tags = request.Tags,
            PreviewImage = request.PreviewImage,
            Author = request.Author,
            Version = "1.0",
            Status = "Draft",
            UsageCount = 0,
            Rating = 0,
            RatingCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            LastUsed = DateTime.UtcNow
        };
        return Task.FromResult(TestResult<Template>.Success(template));
    }

    public Task<TestResult<Template>> GetTemplateByIdAsync(Guid id)
    {
        var template = new Template
        {
            Id = id,
            Name = "Sample Template",
            Description = "A sample template",
            Content = "Template content",
            Category = "BusinessPlan",
            Type = "Standard",
            Industry = "Technology",
            IsPublic = true,
            UsageCount = 10,
            Rating = 4.5m,
            RatingCount = 5,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            UpdatedAt = DateTime.UtcNow,
            LastUsed = DateTime.UtcNow.AddDays(-1)
        };
        return Task.FromResult(TestResult<Template>.Success(template));
    }

    public Task<TestResult<List<Template>>> GetTemplatesByCategoryAsync(string category)
    {
        var templates = new List<Template>
        {
            new Template
            {
                Id = Guid.NewGuid(),
                Name = "Business Plan Template",
                Description = "Comprehensive business plan template",
                Category = category,
                Type = "Standard",
                Industry = "General",
                IsPublic = true,
                UsageCount = 25,
                Rating = 4.2m,
                RatingCount = 8,
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                UpdatedAt = DateTime.UtcNow,
                LastUsed = DateTime.UtcNow.AddDays(-2)
            }
        };
        return Task.FromResult(TestResult<List<Template>>.Success(templates));
    }

    public Task<TestResult<List<Template>>> SearchTemplatesByIndustryAsync(string industry)
    {
        var templates = new List<Template>
        {
            new Template
            {
                Id = Guid.NewGuid(),
                Name = $"{industry} Business Template",
                Description = $"Template for {industry} businesses",
                Category = "BusinessPlan",
                Type = "IndustrySpecific",
                Industry = industry,
                IsPublic = true,
                UsageCount = 15,
                Rating = 4.0m,
                RatingCount = 3,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow,
                LastUsed = DateTime.UtcNow.AddDays(-3)
            }
        };
        return Task.FromResult(TestResult<List<Template>>.Success(templates));
    }

    public Task<TestResult<List<Template>>> GetPublicTemplatesAsync()
    {
        var templates = new List<Template>
        {
            new Template
            {
                Id = Guid.NewGuid(),
                Name = "Public Business Template",
                Description = "A public template for general use",
                Category = "BusinessPlan",
                Type = "Standard",
                Industry = "General",
                IsPublic = true,
                UsageCount = 50,
                Rating = 4.5m,
                RatingCount = 12,
                CreatedAt = DateTime.UtcNow.AddDays(-20),
                UpdatedAt = DateTime.UtcNow,
                LastUsed = DateTime.UtcNow.AddDays(-1)
            }
        };
        return Task.FromResult(TestResult<List<Template>>.Success(templates));
    }

    public Task<TestResult<List<Template>>> GetUserTemplatesAsync()
    {
        var templates = new List<Template>
        {
            new Template
            {
                Id = Guid.NewGuid(),
                Name = "My Custom Template",
                Description = "A custom template I created",
                Category = "BusinessPlan",
                Type = "Custom",
                Industry = "Technology",
                IsPublic = false,
                UsageCount = 5,
                Rating = 0,
                RatingCount = 0,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow,
                LastUsed = DateTime.UtcNow.AddDays(-1)
            }
        };
        return Task.FromResult(TestResult<List<Template>>.Success(templates));
    }

    public Task<TestResult<Template>> CloneTemplateAsync(Guid templateId, string newName)
    {
        var clonedTemplate = new Template
        {
            Id = Guid.NewGuid(),
            Name = newName,
            Description = "Cloned template",
            Content = "Template content",
            Category = "BusinessPlan",
            Type = "Custom",
            Industry = "General",
            IsPublic = false,
            UsageCount = 0,
            Rating = 0,
            RatingCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            LastUsed = DateTime.UtcNow
        };
        return Task.FromResult(TestResult<Template>.Success(clonedTemplate));
    }

    public Task<TestResult<Template>> PublishTemplateAsync(Guid templateId)
    {
        var template = new Template
        {
            Id = templateId,
            Name = "Published Template",
            Description = "A published template",
            Category = "BusinessPlan",
            Type = "Standard",
            Industry = "General",
            IsPublic = true,
            Status = "Published",
            UsageCount = 0,
            Rating = 0,
            RatingCount = 0,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            UpdatedAt = DateTime.UtcNow,
            LastUsed = DateTime.UtcNow
        };
        return Task.FromResult(TestResult<Template>.Success(template));
    }

    public Task<TestResult<Template>> ArchiveTemplateAsync(Guid templateId)
    {
        var template = new Template
        {
            Id = templateId,
            Name = "Archived Template",
            Description = "An archived template",
            Category = "BusinessPlan",
            Type = "Standard",
            Industry = "General",
            IsPublic = false,
            Status = "Archived",
            UsageCount = 10,
            Rating = 4.0m,
            RatingCount = 2,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            UpdatedAt = DateTime.UtcNow,
            LastUsed = DateTime.UtcNow.AddDays(-15)
        };
        return Task.FromResult(TestResult<Template>.Success(template));
    }

    public Task<TestResult<TemplateUsage>> RecordTemplateUsageAsync(Guid templateId, string usageType)
    {
        var usage = new TemplateUsage
        {
            Id = Guid.NewGuid(),
            TemplateId = templateId,
            UserId = Guid.NewGuid(),
            UsageType = usageType,
            UsedAt = DateTime.UtcNow
        };
        return Task.FromResult(TestResult<TemplateUsage>.Success(usage));
    }

    public Task<TestResult<TemplateRating>> RateTemplateAsync(Guid templateId, int rating, string comment)
    {
        var templateRating = new TemplateRating
        {
            Id = Guid.NewGuid(),
            TemplateId = templateId,
            UserId = Guid.NewGuid(),
            Rating = rating,
            Comment = comment,
            CreatedAt = DateTime.UtcNow
        };
        return Task.FromResult(TestResult<TemplateRating>.Success(templateRating));
    }

    public Task<TestResult<TemplateCustomization>> CustomizeTemplateAsync(TemplateCustomizationRequest request)
    {
        var customization = new TemplateCustomization
        {
            Id = Guid.NewGuid(),
            TemplateId = request.TemplateId,
            UserId = Guid.NewGuid(),
            Name = "Custom Template",
            Customizations = request.Customizations,
            CreatedAt = DateTime.UtcNow
        };
        return Task.FromResult(TestResult<TemplateCustomization>.Success(customization));
    }

    public Task<TestResult<List<Template>>> GetPopularTemplatesAsync(int count)
    {
        var templates = new List<Template>
        {
            new Template
            {
                Id = Guid.NewGuid(),
                Name = "Popular Template 1",
                Description = "Most popular template",
                Category = "BusinessPlan",
                Type = "Standard",
                Industry = "General",
                IsPublic = true,
                UsageCount = 100,
                Rating = 4.8m,
                RatingCount = 25,
                CreatedAt = DateTime.UtcNow.AddDays(-60),
                UpdatedAt = DateTime.UtcNow,
                LastUsed = DateTime.UtcNow.AddDays(-1)
            },
            new Template
            {
                Id = Guid.NewGuid(),
                Name = "Popular Template 2",
                Description = "Second most popular template",
                Category = "BusinessPlan",
                Type = "Standard",
                Industry = "Technology",
                IsPublic = true,
                UsageCount = 75,
                Rating = 4.5m,
                RatingCount = 20,
                CreatedAt = DateTime.UtcNow.AddDays(-45),
                UpdatedAt = DateTime.UtcNow,
                LastUsed = DateTime.UtcNow.AddDays(-2)
            }
        };
        return Task.FromResult(TestResult<List<Template>>.Success(templates));
    }

    public Task<TestResult<List<Template>>> GetRecentTemplatesAsync(int count)
    {
        var templates = new List<Template>
        {
            new Template
            {
                Id = Guid.NewGuid(),
                Name = "Recent Template 1",
                Description = "Most recent template",
                Category = "BusinessPlan",
                Type = "Standard",
                Industry = "General",
                IsPublic = true,
                UsageCount = 5,
                Rating = 4.0m,
                RatingCount = 2,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow,
                LastUsed = DateTime.UtcNow
            },
            new Template
            {
                Id = Guid.NewGuid(),
                Name = "Recent Template 2",
                Description = "Second most recent template",
                Category = "BusinessPlan",
                Type = "Standard",
                Industry = "Healthcare",
                IsPublic = true,
                UsageCount = 3,
                Rating = 0,
                RatingCount = 0,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow,
                LastUsed = DateTime.UtcNow.AddDays(-1)
            }
        };
        return Task.FromResult(TestResult<List<Template>>.Success(templates));
    }

    public Task<TestResult<List<Template>>> GetTemplatesByAuthorAsync(string author)
    {
        var templates = new List<Template>
        {
            new Template
            {
                Id = Guid.NewGuid(),
                Name = "Author Template 1",
                Description = "Template by specific author",
                Category = "BusinessPlan",
                Type = "Standard",
                Industry = "General",
                Author = author,
                IsPublic = true,
                UsageCount = 20,
                Rating = 4.2m,
                RatingCount = 5,
                CreatedAt = DateTime.UtcNow.AddDays(-20),
                UpdatedAt = DateTime.UtcNow,
                LastUsed = DateTime.UtcNow.AddDays(-5)
            }
        };
        return Task.FromResult(TestResult<List<Template>>.Success(templates));
    }

    public Task<TestResult<TemplateAnalytics>> GetTemplateAnalyticsAsync(Guid templateId)
    {
        var analytics = new TemplateAnalytics
        {
            TemplateId = templateId,
            TemplateName = "Sample Template",
            TotalViews = 150,
            TotalDownloads = 75,
            TotalUses = 50,
            AverageRating = 4.3m,
            UniqueUsers = 45,
            TopCountries = new List<string> { "United States", "Canada", "United Kingdom" },
            LastUsed = DateTime.UtcNow.AddDays(-1)
        };
        return Task.FromResult(TestResult<TemplateAnalytics>.Success(analytics));
    }
}

// Template Request Models
public class CreateTemplateRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public string TargetAudience { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public string Tags { get; set; } = string.Empty;
    public string PreviewImage { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
}

public class TemplateCustomizationRequest
{
    public Guid TemplateId { get; set; }
    public string Customizations { get; set; } = string.Empty;
}

// Financial Models
public class FinancialProjection
{
    public Guid Id { get; set; }
    public Guid BusinessPlanId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ProjectionType { get; set; } = string.Empty;
    public string Scenario { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal BaseAmount { get; set; }
    public string Category { get; set; } = string.Empty;
    public string SubCategory { get; set; } = string.Empty;
    public bool IsRecurring { get; set; }
    public string Frequency { get; set; } = string.Empty;
    public decimal GrowthRate { get; set; }
    public string Assumptions { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class InvestmentAnalysis
{
    public Guid Id { get; set; }
    public Guid BusinessPlanId { get; set; }
    public string AnalysisType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal InitialInvestment { get; set; }
    public decimal ExpectedReturn { get; set; }
    public decimal NetPresentValue { get; set; }
    public decimal InternalRateOfReturn { get; set; }
    public decimal PaybackPeriod { get; set; }
    public decimal ReturnOnInvestment { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal DiscountRate { get; set; }
    public int AnalysisPeriod { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
    public string InvestmentType { get; set; } = string.Empty;
    public string InvestorType { get; set; } = string.Empty;
    public decimal Valuation { get; set; }
    public decimal EquityOffering { get; set; }
    public decimal FundingRequired { get; set; }
    public string FundingStage { get; set; } = string.Empty;
    public string Assumptions { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class TaxCalculation
{
    public Guid Id { get; set; }
    public Guid FinancialProjectionId { get; set; }
    public Guid TaxRuleId { get; set; }
    public string TaxName { get; set; } = string.Empty;
    public string TaxType { get; set; } = string.Empty;
    public decimal TaxableAmount { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string CalculationMethod { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public DateTime TaxPeriod { get; set; }
    public bool IsPaid { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string PaymentReference { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class FinancialKPI
{
    public Guid Id { get; set; }
    public Guid BusinessPlanId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string MetricType { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TargetValue { get; set; }
    public decimal PreviousValue { get; set; }
    public decimal ChangePercentage { get; set; }
    public string Trend { get; set; } = string.Empty;
    public string Benchmark { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CashFlowReport
{
    public Guid BusinessPlanId { get; set; }
    public string Period { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal OpeningBalance { get; set; }
    public decimal CashInflows { get; set; }
    public decimal CashOutflows { get; set; }
    public decimal NetCashFlow { get; set; }
    public decimal ClosingBalance { get; set; }
    public List<CashFlowItem> Items { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}

public class CashFlowItem
{
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
}

public class ProfitLossReport
{
    public Guid BusinessPlanId { get; set; }
    public string Period { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public decimal CostOfGoodsSold { get; set; }
    public decimal GrossProfit { get; set; }
    public decimal OperatingExpenses { get; set; }
    public decimal OperatingIncome { get; set; }
    public decimal InterestExpense { get; set; }
    public decimal TaxExpense { get; set; }
    public decimal NetIncome { get; set; }
    public List<ProfitLossItem> Items { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}

public class ProfitLossItem
{
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
}

public class BalanceSheetReport
{
    public Guid BusinessPlanId { get; set; }
    public string Period { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal TotalAssets { get; set; }
    public decimal TotalLiabilities { get; set; }
    public decimal TotalEquity { get; set; }
    public List<BalanceSheetItem> Assets { get; set; } = new();
    public List<BalanceSheetItem> Liabilities { get; set; } = new();
    public List<BalanceSheetItem> Equity { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}

public class BalanceSheetItem
{
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
}

public class ScenarioAnalysis
{
    public string Scenario { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public decimal Expenses { get; set; }
    public decimal NetIncome { get; set; }
    public decimal CashFlow { get; set; }
    public decimal ROI { get; set; }
    public decimal NPV { get; set; }
    public decimal IRR { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string RiskLevel { get; set; } = string.Empty;
    public string Assumptions { get; set; } = string.Empty;
    public List<ScenarioVariable> Variables { get; set; } = new();
}

public class ScenarioVariable
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string Impact { get; set; } = string.Empty;
}

public class SensitivityAnalysis
{
    public string Variable { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal BaseValue { get; set; }
    public decimal MinValue { get; set; }
    public decimal MaxValue { get; set; }
    public decimal Step { get; set; }
    public List<SensitivityPoint> Points { get; set; } = new();
    public string Currency { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string Impact { get; set; } = string.Empty;
}

public class SensitivityPoint
{
    public decimal VariableValue { get; set; }
    public decimal NPV { get; set; }
    public decimal IRR { get; set; }
    public decimal ROI { get; set; }
    public decimal PaybackPeriod { get; set; }
}

public class BreakEvenAnalysis
{
    public Guid BusinessPlanId { get; set; }
    public decimal FixedCosts { get; set; }
    public decimal VariableCostsPerUnit { get; set; }
    public decimal SellingPricePerUnit { get; set; }
    public decimal BreakEvenUnits { get; set; }
    public decimal BreakEvenRevenue { get; set; }
    public decimal ContributionMargin { get; set; }
    public decimal ContributionMarginRatio { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Timeframe { get; set; } = string.Empty;
    public List<BreakEvenPoint> Points { get; set; } = new();
    public string Notes { get; set; } = string.Empty;
}

public class BreakEvenPoint
{
    public decimal Units { get; set; }
    public decimal Revenue { get; set; }
    public decimal TotalCosts { get; set; }
    public decimal Profit { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
}

// Financial Service Interface and Implementation
public interface IFinancialService
{
    Task<TestResult<FinancialProjection>> CreateFinancialProjectionAsync(CreateFinancialProjectionCommand command);
    Task<TestResult<FinancialProjection>> GetFinancialProjectionByIdAsync(Guid id);
    Task<TestResult<List<FinancialProjection>>> GetFinancialProjectionsByBusinessPlanAsync(Guid businessPlanId);
    Task<TestResult<List<FinancialProjection>>> GetFinancialProjectionsByScenarioAsync(Guid businessPlanId, ScenarioType scenario);
    Task<TestResult<FinancialProjection>> UpdateFinancialProjectionAsync(UpdateFinancialProjectionCommand command);
    Task<TestResult<bool>> DeleteFinancialProjectionAsync(Guid id);
    Task<TestResult<decimal>> ConvertCurrencyAsync(decimal amount, string fromCurrency, string toCurrency);
    Task<TestResult<TaxCalculation>> CalculateTaxAsync(TaxCalculationRequest request);
    Task<TestResult<List<TaxCalculation>>> CalculateTaxesForProjectionAsync(Guid projectionId);
    Task<TestResult<List<FinancialKPI>>> CalculateKPIsAsync(Guid businessPlanId);
    Task<TestResult<InvestmentAnalysis>> CreateInvestmentAnalysisAsync(CreateInvestmentAnalysisCommand command);
    Task<TestResult<InvestmentAnalysis>> CalculateROIAsync(Guid businessPlanId, decimal investmentAmount);
    Task<TestResult<InvestmentAnalysis>> CalculateNPVAsync(Guid businessPlanId, decimal discountRate);
    Task<TestResult<InvestmentAnalysis>> CalculateIRRAsync(Guid businessPlanId);
    Task<TestResult<CashFlowReport>> GenerateCashFlowReportAsync(Guid businessPlanId);
    Task<TestResult<ProfitLossReport>> GenerateProfitLossReportAsync(Guid businessPlanId);
    Task<TestResult<BalanceSheetReport>> GenerateBalanceSheetReportAsync(Guid businessPlanId);
    Task<TestResult<List<ScenarioAnalysis>>> PerformScenarioAnalysisAsync(Guid businessPlanId);
    Task<TestResult<SensitivityAnalysis>> PerformSensitivityAnalysisAsync(Guid businessPlanId, string variable);
    Task<TestResult<BreakEvenAnalysis>> CalculateBreakEvenAsync(Guid businessPlanId);
}

public class TestFinancialService : IFinancialService
{
    public Task<TestResult<FinancialProjection>> CreateFinancialProjectionAsync(CreateFinancialProjectionCommand command)
    {
        var projection = new FinancialProjection
        {
            Id = Guid.NewGuid(),
            BusinessPlanId = command.BusinessPlanId,
            Name = command.Name,
            Description = command.Description,
            ProjectionType = command.ProjectionType,
            Scenario = command.Scenario.ToString(),
            Year = command.Year,
            Month = command.Month,
            Amount = command.Amount,
            CurrencyCode = command.CurrencyCode,
            BaseAmount = command.Amount,
            Category = command.Category.ToString(),
            SubCategory = command.SubCategory,
            IsRecurring = command.IsRecurring,
            Frequency = command.Frequency,
            GrowthRate = command.GrowthRate,
            Assumptions = command.Assumptions,
            Notes = command.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        return Task.FromResult(TestResult<FinancialProjection>.Success(projection));
    }

    public Task<TestResult<FinancialProjection>> GetFinancialProjectionByIdAsync(Guid id)
    {
        var projection = new FinancialProjection
        {
            Id = id,
            BusinessPlanId = Guid.NewGuid(),
            Name = "Sample Projection",
            Description = "Sample financial projection",
            ProjectionType = "Revenue",
            Scenario = "Realistic",
            Year = 2024,
            Month = 1,
            Amount = 10000m,
            CurrencyCode = "USD",
            BaseAmount = 10000m,
            Category = "Revenue",
            SubCategory = "Product Sales",
            IsRecurring = true,
            Frequency = "Monthly",
            GrowthRate = 10.0m,
            Assumptions = "Standard market conditions",
            Notes = "Sample projection",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        return Task.FromResult(TestResult<FinancialProjection>.Success(projection));
    }

    public Task<TestResult<List<FinancialProjection>>> GetFinancialProjectionsByBusinessPlanAsync(Guid businessPlanId)
    {
        var projections = new List<FinancialProjection>
        {
            new FinancialProjection
            {
                Id = Guid.NewGuid(),
                BusinessPlanId = businessPlanId,
                Name = "Revenue Projection",
                Description = "Monthly revenue projection",
                ProjectionType = "Revenue",
                Scenario = "Realistic",
                Year = 2024,
                Month = 1,
                Amount = 10000m,
                CurrencyCode = "USD",
                BaseAmount = 10000m,
                Category = "Revenue",
                SubCategory = "Product Sales",
                IsRecurring = true,
                Frequency = "Monthly",
                GrowthRate = 10.0m,
                Assumptions = "Standard market conditions",
                Notes = "Monthly revenue projection",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };
        return Task.FromResult(TestResult<List<FinancialProjection>>.Success(projections));
    }

    public Task<TestResult<List<FinancialProjection>>> GetFinancialProjectionsByScenarioAsync(Guid businessPlanId, ScenarioType scenario)
    {
        var projections = new List<FinancialProjection>
        {
            new FinancialProjection
            {
                Id = Guid.NewGuid(),
                BusinessPlanId = businessPlanId,
                Name = $"{scenario} Projection",
                Description = $"{scenario} scenario projection",
                ProjectionType = "Revenue",
                Scenario = scenario.ToString(),
                Year = 2024,
                Month = 1,
                Amount = scenario == ScenarioType.Optimistic ? 15000m : scenario == ScenarioType.Pessimistic ? 5000m : 10000m,
                CurrencyCode = "USD",
                BaseAmount = scenario == ScenarioType.Optimistic ? 15000m : scenario == ScenarioType.Pessimistic ? 5000m : 10000m,
                Category = "Revenue",
                SubCategory = "Product Sales",
                IsRecurring = true,
                Frequency = "Monthly",
                GrowthRate = scenario == ScenarioType.Optimistic ? 20.0m : scenario == ScenarioType.Pessimistic ? 5.0m : 10.0m,
                Assumptions = $"{scenario} market conditions",
                Notes = $"{scenario} scenario projection",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };
        return Task.FromResult(TestResult<List<FinancialProjection>>.Success(projections));
    }

    public Task<TestResult<FinancialProjection>> UpdateFinancialProjectionAsync(UpdateFinancialProjectionCommand command)
    {
        var projection = new FinancialProjection
        {
            Id = command.Id,
            BusinessPlanId = Guid.NewGuid(),
            Name = command.Name,
            Description = command.Description,
            ProjectionType = command.ProjectionType,
            Scenario = command.Scenario.ToString(),
            Year = command.Year,
            Month = command.Month,
            Amount = command.Amount,
            CurrencyCode = command.CurrencyCode,
            BaseAmount = command.Amount,
            Category = command.Category.ToString(),
            SubCategory = command.SubCategory,
            IsRecurring = command.IsRecurring,
            Frequency = command.Frequency,
            GrowthRate = command.GrowthRate,
            Assumptions = command.Assumptions,
            Notes = command.Notes,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow
        };
        return Task.FromResult(TestResult<FinancialProjection>.Success(projection));
    }

    public Task<TestResult<bool>> DeleteFinancialProjectionAsync(Guid id)
    {
        return Task.FromResult(TestResult<bool>.Success(true));
    }

    public Task<TestResult<decimal>> ConvertCurrencyAsync(decimal amount, string fromCurrency, string toCurrency)
    {
        // Mock conversion rate (1 USD = 0.85 EUR)
        var convertedAmount = fromCurrency == "USD" && toCurrency == "EUR" ? amount * 0.85m : amount;
        return Task.FromResult(TestResult<decimal>.Success(convertedAmount));
    }

    public Task<TestResult<TaxCalculation>> CalculateTaxAsync(TaxCalculationRequest request)
    {
        var taxCalculation = new TaxCalculation
        {
            Id = Guid.NewGuid(),
            FinancialProjectionId = request.FinancialProjectionId,
            TaxRuleId = Guid.NewGuid(),
            TaxName = "Income Tax",
            TaxType = request.TaxType,
            TaxableAmount = request.TaxableAmount,
            TaxRate = 25.0m, // 25% tax rate
            TaxAmount = request.TaxableAmount * 0.25m,
            CurrencyCode = request.CurrencyCode,
            CalculationMethod = "Percentage",
            Country = request.Country,
            Region = request.Region,
            TaxPeriod = request.TaxPeriod,
            IsPaid = false,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        return Task.FromResult(TestResult<TaxCalculation>.Success(taxCalculation));
    }

    public Task<TestResult<List<TaxCalculation>>> CalculateTaxesForProjectionAsync(Guid projectionId)
    {
        var taxCalculations = new List<TaxCalculation>
        {
            new TaxCalculation
            {
                Id = Guid.NewGuid(),
                FinancialProjectionId = projectionId,
                TaxRuleId = Guid.NewGuid(),
                TaxName = "Corporate Tax",
                TaxType = "Corporate",
                TaxableAmount = 50000m,
                TaxRate = 25.0m,
                TaxAmount = 12500m,
                CurrencyCode = "USD",
                CalculationMethod = "Percentage",
                Country = "United States",
                Region = "California",
                TaxPeriod = DateTime.UtcNow,
                IsPaid = false,
                Notes = "Corporate tax calculation",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };
        return Task.FromResult(TestResult<List<TaxCalculation>>.Success(taxCalculations));
    }

    public Task<TestResult<List<FinancialKPI>>> CalculateKPIsAsync(Guid businessPlanId)
    {
        var kpis = new List<FinancialKPI>
        {
            new FinancialKPI
            {
                Id = Guid.NewGuid(),
                BusinessPlanId = businessPlanId,
                Name = "Revenue Growth Rate",
                Description = "Year-over-year revenue growth",
                Category = "Growth",
                MetricType = "Percentage",
                Value = 15.5m,
                Unit = "%",
                CurrencyCode = "USD",
                Year = 2024,
                Month = 1,
                TargetValue = 20.0m,
                PreviousValue = 12.0m,
                ChangePercentage = 3.5m,
                Trend = "Up",
                Benchmark = "Industry Average: 10%",
                Status = "Good",
                Notes = "Strong revenue growth",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };
        return Task.FromResult(TestResult<List<FinancialKPI>>.Success(kpis));
    }

    public Task<TestResult<InvestmentAnalysis>> CreateInvestmentAnalysisAsync(CreateInvestmentAnalysisCommand command)
    {
        var analysis = new InvestmentAnalysis
        {
            Id = Guid.NewGuid(),
            BusinessPlanId = command.BusinessPlanId,
            AnalysisType = command.AnalysisType,
            Name = command.Name,
            Description = command.Description,
            InitialInvestment = command.InitialInvestment,
            ExpectedReturn = command.ExpectedReturn,
            NetPresentValue = command.ExpectedReturn - command.InitialInvestment,
            InternalRateOfReturn = 15.0m,
            PaybackPeriod = 24.0m, // 24 months
            ReturnOnInvestment = ((command.ExpectedReturn - command.InitialInvestment) / command.InitialInvestment) * 100,
            CurrencyCode = command.CurrencyCode,
            DiscountRate = command.DiscountRate,
            AnalysisPeriod = command.AnalysisPeriod,
            RiskLevel = command.RiskLevel,
            InvestmentType = command.InvestmentType,
            InvestorType = command.InvestorType,
            Valuation = command.Valuation,
            EquityOffering = command.EquityOffering,
            FundingRequired = command.FundingRequired,
            FundingStage = command.FundingStage,
            Assumptions = command.Assumptions,
            Notes = command.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        return Task.FromResult(TestResult<InvestmentAnalysis>.Success(analysis));
    }

    public Task<TestResult<InvestmentAnalysis>> CalculateROIAsync(Guid businessPlanId, decimal investmentAmount)
    {
        var analysis = new InvestmentAnalysis
        {
            Id = Guid.NewGuid(),
            BusinessPlanId = businessPlanId,
            AnalysisType = "ROI",
            Name = "ROI Analysis",
            Description = "Return on Investment analysis",
            InitialInvestment = investmentAmount,
            ExpectedReturn = investmentAmount * 1.5m,
            NetPresentValue = investmentAmount * 0.5m,
            InternalRateOfReturn = 12.0m,
            PaybackPeriod = 30.0m,
            ReturnOnInvestment = 50.0m,
            CurrencyCode = "USD",
            DiscountRate = 8.0m,
            AnalysisPeriod = 3,
            RiskLevel = "Medium",
            InvestmentType = "Equity",
            InvestorType = "Angel",
            Valuation = investmentAmount * 10m,
            EquityOffering = 10.0m,
            FundingRequired = investmentAmount,
            FundingStage = "Series A",
            Assumptions = "Standard market conditions",
            Notes = "ROI analysis for business plan",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        return Task.FromResult(TestResult<InvestmentAnalysis>.Success(analysis));
    }

    public Task<TestResult<InvestmentAnalysis>> CalculateNPVAsync(Guid businessPlanId, decimal discountRate)
    {
        var analysis = new InvestmentAnalysis
        {
            Id = Guid.NewGuid(),
            BusinessPlanId = businessPlanId,
            AnalysisType = "NPV",
            Name = "NPV Analysis",
            Description = "Net Present Value analysis",
            InitialInvestment = 100000m,
            ExpectedReturn = 150000m,
            NetPresentValue = 25000m,
            InternalRateOfReturn = 15.0m,
            PaybackPeriod = 24.0m,
            ReturnOnInvestment = 50.0m,
            CurrencyCode = "USD",
            DiscountRate = discountRate,
            AnalysisPeriod = 3,
            RiskLevel = "Medium",
            InvestmentType = "Equity",
            InvestorType = "VC",
            Valuation = 1000000m,
            EquityOffering = 10.0m,
            FundingRequired = 100000m,
            FundingStage = "Series A",
            Assumptions = "Standard market conditions",
            Notes = "NPV analysis for business plan",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        return Task.FromResult(TestResult<InvestmentAnalysis>.Success(analysis));
    }

    public Task<TestResult<InvestmentAnalysis>> CalculateIRRAsync(Guid businessPlanId)
    {
        var analysis = new InvestmentAnalysis
        {
            Id = Guid.NewGuid(),
            BusinessPlanId = businessPlanId,
            AnalysisType = "IRR",
            Name = "IRR Analysis",
            Description = "Internal Rate of Return analysis",
            InitialInvestment = 100000m,
            ExpectedReturn = 150000m,
            NetPresentValue = 25000m,
            InternalRateOfReturn = 15.0m,
            PaybackPeriod = 24.0m,
            ReturnOnInvestment = 50.0m,
            CurrencyCode = "USD",
            DiscountRate = 8.0m,
            AnalysisPeriod = 3,
            RiskLevel = "Medium",
            InvestmentType = "Equity",
            InvestorType = "VC",
            Valuation = 1000000m,
            EquityOffering = 10.0m,
            FundingRequired = 100000m,
            FundingStage = "Series A",
            Assumptions = "Standard market conditions",
            Notes = "IRR analysis for business plan",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        return Task.FromResult(TestResult<InvestmentAnalysis>.Success(analysis));
    }

    public Task<TestResult<CashFlowReport>> GenerateCashFlowReportAsync(Guid businessPlanId)
    {
        var report = new CashFlowReport
        {
            BusinessPlanId = businessPlanId,
            Period = "2024 Q1",
            Currency = "USD",
            OpeningBalance = 50000m,
            CashInflows = 75000m,
            CashOutflows = 60000m,
            NetCashFlow = 15000m,
            ClosingBalance = 65000m,
            Items = new List<CashFlowItem>
            {
                new CashFlowItem { Category = "Revenue", Description = "Product Sales", Amount = 50000m, Type = "Inflow", Month = 1, Year = 2024 },
                new CashFlowItem { Category = "Revenue", Description = "Service Revenue", Amount = 25000m, Type = "Inflow", Month = 1, Year = 2024 },
                new CashFlowItem { Category = "Expenses", Description = "Operating Expenses", Amount = 40000m, Type = "Outflow", Month = 1, Year = 2024 },
                new CashFlowItem { Category = "Expenses", Description = "Marketing", Amount = 20000m, Type = "Outflow", Month = 1, Year = 2024 }
            },
            GeneratedAt = DateTime.UtcNow
        };
        return Task.FromResult(TestResult<CashFlowReport>.Success(report));
    }

    public Task<TestResult<ProfitLossReport>> GenerateProfitLossReportAsync(Guid businessPlanId)
    {
        var report = new ProfitLossReport
        {
            BusinessPlanId = businessPlanId,
            Period = "2024 Q1",
            Currency = "USD",
            Revenue = 100000m,
            CostOfGoodsSold = 40000m,
            GrossProfit = 60000m,
            OperatingExpenses = 30000m,
            OperatingIncome = 30000m,
            InterestExpense = 2000m,
            TaxExpense = 7000m,
            NetIncome = 21000m,
            Items = new List<ProfitLossItem>
            {
                new ProfitLossItem { Category = "Revenue", Description = "Product Sales", Amount = 80000m, Type = "Revenue", Month = 1, Year = 2024 },
                new ProfitLossItem { Category = "Revenue", Description = "Service Revenue", Amount = 20000m, Type = "Revenue", Month = 1, Year = 2024 },
                new ProfitLossItem { Category = "COGS", Description = "Product Costs", Amount = 32000m, Type = "Expense", Month = 1, Year = 2024 },
                new ProfitLossItem { Category = "COGS", Description = "Service Costs", Amount = 8000m, Type = "Expense", Month = 1, Year = 2024 }
            },
            GeneratedAt = DateTime.UtcNow
        };
        return Task.FromResult(TestResult<ProfitLossReport>.Success(report));
    }

    public Task<TestResult<BalanceSheetReport>> GenerateBalanceSheetReportAsync(Guid businessPlanId)
    {
        var report = new BalanceSheetReport
        {
            BusinessPlanId = businessPlanId,
            Period = "2024 Q1",
            Currency = "USD",
            TotalAssets = 500000m,
            TotalLiabilities = 200000m,
            TotalEquity = 300000m,
            Assets = new List<BalanceSheetItem>
            {
                new BalanceSheetItem { Category = "Current Assets", Description = "Cash", Amount = 100000m, Type = "Asset", Month = 1, Year = 2024 },
                new BalanceSheetItem { Category = "Current Assets", Description = "Accounts Receivable", Amount = 50000m, Type = "Asset", Month = 1, Year = 2024 },
                new BalanceSheetItem { Category = "Fixed Assets", Description = "Equipment", Amount = 350000m, Type = "Asset", Month = 1, Year = 2024 }
            },
            Liabilities = new List<BalanceSheetItem>
            {
                new BalanceSheetItem { Category = "Current Liabilities", Description = "Accounts Payable", Amount = 30000m, Type = "Liability", Month = 1, Year = 2024 },
                new BalanceSheetItem { Category = "Long-term Liabilities", Description = "Loans", Amount = 170000m, Type = "Liability", Month = 1, Year = 2024 }
            },
            Equity = new List<BalanceSheetItem>
            {
                new BalanceSheetItem { Category = "Equity", Description = "Owner's Equity", Amount = 200000m, Type = "Equity", Month = 1, Year = 2024 },
                new BalanceSheetItem { Category = "Equity", Description = "Retained Earnings", Amount = 100000m, Type = "Equity", Month = 1, Year = 2024 }
            },
            GeneratedAt = DateTime.UtcNow
        };
        return Task.FromResult(TestResult<BalanceSheetReport>.Success(report));
    }

    public Task<TestResult<List<ScenarioAnalysis>>> PerformScenarioAnalysisAsync(Guid businessPlanId)
    {
        var scenarios = new List<ScenarioAnalysis>
        {
            new ScenarioAnalysis
            {
                Scenario = "Optimistic",
                Description = "Best case scenario",
                Revenue = 150000m,
                Expenses = 80000m,
                NetIncome = 70000m,
                CashFlow = 75000m,
                ROI = 70.0m,
                NPV = 50000m,
                IRR = 20.0m,
                Currency = "USD",
                RiskLevel = "Low",
                Assumptions = "Strong market growth, high demand",
                Variables = new List<ScenarioVariable>
                {
                    new ScenarioVariable { Name = "Market Growth", Description = "Annual market growth rate", Value = 15.0m, Unit = "%", Impact = "High" },
                    new ScenarioVariable { Name = "Customer Acquisition", Description = "New customers per month", Value = 100m, Unit = "customers", Impact = "High" }
                }
            },
            new ScenarioAnalysis
            {
                Scenario = "Realistic",
                Description = "Most likely scenario",
                Revenue = 100000m,
                Expenses = 60000m,
                NetIncome = 40000m,
                CashFlow = 45000m,
                ROI = 40.0m,
                NPV = 25000m,
                IRR = 15.0m,
                Currency = "USD",
                RiskLevel = "Medium",
                Assumptions = "Steady market growth, moderate demand",
                Variables = new List<ScenarioVariable>
                {
                    new ScenarioVariable { Name = "Market Growth", Description = "Annual market growth rate", Value = 10.0m, Unit = "%", Impact = "Medium" },
                    new ScenarioVariable { Name = "Customer Acquisition", Description = "New customers per month", Value = 50m, Unit = "customers", Impact = "Medium" }
                }
            },
            new ScenarioAnalysis
            {
                Scenario = "Pessimistic",
                Description = "Worst case scenario",
                Revenue = 60000m,
                Expenses = 50000m,
                NetIncome = 10000m,
                CashFlow = 15000m,
                ROI = 10.0m,
                NPV = 5000m,
                IRR = 8.0m,
                Currency = "USD",
                RiskLevel = "High",
                Assumptions = "Slow market growth, low demand",
                Variables = new List<ScenarioVariable>
                {
                    new ScenarioVariable { Name = "Market Growth", Description = "Annual market growth rate", Value = 5.0m, Unit = "%", Impact = "Low" },
                    new ScenarioVariable { Name = "Customer Acquisition", Description = "New customers per month", Value = 20m, Unit = "customers", Impact = "Low" }
                }
            }
        };
        return Task.FromResult(TestResult<List<ScenarioAnalysis>>.Success(scenarios));
    }

    public Task<TestResult<SensitivityAnalysis>> PerformSensitivityAnalysisAsync(Guid businessPlanId, string variable)
    {
        var analysis = new SensitivityAnalysis
        {
            Variable = variable,
            Description = $"Sensitivity analysis for {variable}",
            BaseValue = 100000m,
            MinValue = 50000m,
            MaxValue = 200000m,
            Step = 10000m,
            Currency = "USD",
            Unit = "USD",
            Impact = "High",
            Points = new List<SensitivityPoint>
            {
                new SensitivityPoint { VariableValue = 50000m, NPV = 10000m, IRR = 8.0m, ROI = 20.0m, PaybackPeriod = 36.0m },
                new SensitivityPoint { VariableValue = 75000m, NPV = 20000m, IRR = 12.0m, ROI = 35.0m, PaybackPeriod = 30.0m },
                new SensitivityPoint { VariableValue = 100000m, NPV = 30000m, IRR = 15.0m, ROI = 50.0m, PaybackPeriod = 24.0m },
                new SensitivityPoint { VariableValue = 125000m, NPV = 40000m, IRR = 18.0m, ROI = 65.0m, PaybackPeriod = 20.0m },
                new SensitivityPoint { VariableValue = 150000m, NPV = 50000m, IRR = 20.0m, ROI = 80.0m, PaybackPeriod = 18.0m }
            }
        };
        return Task.FromResult(TestResult<SensitivityAnalysis>.Success(analysis));
    }

    public Task<TestResult<BreakEvenAnalysis>> CalculateBreakEvenAsync(Guid businessPlanId)
    {
        var analysis = new BreakEvenAnalysis
        {
            BusinessPlanId = businessPlanId,
            FixedCosts = 50000m,
            VariableCostsPerUnit = 25m,
            SellingPricePerUnit = 100m,
            BreakEvenUnits = 667m, // 50000 / (100 - 25)
            BreakEvenRevenue = 66667m, // 667 * 100
            ContributionMargin = 75m, // 100 - 25
            ContributionMarginRatio = 0.75m, // 75 / 100
            Currency = "USD",
            Timeframe = "Monthly",
            Notes = "Break-even analysis for business plan",
            Points = new List<BreakEvenPoint>
            {
                new BreakEvenPoint { Units = 500m, Revenue = 50000m, TotalCosts = 62500m, Profit = -12500m, Month = 1, Year = 2024 },
                new BreakEvenPoint { Units = 667m, Revenue = 66667m, TotalCosts = 66667m, Profit = 0m, Month = 2, Year = 2024 },
                new BreakEvenPoint { Units = 800m, Revenue = 80000m, TotalCosts = 70000m, Profit = 10000m, Month = 3, Year = 2024 }
            }
        };
        return Task.FromResult(TestResult<BreakEvenAnalysis>.Success(analysis));
    }
}

// Financial Request Models
public class CreateFinancialProjectionCommand
{
    public Guid BusinessPlanId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ProjectionType { get; set; } = string.Empty;
    public ScenarioType Scenario { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public FinancialCategory Category { get; set; }
    public string SubCategory { get; set; } = string.Empty;
    public bool IsRecurring { get; set; }
    public string Frequency { get; set; } = string.Empty;
    public decimal GrowthRate { get; set; }
    public string Assumptions { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

public class UpdateFinancialProjectionCommand
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ProjectionType { get; set; } = string.Empty;
    public ScenarioType Scenario { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public FinancialCategory Category { get; set; }
    public string SubCategory { get; set; } = string.Empty;
    public bool IsRecurring { get; set; }
    public string Frequency { get; set; } = string.Empty;
    public decimal GrowthRate { get; set; }
    public string Assumptions { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

public class CreateInvestmentAnalysisCommand
{
    public Guid BusinessPlanId { get; set; }
    public string AnalysisType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal InitialInvestment { get; set; }
    public decimal ExpectedReturn { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal DiscountRate { get; set; }
    public int AnalysisPeriod { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
    public string InvestmentType { get; set; } = string.Empty;
    public string InvestorType { get; set; } = string.Empty;
    public decimal Valuation { get; set; }
    public decimal EquityOffering { get; set; }
    public decimal FundingRequired { get; set; }
    public string FundingStage { get; set; } = string.Empty;
    public string Assumptions { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

public class TaxCalculationRequest
{
    public Guid FinancialProjectionId { get; set; }
    public string Country { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string TaxType { get; set; } = string.Empty;
    public decimal TaxableAmount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public DateTime TaxPeriod { get; set; }
    public string BusinessType { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

// Financial Enums
public enum ScenarioType
{
    Optimistic = 1,
    Realistic = 2,
    Pessimistic = 3,
    BestCase = 4,
    WorstCase = 5,
    BaseCase = 6,
    StressTest = 7,
    SensitivityAnalysis = 8,
    MonteCarlo = 9,
    Other = 10
}

public enum FinancialCategory
{
    Revenue = 1,
    CostOfGoodsSold = 2,
    GrossProfit = 3,
    OperatingExpenses = 4,
    OperatingIncome = 5,
    InterestExpense = 6,
    TaxExpense = 7,
    NetIncome = 8,
    CashFlow = 9,
    Assets = 10,
    Liabilities = 11,
    Equity = 12,
    WorkingCapital = 13,
    Depreciation = 14,
    Amortization = 15,
    Other = 16
}
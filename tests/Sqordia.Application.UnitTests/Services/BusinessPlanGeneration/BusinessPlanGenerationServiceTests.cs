using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Services.Implementations;
using Sqordia.Domain.Entities.BusinessPlan;
using Sqordia.Domain.Enums;
using Sqordia.Persistence.Contexts;
using Xunit;
using OrganizationEntity = Sqordia.Domain.Entities.Organization;

namespace Sqordia.Application.UnitTests.Services.BusinessPlanGeneration;

public class BusinessPlanGenerationServiceTests : IDisposable
{
    private readonly IFixture _fixture;
    private readonly ApplicationDbContext _context;
    private readonly Mock<IAIService> _aiServiceMock;
    private readonly Mock<ILogger<BusinessPlanGenerationService>> _loggerMock;
    private readonly BusinessPlanGenerationService _sut;

    public BusinessPlanGenerationServiceTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        // Setup mocks
        _aiServiceMock = new Mock<IAIService>();
        _loggerMock = new Mock<ILogger<BusinessPlanGenerationService>>();

        // Create service under test
        _sut = new BusinessPlanGenerationService(_context, _aiServiceMock.Object, _loggerMock.Object);
    }

    #region GenerateBusinessPlanAsync Tests

    [Fact]
    public async Task GenerateBusinessPlanAsync_WithValidBusinessPlan_ShouldGenerateAllSections()
    {
        // Arrange
        var organization = CreateOrganization();
        var businessPlan = CreateBusinessPlan(organization, BusinessPlanStatus.QuestionnaireComplete);
        var questionnaireResponses = CreateQuestionnaireResponses(businessPlan, 20);

        await _context.Organizations.AddAsync(organization);
        await _context.BusinessPlans.AddAsync(businessPlan);
        await _context.SaveChangesAsync();

        _aiServiceMock
            .Setup(x => x.GenerateContentWithRetryAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<float>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("Generated content for section");

        // Act
        var result = await _sut.GenerateBusinessPlanAsync(businessPlan.Id, "fr", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue($"Error: {result.Error?.Message}");
        result.Value.Should().NotBeNull();
        result.Value!.Status.Should().Be(BusinessPlanStatus.Generated);

        // Verify AI service was called for all sections
        var expectedSections = _sut.GetAvailableSections(businessPlan.PlanType.ToString()).Count;
        _aiServiceMock.Verify(
            x => x.GenerateContentWithRetryAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<float>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(expectedSections));
    }

    [Fact]
    public async Task GenerateBusinessPlanAsync_WithNonExistentBusinessPlan_ShouldReturnFailure()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _sut.GenerateBusinessPlanAsync(nonExistentId, "fr", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task GenerateBusinessPlanAsync_WithIncompleteQuestionnaire_ShouldReturnFailure()
    {
        // Arrange
        var organization = CreateOrganization();
        var businessPlan = CreateBusinessPlan(organization, BusinessPlanStatus.Draft);

        await _context.Organizations.AddAsync(organization);
        await _context.BusinessPlans.AddAsync(businessPlan);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GenerateBusinessPlanAsync(businessPlan.Id, "fr", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Contain("must be complete");
    }

    [Fact]
    public async Task GenerateBusinessPlanAsync_WithEnglishLanguage_ShouldGenerateInEnglish()
    {
        // Arrange
        var organization = CreateOrganization();
        var businessPlan = CreateBusinessPlan(organization, BusinessPlanStatus.QuestionnaireComplete);
        var questionnaireResponses = CreateQuestionnaireResponses(businessPlan, 20);

        await _context.Organizations.AddAsync(organization);
        await _context.BusinessPlans.AddAsync(businessPlan);
        await _context.SaveChangesAsync();

        _aiServiceMock
            .Setup(x => x.GenerateContentWithRetryAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<float>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("Generated content in English");

        // Act
        var result = await _sut.GenerateBusinessPlanAsync(businessPlan.Id, "en", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        
        // Debug: Output error if it failed
        if (!result.IsSuccess)
        {
            _loggerMock.Object.LogError($"Test failed with error: {result.Error?.Message}");
        }
        
        result.IsSuccess.Should().BeTrue($"Error: {result.Error?.Message}");

        // Verify generation was successful
        _aiServiceMock.Verify(
            x => x.GenerateContentWithRetryAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<float>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task GenerateBusinessPlanAsync_WhenAIServiceFails_ShouldReturnFailure()
    {
        // Arrange
        var organization = CreateOrganization();
        var businessPlan = CreateBusinessPlan(organization, BusinessPlanStatus.QuestionnaireComplete);

        await _context.Organizations.AddAsync(organization);
        await _context.BusinessPlans.AddAsync(businessPlan);
        await _context.SaveChangesAsync();

        _aiServiceMock
            .Setup(x => x.GenerateContentWithRetryAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<float>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("AI service unavailable"));

        // Act
        var result = await _sut.GenerateBusinessPlanAsync(businessPlan.Id, "fr", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Contain("Failed to generate business plan");
    }

    #endregion

    #region RegenerateSectionAsync Tests

    [Fact]
    public async Task RegenerateSectionAsync_WithValidSection_ShouldRegenerateSection()
    {
        // Arrange
        var organization = CreateOrganization();
        var businessPlan = CreateBusinessPlan(organization, BusinessPlanStatus.Generated);
        var questionnaireResponses = CreateQuestionnaireResponses(businessPlan, 20);

        await _context.Organizations.AddAsync(organization);
        await _context.BusinessPlans.AddAsync(businessPlan);
        await _context.SaveChangesAsync();

        _aiServiceMock
            .Setup(x => x.GenerateContentWithRetryAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<float>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("Regenerated content for ExecutiveSummary");

        // Act
        var result = await _sut.RegenerateSectionAsync(businessPlan.Id, "ExecutiveSummary", "fr", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        // Verify AI service was called once
        _aiServiceMock.Verify(
            x => x.GenerateContentWithRetryAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<float>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RegenerateSectionAsync_WithNonExistentBusinessPlan_ShouldReturnFailure()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _sut.RegenerateSectionAsync(nonExistentId, "ExecutiveSummary", "fr", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Contain("not found");
    }

    #endregion

    #region GetGenerationStatusAsync Tests

    [Fact]
    public async Task GetGenerationStatusAsync_WithExistingBusinessPlan_ShouldReturnStatus()
    {
        // Arrange
        var organization = CreateOrganization();
        var businessPlan = CreateBusinessPlan(organization, BusinessPlanStatus.Generated);

        await _context.Organizations.AddAsync(organization);
        await _context.BusinessPlans.AddAsync(businessPlan);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetGenerationStatusAsync(businessPlan.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.BusinessPlanId.Should().Be(businessPlan.Id);
        result.Value.Status.Should().Be(BusinessPlanStatus.Generated.ToString());
        result.Value.TotalSections.Should().BeGreaterThan(0);
        result.Value.CompletionPercentage.Should().BeInRange(0, 100);
    }

    [Fact]
    public async Task GetGenerationStatusAsync_WithNonExistentBusinessPlan_ShouldReturnFailure()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _sut.GetGenerationStatusAsync(nonExistentId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Contain("not found");
    }

    #endregion

    #region GetAvailableSections Tests

    [Fact]
    public void GetAvailableSections_ForBusinessPlan_ShouldReturnStartupSections()
    {
        // Act
        var sections = _sut.GetAvailableSections("0"); // BusinessPlan = 0

        // Assert
        sections.Should().NotBeNull();
        sections.Should().Contain("ExitStrategy");
        sections.Should().NotContain("MissionStatement");
        sections.Should().NotContain("SocialImpact");
    }

    [Fact]
    public void GetAvailableSections_ForStrategicPlan_ShouldReturnOBNLSections()
    {
        // Act
        var sections = _sut.GetAvailableSections("2"); // StrategicPlan = 2

        // Assert
        sections.Should().NotBeNull();
        sections.Should().Contain("MissionStatement");
        sections.Should().Contain("SocialImpact");
        sections.Should().Contain("BeneficiaryProfile");
        sections.Should().Contain("GrantStrategy");
        sections.Should().Contain("SustainabilityPlan");
        sections.Should().NotContain("ExitStrategy");
    }

    [Fact]
    public void GetAvailableSections_ForBothTypes_ShouldIncludeCommonSections()
    {
        // Act
        var businessPlanSections = _sut.GetAvailableSections("0");
        var strategicPlanSections = _sut.GetAvailableSections("2");

        // Assert - Common sections should be in both
        var commonSections = new[]
        {
            "ExecutiveSummary",
            "ProblemStatement",
            "Solution",
            "MarketAnalysis",
            "CompetitiveAnalysis",
            "SwotAnalysis",
            "BusinessModel",
            "MarketingStrategy",
            "BrandingStrategy",
            "OperationsPlan",
            "ManagementTeam",
            "FinancialProjections",
            "FundingRequirements",
            "RiskAnalysis"
        };

        businessPlanSections.Should().Contain(commonSections);
        strategicPlanSections.Should().Contain(commonSections);
    }

    #endregion

    #region Helper Methods

    private OrganizationEntity CreateOrganization()
    {
        return new OrganizationEntity(
            "Test Organization",
            OrganizationType.Startup,
            "Test Description");
    }

    private BusinessPlan CreateBusinessPlan(OrganizationEntity organization, BusinessPlanStatus status)
    {
        var businessPlan = new BusinessPlan(
            "Test Business Plan",
            BusinessPlanType.BusinessPlan,
            organization.Id,
            "Test Description");

        // Use reflection to set status and questionnaire completion tracking for testing purposes
        var statusProperty = typeof(BusinessPlan).GetProperty("Status");
        var questionnaireCompletedAtProperty = typeof(BusinessPlan).GetProperty("QuestionnaireCompletedAt");
        var totalQuestionsProperty = typeof(BusinessPlan).GetProperty("TotalQuestions");
        var completedQuestionsProperty = typeof(BusinessPlan).GetProperty("CompletedQuestions");
        var completionPercentageProperty = typeof(BusinessPlan).GetProperty("CompletionPercentage");

        if (status == BusinessPlanStatus.QuestionnaireComplete)
        {
            // Set questionnaire as complete
            totalQuestionsProperty?.SetValue(businessPlan, 20);
            completedQuestionsProperty?.SetValue(businessPlan, 20);
            completionPercentageProperty?.SetValue(businessPlan, 100m);
            questionnaireCompletedAtProperty?.SetValue(businessPlan, DateTime.UtcNow);
            statusProperty?.SetValue(businessPlan, BusinessPlanStatus.QuestionnaireComplete);
        }
        else if (status == BusinessPlanStatus.Generated)
        {
            // Set as completed questionnaire first
            totalQuestionsProperty?.SetValue(businessPlan, 20);
            completedQuestionsProperty?.SetValue(businessPlan, 20);
            completionPercentageProperty?.SetValue(businessPlan, 100m);
            questionnaireCompletedAtProperty?.SetValue(businessPlan, DateTime.UtcNow);
            statusProperty?.SetValue(businessPlan, BusinessPlanStatus.QuestionnaireComplete);
            
            // Then start and complete generation
            businessPlan.StartGeneration("Test");
            businessPlan.CompleteGeneration();
        }

        return businessPlan;
    }

    private List<QuestionnaireResponse> CreateQuestionnaireResponses(BusinessPlan businessPlan, int count)
    {
        var responses = new List<QuestionnaireResponse>();

        for (int i = 1; i <= count; i++)
        {
            var template = new QuestionTemplate(
                Guid.NewGuid(), // questionnaireTemplateId
                $"Question {i}", // questionText
                QuestionType.ShortText, // questionType
                i, // order
                true, // isRequired
                "Test Section", // section
                "Help text", // helpText
                null); // options

            // Set the template's Id using reflection
            var templateIdProperty = template.GetType().BaseType?.GetProperty("Id");
            var templateId = Guid.NewGuid();
            templateIdProperty?.SetValue(template, templateId);

            var response = new QuestionnaireResponse(
                businessPlan.Id,
                templateId,
                $"Answer {i}");

            // Set the template to the response using reflection
            var templateProperty = typeof(QuestionnaireResponse).GetProperty("QuestionTemplate");
            templateProperty?.SetValue(response, template);

            responses.Add(response);
        }

        _context.QuestionnaireResponses.AddRange(responses);

        return responses;
    }

    #endregion

    public void Dispose()
    {
        _context.Dispose();
    }
}


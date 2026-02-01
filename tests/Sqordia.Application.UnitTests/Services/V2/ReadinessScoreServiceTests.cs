using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Services.V2.Implementations;
using Sqordia.Domain.Entities;
using Sqordia.Domain.Entities.BusinessPlan;
using Sqordia.Domain.Entities.Identity;
using Sqordia.Domain.Enums;
using Xunit;

namespace Sqordia.Application.UnitTests.Services.V2;

public class ReadinessScoreServiceTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ILogger<ReadinessScoreService>> _loggerMock;
    private readonly ReadinessScoreService _sut;

    public ReadinessScoreServiceTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _loggerMock = new Mock<ILogger<ReadinessScoreService>>();

        _sut = new ReadinessScoreService(
            _contextMock.Object,
            _currentUserServiceMock.Object,
            _loggerMock.Object);
    }

    #region Helper Methods

    /// <summary>
    /// Creates a BusinessPlan entity using the public constructor, then populates sections via update methods.
    /// </summary>
    private static BusinessPlan CreateBusinessPlan(
        Guid? id = null,
        Guid? organizationId = null,
        string? executiveSummary = null,
        string? businessModel = null,
        string? marketAnalysis = null,
        string? financialProjections = null,
        string? swotAnalysis = null,
        string? operationsPlan = null,
        bool isDeleted = false)
    {
        var orgId = organizationId ?? Guid.NewGuid();
        var plan = new BusinessPlan("Test Plan", BusinessPlanType.BusinessPlan, orgId);

        // Use reflection to set Id since it has a protected setter
        if (id.HasValue)
        {
            typeof(BusinessPlan).BaseType!.BaseType!
                .GetProperty("Id")!
                .SetValue(plan, id.Value);
        }

        plan.UpdateExecutiveSummary(executiveSummary);
        plan.UpdateBusinessModel(businessModel);
        plan.UpdateMarketAnalysis(marketAnalysis);
        plan.UpdateFinancialProjections(financialProjections);
        plan.UpdateSwotAnalysis(swotAnalysis);
        plan.UpdateOperationsPlan(operationsPlan);

        if (isDeleted)
        {
            plan.SoftDelete();
        }

        return plan;
    }

    /// <summary>
    /// Creates a mock DbSet backed by an in-memory list, supporting async operations and Include().
    /// </summary>
    private static Mock<DbSet<T>> CreateMockDbSet<T>(List<T> data) where T : class
    {
        var queryable = data.AsQueryable();
        var mockDbSet = new Mock<DbSet<T>>();

        mockDbSet.As<IAsyncEnumerable<T>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));

        mockDbSet.As<IQueryable<T>>()
            .Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<T>(queryable.Provider));

        mockDbSet.As<IQueryable<T>>()
            .Setup(m => m.Expression)
            .Returns(queryable.Expression);

        mockDbSet.As<IQueryable<T>>()
            .Setup(m => m.ElementType)
            .Returns(queryable.ElementType);

        mockDbSet.As<IQueryable<T>>()
            .Setup(m => m.GetEnumerator())
            .Returns(queryable.GetEnumerator());

        return mockDbSet;
    }

    private void SetupAuthenticatedUser(Guid userId)
    {
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId.ToString());
    }

    private void SetupDbSets(List<BusinessPlan> businessPlans, List<OrganizationMember> members)
    {
        var bpDbSet = CreateMockDbSet(businessPlans);
        var omDbSet = CreateMockDbSet(members);

        _contextMock.Setup(x => x.BusinessPlans).Returns(bpDbSet.Object);
        _contextMock.Setup(x => x.OrganizationMembers).Returns(omDbSet.Object);
    }

    /// <summary>
    /// Generates a string of specified length with realistic content.
    /// </summary>
    private static string GenerateContent(int length, string? seed = null)
    {
        var baseText = seed ?? "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. ";
        var result = string.Empty;
        while (result.Length < length)
        {
            result += baseText;
        }
        return result[..length];
    }

    /// <summary>
    /// Generates realistic content for a section with numbers and structured paragraphs.
    /// </summary>
    private static string GenerateRichContent(int length)
    {
        var paragraphs = new[]
        {
            "Our market analysis shows a 25% growth rate in the target segment.\n",
            "The competitive landscape reveals 15 major competitors.\n",
            "Revenue projections indicate $500,000 in the first year.\n",
            "The total addressable market is valued at $10 billion.\n",
            "We project a 40% gross margin by year three.\n"
        };
        var result = string.Join("\n", paragraphs);
        while (result.Length < length)
        {
            result += string.Join("\n", paragraphs);
        }
        return result[..length];
    }

    #endregion

    #region Authentication and Access Tests

    [Fact]
    public async Task CalculateReadinessScoreAsync_WhenUserNotAuthenticated_ShouldReturnNotFound()
    {
        // Arrange
        var businessPlanId = Guid.NewGuid();
        _currentUserServiceMock.Setup(x => x.UserId).Returns((string?)null);

        // Act
        var result = await _sut.CalculateReadinessScoreAsync(businessPlanId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Contain("NotFound");
    }

    [Fact]
    public async Task CalculateReadinessScoreAsync_WhenUserIdNotValidGuid_ShouldReturnNotFound()
    {
        // Arrange
        var businessPlanId = Guid.NewGuid();
        _currentUserServiceMock.Setup(x => x.UserId).Returns("invalid-guid");

        // Act
        var result = await _sut.CalculateReadinessScoreAsync(businessPlanId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Contain("NotFound");
    }

    [Fact]
    public async Task CalculateReadinessScoreAsync_WhenUserIdIsEmptyString_ShouldReturnNotFound()
    {
        // Arrange
        var businessPlanId = Guid.NewGuid();
        _currentUserServiceMock.Setup(x => x.UserId).Returns(string.Empty);

        // Act
        var result = await _sut.CalculateReadinessScoreAsync(businessPlanId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Contain("NotFound");
    }

    [Fact]
    public async Task CalculateReadinessScoreAsync_WhenBusinessPlanNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var businessPlanId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);
        SetupDbSets(new List<BusinessPlan>(), new List<OrganizationMember>());

        // Act
        var result = await _sut.CalculateReadinessScoreAsync(businessPlanId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("BusinessPlan.NotFound");
    }

    [Fact]
    public async Task CalculateReadinessScoreAsync_WhenBusinessPlanIsDeleted_ShouldReturnNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var businessPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);

        var plan = CreateBusinessPlan(id: businessPlanId, organizationId: orgId, isDeleted: true);
        var member = new OrganizationMember(orgId, userId, OrganizationRole.Owner);

        SetupDbSets(new List<BusinessPlan> { plan }, new List<OrganizationMember> { member });

        // Act
        var result = await _sut.CalculateReadinessScoreAsync(businessPlanId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("BusinessPlan.NotFound");
    }

    [Fact]
    public async Task CalculateReadinessScoreAsync_WhenUserNotMemberOfOrganization_ShouldReturnNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var businessPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);

        var plan = CreateBusinessPlan(id: businessPlanId, organizationId: orgId);
        // Member belongs to a different user
        var member = new OrganizationMember(orgId, otherUserId, OrganizationRole.Owner);

        SetupDbSets(new List<BusinessPlan> { plan }, new List<OrganizationMember> { member });

        // Act
        var result = await _sut.CalculateReadinessScoreAsync(businessPlanId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("BusinessPlan.NotFound");
    }

    [Fact]
    public async Task CalculateReadinessScoreAsync_WhenUserMembershipIsInactive_ShouldReturnNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var businessPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);

        var plan = CreateBusinessPlan(id: businessPlanId, organizationId: orgId);
        var member = new OrganizationMember(orgId, userId, OrganizationRole.Owner);
        member.Deactivate();

        SetupDbSets(new List<BusinessPlan> { plan }, new List<OrganizationMember> { member });

        // Act
        var result = await _sut.CalculateReadinessScoreAsync(businessPlanId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("BusinessPlan.NotFound");
    }

    #endregion

    #region Complete Business Plan Data Tests

    [Fact]
    public async Task CalculateReadinessScoreAsync_WithCompleteBusinessPlan_ShouldReturnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var businessPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);

        var plan = CreateBusinessPlan(
            id: businessPlanId,
            organizationId: orgId,
            executiveSummary: GenerateRichContent(600),
            businessModel: GenerateRichContent(200),
            marketAnalysis: "Our market analysis shows a 25% growth rate with 15 competitors and a total market of $10 billion.",
            financialProjections: "Revenue projections: $500,000 in year 1 with risk mitigation strategies and reserve funds for runway.",
            swotAnalysis: "Strengths: Strong team. Weaknesses: Limited funding. Opportunities: Growing market. Threats: competition and regulatory challenges.",
            operationsPlan: GenerateRichContent(200));

        var member = new OrganizationMember(orgId, userId, OrganizationRole.Owner);
        SetupDbSets(new List<BusinessPlan> { plan }, new List<OrganizationMember> { member });

        // Act
        var result = await _sut.CalculateReadinessScoreAsync(businessPlanId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.BusinessPlanId.Should().Be(businessPlanId);
        result.Value.OverallScore.Should().BeGreaterThan(0);
        result.Value.OverallScore.Should().BeLessThanOrEqualTo(100);
        result.Value.ConsistencyScore.Should().BeGreaterThan(0);
        result.Value.RiskMitigationScore.Should().BeGreaterThan(0);
        result.Value.CompletenessScore.Should().BeGreaterThan(0);
        result.Value.ReadinessLevel.Should().NotBeNullOrEmpty();
        result.Value.Recommendations.Should().NotBeNull();
        result.Value.CalculatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CalculateReadinessScoreAsync_WithAllSectionsFilledAndRichContent_ShouldReturnHighCompletenessScore()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var businessPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);

        // All sections > 100 chars for full completeness
        var plan = CreateBusinessPlan(
            id: businessPlanId,
            organizationId: orgId,
            executiveSummary: GenerateRichContent(600),
            businessModel: GenerateRichContent(200),
            marketAnalysis: GenerateRichContent(200),
            financialProjections: GenerateRichContent(200),
            swotAnalysis: GenerateRichContent(200),
            operationsPlan: GenerateRichContent(200));

        var member = new OrganizationMember(orgId, userId, OrganizationRole.Owner);
        SetupDbSets(new List<BusinessPlan> { plan }, new List<OrganizationMember> { member });

        // Act
        var result = await _sut.CalculateReadinessScoreAsync(businessPlanId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // All 6 sections filled with > 100 chars => completeness = 100
        result.Value!.CompletenessScore.Should().Be(100m);
    }

    [Fact]
    public async Task CalculateReadinessScoreAsync_WithFullPlan_ShouldReturnCorrectOverallAggregation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var businessPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);

        // Build a plan where we can predict the scores:
        // - All sections filled with > 100 chars => completeness = 100
        // - Consistency: No deductions => 100
        //   (financial + market both have numbers, exec summary >= 500, SWOT has strength + weakness)
        // - Risk mitigation: risk + mitigation + reserve keywords => 100
        var plan = CreateBusinessPlan(
            id: businessPlanId,
            organizationId: orgId,
            executiveSummary: GenerateContent(600, "This is a comprehensive executive summary covering all aspects of the business plan including market and financial details. "),
            businessModel: GenerateRichContent(200),
            marketAnalysis: "Our market analysis indicates a 25% growth with total addressable market size of 5 billion. We face risk from competitors.",
            financialProjections: "Revenue: 500000 with mitigation strategies and financial reserve for runway in year 1.",
            swotAnalysis: "Strengths: strong brand. Weaknesses: limited capital. We identify key risk areas and provide contingency plans.",
            operationsPlan: GenerateRichContent(200));

        var member = new OrganizationMember(orgId, userId, OrganizationRole.Owner);
        SetupDbSets(new List<BusinessPlan> { plan }, new List<OrganizationMember> { member });

        // Act
        var result = await _sut.CalculateReadinessScoreAsync(businessPlanId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var response = result.Value!;

        // Overall = consistency * 0.50 + riskMitigation * 0.30 + completeness * 0.20
        var expectedOverall = (response.ConsistencyScore * 0.50m) +
                              (response.RiskMitigationScore * 0.30m) +
                              (response.CompletenessScore * 0.20m);
        response.OverallScore.Should().Be(Math.Round(expectedOverall, 2));
    }

    #endregion

    #region Partial Data Tests

    [Fact]
    public async Task CalculateReadinessScoreAsync_WithOnlyExecutiveSummary_ShouldReturnLowCompleteness()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var businessPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);

        var plan = CreateBusinessPlan(
            id: businessPlanId,
            organizationId: orgId,
            executiveSummary: GenerateRichContent(200));

        var member = new OrganizationMember(orgId, userId, OrganizationRole.Owner);
        SetupDbSets(new List<BusinessPlan> { plan }, new List<OrganizationMember> { member });

        // Act
        var result = await _sut.CalculateReadinessScoreAsync(businessPlanId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // Only 1 of 6 sections complete => completeness = 1/6 * 100 ~= 16.67
        result.Value!.CompletenessScore.Should().BeApproximately(16.67m, 0.01m);
    }

    [Fact]
    public async Task CalculateReadinessScoreAsync_WithThreeOfSixSectionsFilled_ShouldReturnFiftyPercentCompleteness()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var businessPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);

        var plan = CreateBusinessPlan(
            id: businessPlanId,
            organizationId: orgId,
            executiveSummary: GenerateRichContent(200),
            businessModel: GenerateRichContent(200),
            marketAnalysis: GenerateRichContent(200));
        // financialProjections, swotAnalysis, operationsPlan are null

        var member = new OrganizationMember(orgId, userId, OrganizationRole.Owner);
        SetupDbSets(new List<BusinessPlan> { plan }, new List<OrganizationMember> { member });

        // Act
        var result = await _sut.CalculateReadinessScoreAsync(businessPlanId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // 3 of 6 sections complete => completeness = 50
        result.Value!.CompletenessScore.Should().Be(50m);
    }

    [Fact]
    public async Task CalculateReadinessScoreAsync_WithSectionsTooShort_ShouldNotCountAsComplete()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var businessPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);

        // All sections filled but under 100 characters
        var plan = CreateBusinessPlan(
            id: businessPlanId,
            organizationId: orgId,
            executiveSummary: "Short summary",
            businessModel: "Brief model",
            marketAnalysis: "Small market",
            financialProjections: "No details",
            swotAnalysis: "Minimal SWOT",
            operationsPlan: "Basic ops");

        var member = new OrganizationMember(orgId, userId, OrganizationRole.Owner);
        SetupDbSets(new List<BusinessPlan> { plan }, new List<OrganizationMember> { member });

        // Act
        var result = await _sut.CalculateReadinessScoreAsync(businessPlanId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // All sections < 100 chars => none count as complete => completeness = 0
        result.Value!.CompletenessScore.Should().Be(0m);
    }

    [Fact]
    public async Task CalculateReadinessScoreAsync_WithPartialData_ShouldGenerateRecommendations()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var businessPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);

        // Only market analysis filled; missing exec summary, financials, business model
        var plan = CreateBusinessPlan(
            id: businessPlanId,
            organizationId: orgId,
            marketAnalysis: GenerateRichContent(200));

        var member = new OrganizationMember(orgId, userId, OrganizationRole.Owner);
        SetupDbSets(new List<BusinessPlan> { plan }, new List<OrganizationMember> { member });

        // Act
        var result = await _sut.CalculateReadinessScoreAsync(businessPlanId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Recommendations.Should().NotBeEmpty();
        result.Value.Recommendations.Should().Contain(r => r.Contains("executive summary"));
        result.Value.Recommendations.Should().Contain(r => r.Contains("financial projections"));
        result.Value.Recommendations.Should().Contain(r => r.Contains("business model"));
    }

    #endregion

    #region Financial Readiness Tests

    [Fact]
    public async Task CalculateReadinessScoreAsync_WithFinancialProjectionsContainingNumbers_ShouldContributeToConsistency()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var businessPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);

        // Financial projections with numbers but NO market data with numbers => consistency deduction of 20
        var plan = CreateBusinessPlan(
            id: businessPlanId,
            organizationId: orgId,
            financialProjections: "Revenue: $500,000 in year 1, growing to $1,000,000 in year 2.",
            marketAnalysis: "The market is large and growing with many opportunities in the technology sector overall.");

        var member = new OrganizationMember(orgId, userId, OrganizationRole.Owner);
        SetupDbSets(new List<BusinessPlan> { plan }, new List<OrganizationMember> { member });

        // Act
        var result = await _sut.CalculateReadinessScoreAsync(businessPlanId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // Financial has numbers (\d+), market does NOT have numbers => deduction of 20
        // Consistency should be 100 - 20 = 80
        result.Value!.ConsistencyScore.Should().Be(80m);
    }

    [Fact]
    public async Task CalculateReadinessScoreAsync_WithFinancialAndMarketDataAligned_ShouldNotDeductConsistency()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var businessPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);

        // Both financial and market have numbers => no deduction for misalignment
        var plan = CreateBusinessPlan(
            id: businessPlanId,
            organizationId: orgId,
            financialProjections: "Revenue: 500000 in year 1.",
            marketAnalysis: "Market size is 10 billion with 25% growth.");

        var member = new OrganizationMember(orgId, userId, OrganizationRole.Owner);
        SetupDbSets(new List<BusinessPlan> { plan }, new List<OrganizationMember> { member });

        // Act
        var result = await _sut.CalculateReadinessScoreAsync(businessPlanId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // No financial/market misalignment deduction
        result.Value!.ConsistencyScore.Should().Be(100m);
    }

    [Fact]
    public async Task CalculateReadinessScoreAsync_WithNoFinancials_ShouldReturnZeroForFinancialSafeguards()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var businessPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);

        // No financial projections, no financial keywords anywhere
        var plan = CreateBusinessPlan(
            id: businessPlanId,
            organizationId: orgId,
            executiveSummary: GenerateContent(200));

        var member = new OrganizationMember(orgId, userId, OrganizationRole.Owner);
        SetupDbSets(new List<BusinessPlan> { plan }, new List<OrganizationMember> { member });

        // Act
        var result = await _sut.CalculateReadinessScoreAsync(businessPlanId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // No risk, mitigation, or financial keywords => risk mitigation score = 0
        result.Value!.RiskMitigationScore.Should().Be(0m);
    }

    [Fact]
    public async Task CalculateReadinessScoreAsync_WithFinancialSafeguardKeywords_ShouldScoreFinancialSafeguards()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var businessPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);

        // Financial projections containing "reserve" and "runway" keywords
        var plan = CreateBusinessPlan(
            id: businessPlanId,
            organizationId: orgId,
            financialProjections: "We maintain a financial reserve of 6 months runway with insurance coverage to ensure stability.");

        var member = new OrganizationMember(orgId, userId, OrganizationRole.Owner);
        SetupDbSets(new List<BusinessPlan> { plan }, new List<OrganizationMember> { member });

        // Act
        var result = await _sut.CalculateReadinessScoreAsync(businessPlanId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // "reserve" and "runway" and "insurance" are financial safeguard keywords => +30
        result.Value!.RiskMitigationScore.Should().BeGreaterThanOrEqualTo(30m);
    }

    [Fact]
    public async Task CalculateReadinessScoreAsync_WithNoFinancials_ShouldRecommendFinancialProjections()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var businessPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);

        var plan = CreateBusinessPlan(
            id: businessPlanId,
            organizationId: orgId,
            executiveSummary: GenerateRichContent(200),
            businessModel: GenerateRichContent(200));

        var member = new OrganizationMember(orgId, userId, OrganizationRole.Owner);
        SetupDbSets(new List<BusinessPlan> { plan }, new List<OrganizationMember> { member });

        // Act
        var result = await _sut.CalculateReadinessScoreAsync(businessPlanId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Recommendations.Should().Contain(r => r.Contains("financial projections"));
    }

    #endregion

    #region Strategic Readiness (Consistency Score) Tests

    [Fact]
    public async Task CalculateReadinessScoreAsync_WithShortExecutiveSummaryAndMultipleSections_ShouldDeductConsistency()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var businessPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);

        // Executive summary < 500 chars, but market analysis and financial projections both present
        var plan = CreateBusinessPlan(
            id: businessPlanId,
            organizationId: orgId,
            executiveSummary: "Short summary that does not fully cover the plan.",
            marketAnalysis: "Market data shows 25% growth.",
            financialProjections: "Revenue: 500000 in year 1.");

        var member = new OrganizationMember(orgId, userId, OrganizationRole.Owner);
        SetupDbSets(new List<BusinessPlan> { plan }, new List<OrganizationMember> { member });

        // Act
        var result = await _sut.CalculateReadinessScoreAsync(businessPlanId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // Deduction of 15 for short exec summary with multiple sections
        result.Value!.ConsistencyScore.Should().Be(85m);
    }

    [Fact]
    public async Task CalculateReadinessScoreAsync_WithLongExecutiveSummaryAndMultipleSections_ShouldNotDeductForSummaryLength()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var businessPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);

        // Exec summary >= 500 chars
        var plan = CreateBusinessPlan(
            id: businessPlanId,
            organizationId: orgId,
            executiveSummary: GenerateContent(600),
            marketAnalysis: "Market data shows 25% growth rate.",
            financialProjections: "Revenue: 500000 in year 1.");

        var member = new OrganizationMember(orgId, userId, OrganizationRole.Owner);
        SetupDbSets(new List<BusinessPlan> { plan }, new List<OrganizationMember> { member });

        // Act
        var result = await _sut.CalculateReadinessScoreAsync(businessPlanId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // No short-summary deduction (>= 500 chars)
        result.Value!.ConsistencyScore.Should().Be(100m);
    }

    [Fact]
    public async Task CalculateReadinessScoreAsync_WithSwotMissingStrengths_ShouldDeductConsistency()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var businessPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);

        // SWOT has weakness but no strength/force keyword
        var plan = CreateBusinessPlan(
            id: businessPlanId,
            organizationId: orgId,
            swotAnalysis: "Weakness: limited funding. Opportunities: growing market. Threats: competition.");

        var member = new OrganizationMember(orgId, userId, OrganizationRole.Owner);
        SetupDbSets(new List<BusinessPlan> { plan }, new List<OrganizationMember> { member });

        // Act
        var result = await _sut.CalculateReadinessScoreAsync(businessPlanId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // Missing "strength"/"force" => 15 point deduction
        result.Value!.ConsistencyScore.Should().Be(85m);
    }

    [Fact]
    public async Task CalculateReadinessScoreAsync_WithSwotMissingWeaknesses_ShouldDeductConsistency()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var businessPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);

        // SWOT has strength but no weakness/faiblesse keyword
        var plan = CreateBusinessPlan(
            id: businessPlanId,
            organizationId: orgId,
            swotAnalysis: "Strength: strong team. Opportunities: growing market.");

        var member = new OrganizationMember(orgId, userId, OrganizationRole.Owner);
        SetupDbSets(new List<BusinessPlan> { plan }, new List<OrganizationMember> { member });

        // Act
        var result = await _sut.CalculateReadinessScoreAsync(businessPlanId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // Missing "weakness"/"faiblesse" => 15 point deduction
        result.Value!.ConsistencyScore.Should().Be(85m);
    }

    [Fact]
    public async Task CalculateReadinessScoreAsync_WithSwotContainingFrenchTerms_ShouldNotDeduct()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var businessPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);

        // Using French terms "force" and "faiblesse"
        var plan = CreateBusinessPlan(
            id: businessPlanId,
            organizationId: orgId,
            swotAnalysis: "Force: equipe solide. Faiblesse: capital limite.");

        var member = new OrganizationMember(orgId, userId, OrganizationRole.Owner);
        SetupDbSets(new List<BusinessPlan> { plan }, new List<OrganizationMember> { member });

        // Act
        var result = await _sut.CalculateReadinessScoreAsync(businessPlanId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // French terms are accepted, no deduction for SWOT
        result.Value!.ConsistencyScore.Should().Be(100m);
    }

    [Fact]
    public async Task CalculateReadinessScoreAsync_WithMultipleConsistencyIssues_ShouldCumulateDeductions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var businessPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);

        // Short exec summary (< 500 but > 0, with multiple sections) => -15
        // Financial has numbers, market does NOT => -20
        // SWOT missing both strength and weakness => -15
        var plan = CreateBusinessPlan(
            id: businessPlanId,
            organizationId: orgId,
            executiveSummary: "Brief overview.",
            marketAnalysis: "The market is very promising and large overall.",
            financialProjections: "Revenue: 500000 in year 1.",
            swotAnalysis: "We face competition and regulatory challenges ahead.");

        var member = new OrganizationMember(orgId, userId, OrganizationRole.Owner);
        SetupDbSets(new List<BusinessPlan> { plan }, new List<OrganizationMember> { member });

        // Act
        var result = await _sut.CalculateReadinessScoreAsync(businessPlanId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // 100 - 20 (financial/market mismatch) - 15 (short summary) - 15 (SWOT incomplete) = 50
        result.Value!.ConsistencyScore.Should().Be(50m);
    }

    #endregion

    #region Risk Mitigation Score Tests

    [Fact]
    public async Task CalculateReadinessScoreAsync_WithRiskKeywords_ShouldScore30ForRiskIdentification()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var businessPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);

        // Only risk keyword, no mitigation or financial safeguard keywords
        var plan = CreateBusinessPlan(
            id: businessPlanId,
            organizationId: orgId,
            swotAnalysis: "The main risk is market volatility.");

        var member = new OrganizationMember(orgId, userId, OrganizationRole.Owner);
        SetupDbSets(new List<BusinessPlan> { plan }, new List<OrganizationMember> { member });

        // Act
        var result = await _sut.CalculateReadinessScoreAsync(businessPlanId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.RiskMitigationScore.Should().Be(30m);
    }

    [Fact]
    public async Task CalculateReadinessScoreAsync_WithMitigationKeywords_ShouldScore40ForMitigationStrategies()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var businessPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);

        // Only mitigation keyword, no risk or financial safeguard keywords
        var plan = CreateBusinessPlan(
            id: businessPlanId,
            organizationId: orgId,
            operationsPlan: "We have a contingency plan for supply chain disruptions.");

        var member = new OrganizationMember(orgId, userId, OrganizationRole.Owner);
        SetupDbSets(new List<BusinessPlan> { plan }, new List<OrganizationMember> { member });

        // Act
        var result = await _sut.CalculateReadinessScoreAsync(businessPlanId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.RiskMitigationScore.Should().Be(40m);
    }

    [Fact]
    public async Task CalculateReadinessScoreAsync_WithAllRiskMitigationKeywords_ShouldScore100()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var businessPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);

        // All three categories: risk (+30), mitigation (+40), financial safeguards (+30)
        var plan = CreateBusinessPlan(
            id: businessPlanId,
            organizationId: orgId,
            swotAnalysis: "The main risk is competition.",
            financialProjections: "We have a contingency plan and maintain a reserve fund.",
            operationsPlan: "Insurance coverage protects against operational risks.");

        var member = new OrganizationMember(orgId, userId, OrganizationRole.Owner);
        SetupDbSets(new List<BusinessPlan> { plan }, new List<OrganizationMember> { member });

        // Act
        var result = await _sut.CalculateReadinessScoreAsync(businessPlanId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // risk (30) + mitigation (40) + financial safeguards (30) = 100
        result.Value!.RiskMitigationScore.Should().Be(100m);
    }

    [Fact]
    public async Task CalculateReadinessScoreAsync_WithNoRiskRelatedContent_ShouldScoreZeroRiskMitigation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var businessPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);

        // No risk, mitigation, or financial safeguard keywords at all
        var plan = CreateBusinessPlan(
            id: businessPlanId,
            organizationId: orgId,
            swotAnalysis: "We have a strong team and a great product.",
            marketAnalysis: "The market is growing.");

        var member = new OrganizationMember(orgId, userId, OrganizationRole.Owner);
        SetupDbSets(new List<BusinessPlan> { plan }, new List<OrganizationMember> { member });

        // Act
        var result = await _sut.CalculateReadinessScoreAsync(businessPlanId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.RiskMitigationScore.Should().Be(0m);
    }

    [Fact]
    public async Task CalculateReadinessScoreAsync_WithFrenchRiskKeywords_ShouldDetectRisks()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var businessPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);

        // French: "risque" (risk), "attenuation" (mitigation), "reserve" (financial safeguard)
        var plan = CreateBusinessPlan(
            id: businessPlanId,
            organizationId: orgId,
            swotAnalysis: "Le risque principal est la concurrence.",
            financialProjections: "Plan d'attenuation et reserve financiere.",
            operationsPlan: "Nous avons une assurance complete.");

        var member = new OrganizationMember(orgId, userId, OrganizationRole.Owner);
        SetupDbSets(new List<BusinessPlan> { plan }, new List<OrganizationMember> { member });

        // Act
        var result = await _sut.CalculateReadinessScoreAsync(businessPlanId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // "risque" -> risk (30), "attenuation" partial match on "attenuation" but actual keyword is "atténuation"
        // "reserve" -> financial (30), "assurance" -> financial (30, but already counted)
        result.Value!.RiskMitigationScore.Should().BeGreaterThanOrEqualTo(30m);
    }

    [Fact]
    public async Task CalculateReadinessScoreAsync_WithLowRiskMitigation_ShouldRecommendRiskStrategies()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var businessPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);

        // No risk keywords at all => riskMitigation = 0 < 60
        var plan = CreateBusinessPlan(
            id: businessPlanId,
            organizationId: orgId,
            executiveSummary: GenerateContent(200));

        var member = new OrganizationMember(orgId, userId, OrganizationRole.Owner);
        SetupDbSets(new List<BusinessPlan> { plan }, new List<OrganizationMember> { member });

        // Act
        var result = await _sut.CalculateReadinessScoreAsync(businessPlanId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Recommendations.Should().Contain(r => r.Contains("risk"));
        result.Value.Recommendations.Should().Contain(r => r.Contains("contingency"));
    }

    #endregion

    #region Overall Readiness Aggregation Tests

    [Fact]
    public async Task CalculateReadinessScoreAsync_WithHighScore_ShouldReturnBankReadyLevel()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var businessPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);

        // Build a plan that will score >= 85 overall
        // Consistency: 100 (no deductions)
        // Risk: 100 (all keywords)
        // Completeness: 100 (all sections > 100 chars)
        // Overall = 100*0.5 + 100*0.3 + 100*0.2 = 100
        var plan = CreateBusinessPlan(
            id: businessPlanId,
            organizationId: orgId,
            executiveSummary: GenerateContent(600, "This is a comprehensive executive summary. "),
            businessModel: GenerateRichContent(200),
            marketAnalysis: "Our market shows 25% growth. The total addressable market is $10 billion with 50 competitors.",
            financialProjections: "Revenue: 500000. We maintain a financial reserve and contingency plan for risk mitigation with insurance and runway.",
            swotAnalysis: "Strength: strong team and brand. Weakness: limited capital. Risk is managed through alternative strategies.",
            operationsPlan: GenerateRichContent(200));

        var member = new OrganizationMember(orgId, userId, OrganizationRole.Owner);
        SetupDbSets(new List<BusinessPlan> { plan }, new List<OrganizationMember> { member });

        // Act
        var result = await _sut.CalculateReadinessScoreAsync(businessPlanId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.ReadinessLevel.Should().Be("BankReady");
        result.Value.OverallScore.Should().BeGreaterThanOrEqualTo(85m);
    }

    [Fact]
    public async Task CalculateReadinessScoreAsync_WithMediumScore_ShouldReturnReadyLevel()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var businessPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);

        // Target: 70 <= overall < 85
        // Consistency: 100 (no deductions - exec summary > 500 chars, SWOT has strengths+weaknesses)
        // Risk: 30 (only risk keyword, no mitigation/safeguard keywords)
        // Completeness: 100 (all 6 sections > 100 chars)
        // Overall = 100*0.5 + 30*0.3 + 100*0.2 = 50 + 9 + 20 = 79
        var plan = CreateBusinessPlan(
            id: businessPlanId,
            organizationId: orgId,
            executiveSummary: GenerateContent(600, "This is a comprehensive summary that covers all plan details. "),
            businessModel: GenerateRichContent(200),
            marketAnalysis: "Our market shows 25% growth with many opportunities and data points. The total addressable market is worth over $500 million annually with strong demand.",
            financialProjections: "Revenue: 500000 in year 1 with detailed projections. We expect to reach profitability by year 2 with a gross margin of 65% across all product lines.",
            swotAnalysis: "Strength: strong team with diverse skills. Weakness: limited capital for expansion. The main risk is market volatility and competitive pressure from incumbents.",
            operationsPlan: GenerateRichContent(200));

        var member = new OrganizationMember(orgId, userId, OrganizationRole.Owner);
        SetupDbSets(new List<BusinessPlan> { plan }, new List<OrganizationMember> { member });

        // Act
        var result = await _sut.CalculateReadinessScoreAsync(businessPlanId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.ReadinessLevel.Should().Be("Ready");
        result.Value.OverallScore.Should().BeGreaterThanOrEqualTo(70m);
        result.Value.OverallScore.Should().BeLessThan(85m);
    }

    [Fact]
    public async Task CalculateReadinessScoreAsync_WithLowScore_ShouldReturnDevelopingLevel()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var businessPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);

        // Target: 50 <= overall < 70
        // Consistency: 100 (no deductions, no SWOT/financial/market to trigger deductions)
        // Risk: 0 (no risk keywords)
        // Completeness: 50 (3 of 6 sections with > 100 chars)
        // Overall = 100*0.5 + 0*0.3 + 50*0.2 = 50 + 0 + 10 = 60
        var plan = CreateBusinessPlan(
            id: businessPlanId,
            organizationId: orgId,
            executiveSummary: GenerateContent(200, "A straightforward executive summary of the business opportunity. "),
            businessModel: GenerateContent(200, "Our business model relies on subscription fees. "),
            operationsPlan: GenerateContent(200, "We will operate from a central office with a lean team. "));

        var member = new OrganizationMember(orgId, userId, OrganizationRole.Owner);
        SetupDbSets(new List<BusinessPlan> { plan }, new List<OrganizationMember> { member });

        // Act
        var result = await _sut.CalculateReadinessScoreAsync(businessPlanId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.ReadinessLevel.Should().Be("Developing");
        result.Value.OverallScore.Should().BeGreaterThanOrEqualTo(50m);
        result.Value.OverallScore.Should().BeLessThan(70m);
    }

    [Fact]
    public async Task CalculateReadinessScoreAsync_WithVeryLowScore_ShouldReturnNotReadyLevel()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var businessPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);

        // Target: overall < 50
        // Consistency: 50 (multiple deductions)
        // Risk: 0 (no keywords)
        // Completeness: 0 (no sections > 100 chars)
        // Overall = 50*0.5 + 0*0.3 + 0*0.2 = 25
        var plan = CreateBusinessPlan(
            id: businessPlanId,
            organizationId: orgId,
            executiveSummary: "Brief.",
            marketAnalysis: "Growing market overall.",
            financialProjections: "Revenue 500000.",
            swotAnalysis: "We have opportunities.");

        var member = new OrganizationMember(orgId, userId, OrganizationRole.Owner);
        SetupDbSets(new List<BusinessPlan> { plan }, new List<OrganizationMember> { member });

        // Act
        var result = await _sut.CalculateReadinessScoreAsync(businessPlanId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.ReadinessLevel.Should().Be("NotReady");
        result.Value.OverallScore.Should().BeLessThan(50m);
    }

    [Fact]
    public async Task CalculateReadinessScoreAsync_OverallScoreFormula_ShouldApplyCorrectWeights()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var businessPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);

        var plan = CreateBusinessPlan(
            id: businessPlanId,
            organizationId: orgId,
            executiveSummary: GenerateContent(600, "Comprehensive executive summary for the business. "),
            marketAnalysis: "Market data 25% growth rate with projected 100000 customers.",
            financialProjections: "Revenue: 500000. We have contingency strategies and insurance reserve.",
            swotAnalysis: "Strength: brand. Weakness: funding. Risk is a key concern.",
            operationsPlan: GenerateRichContent(200));

        var member = new OrganizationMember(orgId, userId, OrganizationRole.Owner);
        SetupDbSets(new List<BusinessPlan> { plan }, new List<OrganizationMember> { member });

        // Act
        var result = await _sut.CalculateReadinessScoreAsync(businessPlanId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var response = result.Value!;

        // Verify the formula: overall = consistency * 0.50 + riskMitigation * 0.30 + completeness * 0.20
        var expectedOverall = Math.Round(
            (response.ConsistencyScore * 0.50m) +
            (response.RiskMitigationScore * 0.30m) +
            (response.CompletenessScore * 0.20m), 2);
        response.OverallScore.Should().Be(expectedOverall);
    }

    [Fact]
    public async Task CalculateReadinessScoreAsync_RecommendationsMaximum_ShouldNotExceedFive()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var businessPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);

        // Plan with many issues to trigger maximum recommendations
        var plan = CreateBusinessPlan(
            id: businessPlanId,
            organizationId: orgId);
        // All sections null => many recommendations triggered

        var member = new OrganizationMember(orgId, userId, OrganizationRole.Owner);
        SetupDbSets(new List<BusinessPlan> { plan }, new List<OrganizationMember> { member });

        // Act
        var result = await _sut.CalculateReadinessScoreAsync(businessPlanId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // GenerateRecommendations takes at most 5
        result.Value!.Recommendations.Count.Should().BeLessThanOrEqualTo(5);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task CalculateReadinessScoreAsync_WithEmptyPlan_ShouldReturnSuccessWithZeroScores()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var businessPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);

        // Completely empty plan (all sections null)
        var plan = CreateBusinessPlan(id: businessPlanId, organizationId: orgId);

        var member = new OrganizationMember(orgId, userId, OrganizationRole.Owner);
        SetupDbSets(new List<BusinessPlan> { plan }, new List<OrganizationMember> { member });

        // Act
        var result = await _sut.CalculateReadinessScoreAsync(businessPlanId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.CompletenessScore.Should().Be(0m);
        result.Value.RiskMitigationScore.Should().Be(0m);
        // Consistency starts at 100, no sections to trigger deductions
        result.Value.ConsistencyScore.Should().Be(100m);
        // Overall = 100*0.5 + 0*0.3 + 0*0.2 = 50
        result.Value.OverallScore.Should().Be(50m);
        result.Value.ReadinessLevel.Should().Be("Developing");
    }

    [Fact]
    public async Task CalculateReadinessScoreAsync_WithAllSectionsEmpty_ShouldReturnNotReadyOrDeveloping()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var businessPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);

        // All sections are empty strings (not null)
        var plan = CreateBusinessPlan(
            id: businessPlanId,
            organizationId: orgId,
            executiveSummary: "",
            businessModel: "",
            marketAnalysis: "",
            financialProjections: "",
            swotAnalysis: "",
            operationsPlan: "");

        var member = new OrganizationMember(orgId, userId, OrganizationRole.Owner);
        SetupDbSets(new List<BusinessPlan> { plan }, new List<OrganizationMember> { member });

        // Act
        var result = await _sut.CalculateReadinessScoreAsync(businessPlanId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.CompletenessScore.Should().Be(0m);
        result.Value.RiskMitigationScore.Should().Be(0m);
    }

    [Fact]
    public async Task CalculateReadinessScoreAsync_WithWhitespaceOnlySections_ShouldTreatAsEmpty()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var businessPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);

        var plan = CreateBusinessPlan(
            id: businessPlanId,
            organizationId: orgId,
            executiveSummary: "   \n\t  ",
            businessModel: "   ",
            marketAnalysis: "\n\n\n");

        var member = new OrganizationMember(orgId, userId, OrganizationRole.Owner);
        SetupDbSets(new List<BusinessPlan> { plan }, new List<OrganizationMember> { member });

        // Act
        var result = await _sut.CalculateReadinessScoreAsync(businessPlanId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // Whitespace-only sections should be treated as empty by IsNullOrWhiteSpace
        result.Value!.CompletenessScore.Should().Be(0m);
    }

    [Fact]
    public async Task CalculateReadinessScoreAsync_ConsistencyNeverGoesBelowZero()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var businessPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);

        // Trigger all consistency deductions:
        // -20 (financial numbers, market no numbers)
        // -15 (short exec summary with multiple sections)
        // -15 (SWOT missing strength or weakness)
        // Total = -50, so consistency = max(0, 100 - 50) = 50
        var plan = CreateBusinessPlan(
            id: businessPlanId,
            organizationId: orgId,
            executiveSummary: "Brief",
            marketAnalysis: "The market is big overall.",
            financialProjections: "Revenue 500000 in year 1.",
            swotAnalysis: "Lots of opportunities ahead.");

        var member = new OrganizationMember(orgId, userId, OrganizationRole.Owner);
        SetupDbSets(new List<BusinessPlan> { plan }, new List<OrganizationMember> { member });

        // Act
        var result = await _sut.CalculateReadinessScoreAsync(businessPlanId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.ConsistencyScore.Should().BeGreaterThanOrEqualTo(0m);
    }

    [Fact]
    public async Task CalculateReadinessScoreAsync_RiskMitigationNeverExceeds100()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var businessPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);

        // Even with many keywords, score is capped at 100
        var plan = CreateBusinessPlan(
            id: businessPlanId,
            organizationId: orgId,
            swotAnalysis: "Risk, risque, threat, menace, challenge, obstacle, defi",
            financialProjections: "Mitigation, contingency, plan B, backup, alternative, reduce",
            operationsPlan: "Reserve, buffer, cushion, insurance, assurance, runway");

        var member = new OrganizationMember(orgId, userId, OrganizationRole.Owner);
        SetupDbSets(new List<BusinessPlan> { plan }, new List<OrganizationMember> { member });

        // Act
        var result = await _sut.CalculateReadinessScoreAsync(businessPlanId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.RiskMitigationScore.Should().BeLessThanOrEqualTo(100m);
    }

    [Fact]
    public async Task CalculateReadinessScoreAsync_OverallScoreAlwaysBetween0And100()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var businessPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);

        var plan = CreateBusinessPlan(id: businessPlanId, organizationId: orgId);

        var member = new OrganizationMember(orgId, userId, OrganizationRole.Owner);
        SetupDbSets(new List<BusinessPlan> { plan }, new List<OrganizationMember> { member });

        // Act
        var result = await _sut.CalculateReadinessScoreAsync(businessPlanId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.OverallScore.Should().BeGreaterThanOrEqualTo(0m);
        result.Value.OverallScore.Should().BeLessThanOrEqualTo(100m);
    }

    [Fact]
    public async Task CalculateReadinessScoreAsync_ScoresAreRoundedToTwoDecimalPlaces()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var businessPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);

        var plan = CreateBusinessPlan(
            id: businessPlanId,
            organizationId: orgId,
            executiveSummary: GenerateRichContent(200));

        var member = new OrganizationMember(orgId, userId, OrganizationRole.Owner);
        SetupDbSets(new List<BusinessPlan> { plan }, new List<OrganizationMember> { member });

        // Act
        var result = await _sut.CalculateReadinessScoreAsync(businessPlanId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // Verify rounding to 2 decimal places
        var overallStr = result.Value!.OverallScore.ToString();
        if (overallStr.Contains('.'))
        {
            var decimalPart = overallStr.Split('.')[1];
            decimalPart.Length.Should().BeLessThanOrEqualTo(2);
        }
    }

    #endregion

    #region GetReadinessBreakdownAsync Tests

    [Fact]
    public async Task GetReadinessBreakdownAsync_WhenUserNotAuthenticated_ShouldReturnNotFound()
    {
        // Arrange
        var businessPlanId = Guid.NewGuid();
        _currentUserServiceMock.Setup(x => x.UserId).Returns((string?)null);

        // Act
        var result = await _sut.GetReadinessBreakdownAsync(businessPlanId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Contain("NotFound");
    }

    [Fact]
    public async Task GetReadinessBreakdownAsync_WithEmptyPlan_ShouldReturnAllSectionsAsMissing()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var businessPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);

        var plan = CreateBusinessPlan(id: businessPlanId, organizationId: orgId);
        var member = new OrganizationMember(orgId, userId, OrganizationRole.Owner);
        SetupDbSets(new List<BusinessPlan> { plan }, new List<OrganizationMember> { member });

        // Act
        var result = await _sut.GetReadinessBreakdownAsync(businessPlanId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.MissingElements.Should().HaveCount(6);
        result.Value.MissingElements.Should().Contain("Executive Summary");
        result.Value.MissingElements.Should().Contain("Business Model");
        result.Value.MissingElements.Should().Contain("Market Analysis");
        result.Value.MissingElements.Should().Contain("Financial Projections");
        result.Value.MissingElements.Should().Contain("SWOT Analysis");
        result.Value.MissingElements.Should().Contain("Operations Plan");
    }

    [Fact]
    public async Task GetReadinessBreakdownAsync_WithCompletePlan_ShouldReturnNoMissingElements()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var businessPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);

        var plan = CreateBusinessPlan(
            id: businessPlanId,
            organizationId: orgId,
            executiveSummary: GenerateRichContent(200),
            businessModel: GenerateRichContent(200),
            marketAnalysis: GenerateRichContent(200),
            financialProjections: GenerateRichContent(200),
            swotAnalysis: GenerateRichContent(200),
            operationsPlan: GenerateRichContent(200));

        var member = new OrganizationMember(orgId, userId, OrganizationRole.Owner);
        SetupDbSets(new List<BusinessPlan> { plan }, new List<OrganizationMember> { member });

        // Act
        var result = await _sut.GetReadinessBreakdownAsync(businessPlanId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.MissingElements.Should().BeEmpty();
        result.Value.Sections.Should().HaveCount(6);
        result.Value.Sections.Should().OnlyContain(s => s.Score > 0);
    }

    [Fact]
    public async Task GetReadinessBreakdownAsync_WithMissingCompetitionContent_ShouldIdentifyCompetitiveRiskGap()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var businessPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);

        // No mention of competition/concurrent in any section
        var plan = CreateBusinessPlan(
            id: businessPlanId,
            organizationId: orgId,
            swotAnalysis: "We have a strong brand and market position.",
            marketAnalysis: "The market is growing at a 25% rate.",
            financialProjections: "Revenue: 500000 in year 1.");

        var member = new OrganizationMember(orgId, userId, OrganizationRole.Owner);
        SetupDbSets(new List<BusinessPlan> { plan }, new List<OrganizationMember> { member });

        // Act
        var result = await _sut.GetReadinessBreakdownAsync(businessPlanId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.RiskGaps.Should().Contain("Competitive risk analysis is missing");
    }

    [Fact]
    public async Task GetReadinessBreakdownAsync_WithLargeRevenueAndNicheMarket_ShouldIdentifyInconsistency()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var businessPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);

        // Large revenue claim ($10 million) with niche market
        var plan = CreateBusinessPlan(
            id: businessPlanId,
            organizationId: orgId,
            financialProjections: "We project revenue of $10 million in year 1.",
            marketAnalysis: "We target a niche market of specialized customers.");

        var member = new OrganizationMember(orgId, userId, OrganizationRole.Owner);
        SetupDbSets(new List<BusinessPlan> { plan }, new List<OrganizationMember> { member });

        // Act
        var result = await _sut.GetReadinessBreakdownAsync(businessPlanId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.InconsistentElements.Should().Contain(
            "Revenue projections may be inconsistent with niche market positioning");
    }

    #endregion

    #region RecalculateAndSaveAsync Tests

    [Fact]
    public async Task RecalculateAndSaveAsync_WhenUserNotAuthenticated_ShouldReturnFailure()
    {
        // Arrange
        var businessPlanId = Guid.NewGuid();
        _currentUserServiceMock.Setup(x => x.UserId).Returns((string?)null);

        // Act
        var result = await _sut.RecalculateAndSaveAsync(businessPlanId);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task RecalculateAndSaveAsync_WithValidPlan_ShouldSaveScoreToDatabase()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var businessPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);

        var plan = CreateBusinessPlan(
            id: businessPlanId,
            organizationId: orgId,
            executiveSummary: GenerateRichContent(200));

        var member = new OrganizationMember(orgId, userId, OrganizationRole.Owner);
        SetupDbSets(new List<BusinessPlan> { plan }, new List<OrganizationMember> { member });

        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.RecalculateAndSaveAsync(businessPlanId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeGreaterThanOrEqualTo(0m);
        result.Value.Should().BeLessThanOrEqualTo(100m);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Async DbSet Mock Infrastructure

    /// <summary>
    /// Provides async query capabilities for mocked DbSets.
    /// </summary>
    private class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        internal TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
        {
            return new TestAsyncEnumerable<TEntity>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression)
        {
            return new TestAsyncEnumerable<TElement>(expression);
        }

        public object? Execute(System.Linq.Expressions.Expression expression)
        {
            return _inner.Execute(expression);
        }

        public TResult Execute<TResult>(System.Linq.Expressions.Expression expression)
        {
            return _inner.Execute<TResult>(expression);
        }

        public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression)
        {
            return new TestAsyncEnumerable<TResult>(expression);
        }

        public TResult ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression, CancellationToken cancellationToken = default)
        {
            var resultType = typeof(TResult).GetGenericArguments()[0];
            var executionResult = typeof(IQueryProvider)
                .GetMethod(
                    name: nameof(IQueryProvider.Execute),
                    genericParameterCount: 1,
                    types: new[] { typeof(System.Linq.Expressions.Expression) })!
                .MakeGenericMethod(resultType)
                .Invoke(this, new object[] { expression });

            return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(resultType)
                .Invoke(null, new[] { executionResult })!;
        }
    }

    private class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable)
            : base(enumerable)
        { }

        public TestAsyncEnumerable(System.Linq.Expressions.Expression expression)
            : base(expression)
        { }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }

    private class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> MoveNextAsync()
        {
            return ValueTask.FromResult(_inner.MoveNext());
        }

        public T Current => _inner.Current;
    }

    #endregion
}

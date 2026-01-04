using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.OBNL.Commands;
using Sqordia.Application.OBNL.Queries;
using Sqordia.Application.OBNL.Services;
using Sqordia.Domain.Entities;
using Sqordia.Domain.ValueObjects;
using Sqordia.Persistence.Contexts;
using Xunit;

namespace Sqordia.Application.UnitTests.Services.OBNL;

public class OBNLPlanServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Fixture _fixture;
    private readonly IOBNLPlanService _service;

    public OBNLPlanServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _fixture = new Fixture();

        // Create the real service for testing
        _service = new OBNLPlanService(_context, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task CreateOBNLPlanAsync_ValidCommand_ShouldReturnPlanId()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var command = new CreateOBNLPlanCommand
        {
            OrganizationId = organizationId,
            OBNLType = "Charitable Organization",
            Mission = "To serve the community",
            Vision = "A thriving community",
            Values = "Compassion, Integrity",
            FundingRequirements = 250000,
            FundingPurpose = "Program expansion",
            LegalStructure = "Non-Profit Corporation",
            RegistrationNumber = "123456789",
            RegistrationDate = DateTime.UtcNow,
            GoverningBody = "Board of Directors",
            BoardComposition = "5 members",
            StakeholderEngagement = "Community engagement plan",
            ImpactMeasurement = "Impact measurement framework",
            SustainabilityStrategy = "Sustainability strategy"
        };

        // Act
        var result = await _service.CreateOBNLPlanAsync(command);

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetOBNLPlanAsync_ExistingPlan_ShouldReturnPlan()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();

        var plan = new OBNLBusinessPlan
        {
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
            StakeholderEngagement = "Test Engagement",
            ImpactMeasurement = "Test Measurement",
            SustainabilityStrategy = "Test Strategy",
            ComplianceStatus = ComplianceStatus.Pending,
            CreatedBy = "test-user"
        };

        _context.OBNLBusinessPlans.Add(plan);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetOBNLPlanAsync(plan.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(plan.Id);
        result.OrganizationId.Should().Be(organizationId);
        result.Mission.Should().Be("Test Mission");
    }

    [Fact]
    public async Task GetOBNLPlansByOrganizationAsync_MultiplePlans_ShouldReturnAllPlans()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        
        var plan1 = new OBNLBusinessPlan
        {
            OrganizationId = organizationId,
            OBNLType = "Charitable Organization",
            Mission = "Mission 1",
            Vision = "Vision 1",
            Values = "Values 1",
            FundingRequirements = 100000,
            FundingPurpose = "Purpose 1",
            LegalStructure = "Non-Profit Corporation",
            RegistrationNumber = "111111111",
            RegistrationDate = DateTime.UtcNow,
            GoverningBody = "Board of Directors",
            BoardComposition = "5 members",
            StakeholderEngagement = "Engagement 1",
            ImpactMeasurement = "Measurement 1",
            SustainabilityStrategy = "Strategy 1",
            ComplianceStatus = ComplianceStatus.Pending,
            CreatedBy = "test-user"
        };

        var plan2 = new OBNLBusinessPlan
        {
            OrganizationId = organizationId,
            OBNLType = "Foundation",
            Mission = "Mission 2",
            Vision = "Vision 2",
            Values = "Values 2",
            FundingRequirements = 200000,
            FundingPurpose = "Purpose 2",
            LegalStructure = "Foundation",
            RegistrationNumber = "222222222",
            RegistrationDate = DateTime.UtcNow,
            GoverningBody = "Board of Trustees",
            BoardComposition = "7 members",
            StakeholderEngagement = "Engagement 2",
            ImpactMeasurement = "Measurement 2",
            SustainabilityStrategy = "Strategy 2",
            ComplianceStatus = ComplianceStatus.Pending,
            CreatedBy = "test-user"
        };

        _context.OBNLBusinessPlans.AddRange(plan1, plan2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetOBNLPlansByOrganizationAsync(organizationId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Mission == "Mission 1");
        result.Should().Contain(p => p.Mission == "Mission 2");
    }

    [Fact]
    public async Task AnalyzeComplianceAsync_ValidPlan_ShouldReturnComplianceAnalysis()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();

        var plan = new OBNLBusinessPlan
        {
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
            StakeholderEngagement = "Test Engagement",
            ImpactMeasurement = "Test Measurement",
            SustainabilityStrategy = "Test Strategy",
            ComplianceStatus = ComplianceStatus.Pending,
            CreatedBy = "test-user"
        };

        _context.OBNLBusinessPlans.Add(plan);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.AnalyzeComplianceAsync(plan.Id);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Compliant");
        result.Level.Should().Be("High");
        result.Requirements.Should().NotBeEmpty();
        result.Recommendations.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateGrantApplicationAsync_ValidRequest_ShouldReturnApplicationId()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();

        var plan = new OBNLBusinessPlan
        {
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
            StakeholderEngagement = "Test Engagement",
            ImpactMeasurement = "Test Measurement",
            SustainabilityStrategy = "Test Strategy",
            ComplianceStatus = ComplianceStatus.Pending,
            CreatedBy = "test-user"
        };

        _context.OBNLBusinessPlans.Add(plan);
        await _context.SaveChangesAsync();

        var command = new CreateGrantApplicationCommand
        {
            OBNLBusinessPlanId = planId,
            GrantName = "Community Development Grant",
            GrantingOrganization = "Community Foundation",
            GrantType = "Program Grant",
            RequestedAmount = 50000,
            MatchingFunds = 10000,
            ProjectDescription = "Community development project",
            Objectives = "Improve community services",
            ExpectedOutcomes = "Increased community engagement",
            TargetPopulation = "Local residents",
            GeographicScope = "Local community",
            Timeline = "12 months",
            BudgetBreakdown = "Detailed budget breakdown",
            EvaluationPlan = "Evaluation methodology",
            SustainabilityPlan = "Long-term sustainability plan",
            ApplicationDeadline = DateTime.UtcNow.AddMonths(1),
            SubmissionDate = DateTime.UtcNow,
            Status = "Draft",
            Notes = "Grant application notes",
            CreatedBy = "test-user"
        };

        // Act
        var result = await _service.CreateGrantApplicationAsync(command);

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateImpactMeasurementAsync_ValidRequest_ShouldReturnMeasurementId()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();

        var plan = new OBNLBusinessPlan
        {
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
            StakeholderEngagement = "Test Engagement",
            ImpactMeasurement = "Test Measurement",
            SustainabilityStrategy = "Test Strategy",
            ComplianceStatus = ComplianceStatus.Pending,
            CreatedBy = "test-user"
        };

        _context.OBNLBusinessPlans.Add(plan);
        await _context.SaveChangesAsync();

        var command = new CreateImpactMeasurementCommand
        {
            OBNLBusinessPlanId = planId,
            MetricName = "Community Impact",
            Description = "Measure community impact",
            MeasurementType = "Quantitative",
            UnitOfMeasurement = "Count",
            BaselineValue = 0,
            TargetValue = 500,
            CurrentValue = 0,
            DataSource = "Community surveys",
            CollectionMethod = "Survey",
            Frequency = "Monthly",
            ResponsibleParty = "Program Manager",
            LastMeasurement = DateTime.UtcNow,
            NextMeasurement = DateTime.UtcNow.AddMonths(1),
            Status = "Active",
            Notes = "Impact measurement notes",
            CreatedBy = "test-user"
        };

        // Act
        var result = await _service.CreateImpactMeasurementAsync(command);

        // Assert
        result.Should().NotBeEmpty();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

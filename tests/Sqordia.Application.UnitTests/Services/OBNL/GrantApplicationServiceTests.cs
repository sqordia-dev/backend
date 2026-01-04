using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.OBNL.Commands;
using Sqordia.Application.OBNL.Queries;
using Sqordia.Domain.Entities;
using Sqordia.Domain.ValueObjects;
using Sqordia.Persistence.Contexts;
using Xunit;

namespace Sqordia.Application.UnitTests.Services.OBNL;

public class GrantApplicationServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Fixture _fixture;

    public GrantApplicationServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _fixture = new Fixture();
    }

    [Fact]
    public async Task CreateGrantApplication_ValidRequest_ShouldCreateApplication()
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

        var application = new GrantApplication
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
            Decision = "Pending",
            Notes = "Grant application notes"
        };

        // Act
        _context.GrantApplications.Add(application);
        await _context.SaveChangesAsync();

        // Assert
        var savedApplication = await _context.GrantApplications.FirstOrDefaultAsync();
        savedApplication.Should().NotBeNull();
        savedApplication!.GrantName.Should().Be("Community Development Grant");
        savedApplication.GrantingOrganization.Should().Be("Community Foundation");
        savedApplication.GrantType.Should().Be("Program Grant");
        savedApplication.RequestedAmount.Should().Be(50000);
        savedApplication.MatchingFunds.Should().Be(10000);
        savedApplication.ProjectDescription.Should().Be("Community development project");
        savedApplication.Objectives.Should().Be("Improve community services");
        savedApplication.ExpectedOutcomes.Should().Be("Increased community engagement");
        savedApplication.TargetPopulation.Should().Be("Local residents");
        savedApplication.GeographicScope.Should().Be("Local community");
        savedApplication.Timeline.Should().Be("12 months");
        savedApplication.BudgetBreakdown.Should().Be("Detailed budget breakdown");
        savedApplication.EvaluationPlan.Should().Be("Evaluation methodology");
        savedApplication.SustainabilityPlan.Should().Be("Long-term sustainability plan");
        savedApplication.Status.Should().Be("Draft");
        savedApplication.Decision.Should().Be("Pending");
        savedApplication.Notes.Should().Be("Grant application notes");
    }

    [Fact]
    public async Task GetGrantApplications_ExistingApplications_ShouldReturnAllApplications()
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

        var application1 = new GrantApplication
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
            Decision = "Pending",
            Notes = "Grant application notes"
        };

        var application2 = new GrantApplication
        {
            OBNLBusinessPlanId = planId,
            GrantName = "Capacity Building Grant",
            GrantingOrganization = "State Foundation",
            GrantType = "Capacity Building Grant",
            RequestedAmount = 25000,
            MatchingFunds = 5000,
            ProjectDescription = "Capacity building project",
            Objectives = "Build organizational capacity",
            ExpectedOutcomes = "Improved organizational effectiveness",
            TargetPopulation = "Organization staff",
            GeographicScope = "Statewide",
            Timeline = "18 months",
            BudgetBreakdown = "Capacity building budget",
            EvaluationPlan = "Capacity evaluation",
            SustainabilityPlan = "Long-term capacity plan",
            ApplicationDeadline = DateTime.UtcNow.AddMonths(2),
            SubmissionDate = DateTime.UtcNow,
            Status = "Submitted",
            Decision = "Under Review",
            Notes = "Capacity building application"
        };

        _context.GrantApplications.AddRange(application1, application2);
        await _context.SaveChangesAsync();

        // Act
        var applications = await _context.GrantApplications
            .Where(a => a.OBNLBusinessPlanId == planId)
            .ToListAsync();

        // Assert
        applications.Should().HaveCount(2);
        applications.Should().Contain(a => a.GrantName == "Community Development Grant");
        applications.Should().Contain(a => a.GrantName == "Capacity Building Grant");
    }

    [Fact]
    public async Task UpdateGrantApplication_ValidRequest_ShouldUpdateApplication()
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

        var application = new GrantApplication
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
            Decision = "Pending",
            Notes = "Grant application notes"
        };

        _context.GrantApplications.Add(application);
        await _context.SaveChangesAsync();

        // Act
        application.Status = "Submitted";
        application.Decision = "Under Review";
        application.Notes = "Application submitted for review";
        _context.GrantApplications.Update(application);
        await _context.SaveChangesAsync();

        // Assert
        var updatedApplication = await _context.GrantApplications.FirstOrDefaultAsync();
        updatedApplication.Should().NotBeNull();
        updatedApplication!.Status.Should().Be("Submitted");
        updatedApplication.Decision.Should().Be("Under Review");
        updatedApplication.Notes.Should().Be("Application submitted for review");
    }

    [Fact]
    public async Task DeleteGrantApplication_ExistingApplication_ShouldDeleteApplication()
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

        var application = new GrantApplication
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
            Decision = "Pending",
            Notes = "Grant application notes"
        };

        _context.GrantApplications.Add(application);
        await _context.SaveChangesAsync();

        // Act
        _context.GrantApplications.Remove(application);
        await _context.SaveChangesAsync();

        // Assert
        var deletedApplication = await _context.GrantApplications.FirstOrDefaultAsync();
        deletedApplication.Should().BeNull();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.OBNL.Commands;
using Sqordia.Domain.Entities;
using Sqordia.Persistence.Contexts;
using Sqordia.Persistence.Repositories;
using Xunit;

namespace Sqordia.Application.UnitTests.Services.OBNL;

public class CreateOBNLPlanCommandHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CreateOBNLPlanCommandHandler _handler;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Fixture _fixture;

    public CreateOBNLPlanCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _fixture = new Fixture();

        _handler = new CreateOBNLPlanCommandHandler(_context, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateOBNLPlan()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var userId = "test-user-id";
        
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

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
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        
        var createdPlan = await _context.OBNLBusinessPlans.FirstOrDefaultAsync();
        createdPlan.Should().NotBeNull();
        createdPlan!.OrganizationId.Should().Be(organizationId);
        createdPlan.OBNLType.Should().Be("Charitable Organization");
        createdPlan.Mission.Should().Be("To serve the community");
        createdPlan.FundingRequirements.Should().Be(250000);
        createdPlan.ComplianceStatus.Status.Should().Be("Pending");
        createdPlan.CreatedBy.Should().Be(userId);
    }

    [Fact]
    public async Task Handle_WithNullCurrentUser_ShouldStillCreatePlan()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        
        _currentUserServiceMock.Setup(x => x.UserId).Returns((string?)null);

        var command = new CreateOBNLPlanCommand
        {
            OrganizationId = organizationId,
            OBNLType = "Charitable Organization",
            Mission = "To serve the community",
            Vision = "A thriving community",
            Values = "Compassion, Integrity",
            FundingRequirements = 100000,
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
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        
        var createdPlan = await _context.OBNLBusinessPlans.FirstOrDefaultAsync();
        createdPlan.Should().NotBeNull();
        createdPlan!.CreatedBy.Should().Be("System");
    }

    [Fact]
    public async Task Handle_MultiplePlans_ShouldCreateSeparatePlans()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var userId = "test-user-id";
        
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        var command1 = new CreateOBNLPlanCommand
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
            SustainabilityStrategy = "Strategy 1"
        };

        var command2 = new CreateOBNLPlanCommand
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
            SustainabilityStrategy = "Strategy 2"
        };

        // Act
        var result1 = await _handler.Handle(command1, CancellationToken.None);
        var result2 = await _handler.Handle(command2, CancellationToken.None);

        // Assert
        result1.Should().NotBeEmpty();
        result2.Should().NotBeEmpty();
        result1.Should().NotBe(result2);

        var plans = await _context.OBNLBusinessPlans.ToListAsync();
        plans.Should().HaveCount(2);
        plans.Should().Contain(p => p.Mission == "Mission 1");
        plans.Should().Contain(p => p.Mission == "Mission 2");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

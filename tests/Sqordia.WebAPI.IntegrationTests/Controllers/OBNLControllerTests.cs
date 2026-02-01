using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Sqordia.Application.OBNL.Commands;
using Sqordia.Application.OBNL.Queries;
using Sqordia.Application.OBNL.Services;
using WebAPI.Controllers;
using Xunit;

namespace Sqordia.WebAPI.IntegrationTests.Controllers;

public class OBNLControllerTests
{
    private readonly Mock<IOBNLPlanService> _obnlPlanServiceMock;
    private readonly OBNLController _sut;

    public OBNLControllerTests()
    {
        _obnlPlanServiceMock = new Mock<IOBNLPlanService>();

        _sut = new OBNLController(_obnlPlanServiceMock.Object);

        // Set up default authenticated HttpContext
        var userId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, "testuser@example.com"),
            new Claim(ClaimTypes.Name, "testuser")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    [Fact]
    public async Task CreateOBNLPlan_ValidRequest_ShouldReturnCreated()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var command = new CreateOBNLPlanCommand
        {
            OrganizationId = Guid.NewGuid(),
            OBNLType = "Charitable Organization",
            Mission = "To serve the community",
            Vision = "A thriving community",
            Values = "Compassion, Integrity",
            FundingRequirements = 250000m,
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

        _obnlPlanServiceMock
            .Setup(x => x.CreateOBNLPlanAsync(It.IsAny<CreateOBNLPlanCommand>()))
            .ReturnsAsync(planId);

        // Act
        var result = await _sut.CreateOBNLPlan(command);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task CreateOBNLPlan_WhenServiceThrows_ShouldReturnBadRequest()
    {
        // Arrange
        var command = new CreateOBNLPlanCommand
        {
            OrganizationId = Guid.NewGuid(),
            OBNLType = "Charitable Organization",
            Mission = "To serve the community"
        };

        _obnlPlanServiceMock
            .Setup(x => x.CreateOBNLPlanAsync(It.IsAny<CreateOBNLPlanCommand>()))
            .ThrowsAsync(new Exception("Validation failed"));

        // Act
        var result = await _sut.CreateOBNLPlan(command);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task GetOBNLPlan_ExistingPlan_ShouldReturnPlan()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var planDto = new OBNLPlanDto
        {
            Id = planId,
            OrganizationId = Guid.NewGuid(),
            OBNLType = "Charitable Organization",
            Mission = "Test Mission",
            Vision = "Test Vision",
            Values = "Test Values",
            FundingRequirements = 100000,
            FundingPurpose = "Test Purpose",
            ComplianceStatus = "Pending",
            LegalStructure = "Non-Profit Corporation",
            CreatedBy = "test-user"
        };

        _obnlPlanServiceMock
            .Setup(x => x.GetOBNLPlanAsync(planId))
            .ReturnsAsync(planDto);

        // Act
        var result = await _sut.GetOBNLPlan(planId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        var returnedPlan = okResult.Value.Should().BeOfType<OBNLPlanDto>().Subject;
        returnedPlan.Mission.Should().Be("Test Mission");
    }

    [Fact]
    public async Task GetOBNLPlan_NonExistentPlan_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        _obnlPlanServiceMock
            .Setup(x => x.GetOBNLPlanAsync(nonExistentId))
            .ThrowsAsync(new KeyNotFoundException("Plan not found"));

        // Act
        var result = await _sut.GetOBNLPlan(nonExistentId);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetOBNLPlansByOrganization_ExistingPlans_ShouldReturnPlans()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var plans = new List<OBNLPlanDto>
        {
            new OBNLPlanDto
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                OBNLType = "Charitable Organization",
                Mission = "Mission 1"
            },
            new OBNLPlanDto
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                OBNLType = "Foundation",
                Mission = "Mission 2"
            }
        };

        _obnlPlanServiceMock
            .Setup(x => x.GetOBNLPlansByOrganizationAsync(organizationId))
            .ReturnsAsync(plans);

        // Act
        var result = await _sut.GetOBNLPlansByOrganization(organizationId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        var returnedPlans = okResult.Value.Should().BeAssignableTo<List<OBNLPlanDto>>().Subject;
        returnedPlans.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateOBNLPlan_ValidRequest_ShouldReturnOk()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var command = new UpdateOBNLPlanCommand
        {
            Mission = "Updated Mission",
            Vision = "Updated Vision",
            Values = "Updated Values",
            FundingRequirements = 150000m,
            FundingPurpose = "Updated Purpose"
        };

        _obnlPlanServiceMock
            .Setup(x => x.UpdateOBNLPlanAsync(It.IsAny<UpdateOBNLPlanCommand>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateOBNLPlan(planId, command);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task UpdateOBNLPlan_ShouldPassCorrectIdToService()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var command = new UpdateOBNLPlanCommand
        {
            Mission = "Updated Mission"
        };

        UpdateOBNLPlanCommand? capturedCommand = null;
        _obnlPlanServiceMock
            .Setup(x => x.UpdateOBNLPlanAsync(It.IsAny<UpdateOBNLPlanCommand>()))
            .Callback<UpdateOBNLPlanCommand>(cmd => capturedCommand = cmd)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.UpdateOBNLPlan(planId, command);

        // Assert
        capturedCommand.Should().NotBeNull();
        capturedCommand!.Id.Should().Be(planId);
    }

    [Fact]
    public async Task DeleteOBNLPlan_ExistingPlan_ShouldReturnNoContent()
    {
        // Arrange
        var planId = Guid.NewGuid();

        _obnlPlanServiceMock
            .Setup(x => x.DeleteOBNLPlanAsync(planId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.DeleteOBNLPlan(planId);

        // Assert
        var noContentResult = result.Should().BeOfType<NoContentResult>().Subject;
        noContentResult.StatusCode.Should().Be(204);
    }

    [Fact]
    public async Task DeleteOBNLPlan_WhenServiceThrows_ShouldReturnBadRequest()
    {
        // Arrange
        var planId = Guid.NewGuid();

        _obnlPlanServiceMock
            .Setup(x => x.DeleteOBNLPlanAsync(planId))
            .ThrowsAsync(new Exception("Plan not found"));

        // Act
        var result = await _sut.DeleteOBNLPlan(planId);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task AnalyzeCompliance_ExistingPlan_ShouldReturnComplianceAnalysis()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var analysis = new ComplianceAnalysisDto
        {
            Status = "Compliant",
            Level = "Full",
            Requirements = new List<string> { "Req1", "Req2" },
            Recommendations = new List<string> { "Rec1" },
            LastUpdated = DateTime.UtcNow,
            Notes = "All requirements met"
        };

        _obnlPlanServiceMock
            .Setup(x => x.AnalyzeComplianceAsync(planId))
            .ReturnsAsync(analysis);

        // Act
        var result = await _sut.AnalyzeCompliance(planId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        var returnedAnalysis = okResult.Value.Should().BeOfType<ComplianceAnalysisDto>().Subject;
        returnedAnalysis.Status.Should().Be("Compliant");
    }

    [Fact]
    public async Task AnalyzeCompliance_WhenServiceThrows_ShouldReturnBadRequest()
    {
        // Arrange
        var planId = Guid.NewGuid();

        _obnlPlanServiceMock
            .Setup(x => x.AnalyzeComplianceAsync(planId))
            .ThrowsAsync(new Exception("Plan not found"));

        // Act
        var result = await _sut.AnalyzeCompliance(planId);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task CreateOBNLPlan_ShouldCallServiceExactlyOnce()
    {
        // Arrange
        var command = new CreateOBNLPlanCommand
        {
            OrganizationId = Guid.NewGuid(),
            OBNLType = "Foundation",
            Mission = "Test Mission"
        };

        _obnlPlanServiceMock
            .Setup(x => x.CreateOBNLPlanAsync(It.IsAny<CreateOBNLPlanCommand>()))
            .ReturnsAsync(Guid.NewGuid());

        // Act
        await _sut.CreateOBNLPlan(command);

        // Assert
        _obnlPlanServiceMock.Verify(
            x => x.CreateOBNLPlanAsync(It.IsAny<CreateOBNLPlanCommand>()),
            Times.Once);
    }

    [Fact]
    public async Task GetOBNLPlan_ShouldCallServiceWithCorrectId()
    {
        // Arrange
        var planId = Guid.NewGuid();

        _obnlPlanServiceMock
            .Setup(x => x.GetOBNLPlanAsync(planId))
            .ReturnsAsync(new OBNLPlanDto { Id = planId });

        // Act
        await _sut.GetOBNLPlan(planId);

        // Assert
        _obnlPlanServiceMock.Verify(
            x => x.GetOBNLPlanAsync(planId),
            Times.Once);
    }
}

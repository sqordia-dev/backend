using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Services;
using Sqordia.Application.Services.V2;
using Sqordia.Contracts.Requests.BusinessPlan;
using Sqordia.Contracts.Responses.BusinessPlan;
using WebAPI.Controllers;
using Xunit;

namespace Sqordia.WebAPI.IntegrationTests.Controllers;

public class BusinessPlanControllerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IBusinessPlanService> _businessPlanServiceMock;
    private readonly Mock<IStrategyMapService> _strategyMapServiceMock;
    private readonly Mock<IReadinessScoreService> _readinessScoreServiceMock;
    private readonly Mock<IAuditService> _auditServiceMock;
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ILogger<BusinessPlanController>> _loggerMock;
    private readonly BusinessPlanController _sut;

    public BusinessPlanControllerTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _businessPlanServiceMock = new Mock<IBusinessPlanService>();
        _strategyMapServiceMock = new Mock<IStrategyMapService>();
        _readinessScoreServiceMock = new Mock<IReadinessScoreService>();
        _auditServiceMock = new Mock<IAuditService>();
        _contextMock = new Mock<IApplicationDbContext>();
        _loggerMock = new Mock<ILogger<BusinessPlanController>>();

        _sut = new BusinessPlanController(
            _businessPlanServiceMock.Object,
            _strategyMapServiceMock.Object,
            _readinessScoreServiceMock.Object,
            _auditServiceMock.Object,
            _contextMock.Object,
            _loggerMock.Object);

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

    #region CreateBusinessPlan

    [Fact]
    public async Task CreateBusinessPlan_WithValidData_ShouldReturnOkResult()
    {
        // Arrange
        var request = new CreateBusinessPlanRequest
        {
            Title = "Test Business Plan",
            Description = "A test plan description",
            PlanType = "BusinessPlan",
            OrganizationId = Guid.NewGuid(),
            Persona = "Entrepreneur"
        };
        var businessPlanResponse = _fixture.Create<BusinessPlanResponse>();
        var result = Result.Success(businessPlanResponse);

        _businessPlanServiceMock
            .Setup(x => x.CreateBusinessPlanAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.CreateBusinessPlan(request, CancellationToken.None);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(businessPlanResponse);
    }

    [Fact]
    public async Task CreateBusinessPlan_WhenServiceReturnsValidationFailure_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateBusinessPlanRequest
        {
            Title = "AB",
            Description = null,
            PlanType = "InvalidType",
            OrganizationId = Guid.NewGuid()
        };
        var error = Error.Validation("BusinessPlan.Validation", "Validation failed");
        var result = Result.Failure<BusinessPlanResponse>(error);

        _businessPlanServiceMock
            .Setup(x => x.CreateBusinessPlanAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.CreateBusinessPlan(request, CancellationToken.None);

        // Assert
        response.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = response as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be(error);
    }

    [Fact]
    public async Task CreateBusinessPlan_WhenServiceReturnsConflict_ShouldReturnConflict()
    {
        // Arrange
        var request = new CreateBusinessPlanRequest
        {
            Title = "Duplicate Plan",
            PlanType = "BusinessPlan",
            OrganizationId = Guid.NewGuid()
        };
        var error = Error.Conflict("BusinessPlan.Conflict", "A plan with this title already exists");
        var result = Result.Failure<BusinessPlanResponse>(error);

        _businessPlanServiceMock
            .Setup(x => x.CreateBusinessPlanAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.CreateBusinessPlan(request, CancellationToken.None);

        // Assert
        response.Should().BeOfType<ConflictObjectResult>();
        var conflictResult = response as ConflictObjectResult;
        conflictResult!.Value.Should().Be(error);
    }

    [Fact]
    public async Task CreateBusinessPlan_ShouldCallServiceExactlyOnce()
    {
        // Arrange
        var request = new CreateBusinessPlanRequest
        {
            Title = "Test Plan",
            PlanType = "LeanCanvas",
            OrganizationId = Guid.NewGuid()
        };
        var businessPlanResponse = _fixture.Create<BusinessPlanResponse>();
        var result = Result.Success(businessPlanResponse);

        _businessPlanServiceMock
            .Setup(x => x.CreateBusinessPlanAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        await _sut.CreateBusinessPlan(request, CancellationToken.None);

        // Assert
        _businessPlanServiceMock.Verify(
            x => x.CreateBusinessPlanAsync(request, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetBusinessPlan

    [Fact]
    public async Task GetBusinessPlan_WithValidId_ShouldReturnOkResult()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var businessPlanResponse = _fixture.Create<BusinessPlanResponse>();
        var result = Result.Success(businessPlanResponse);

        _businessPlanServiceMock
            .Setup(x => x.GetBusinessPlanAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.GetBusinessPlan(planId, sections: false, CancellationToken.None);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(businessPlanResponse);
    }

    [Fact]
    public async Task GetBusinessPlan_WithSectionsFlag_ShouldReturnOkResult()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var businessPlanResponse = _fixture.Create<BusinessPlanResponse>();
        var result = Result.Success(businessPlanResponse);

        _businessPlanServiceMock
            .Setup(x => x.GetBusinessPlanAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.GetBusinessPlan(planId, sections: true, CancellationToken.None);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(businessPlanResponse);
    }

    [Fact]
    public async Task GetBusinessPlan_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var error = Error.NotFound("BusinessPlan.NotFound", "Business plan not found");
        var result = Result.Failure<BusinessPlanResponse>(error);

        _businessPlanServiceMock
            .Setup(x => x.GetBusinessPlanAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.GetBusinessPlan(planId, sections: false, CancellationToken.None);

        // Assert
        response.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = response as NotFoundObjectResult;
        notFoundResult!.Value.Should().Be(error);
    }

    [Fact]
    public async Task GetBusinessPlan_WhenServiceReturnsSuccessWithNullValue_ShouldReturnNotFound()
    {
        // Arrange
        var planId = Guid.NewGuid();
        // Result.Success<T>(null) creates a success result with null value
        var result = Result.Success<BusinessPlanResponse>(null!);

        _businessPlanServiceMock
            .Setup(x => x.GetBusinessPlanAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.GetBusinessPlan(planId, sections: false, CancellationToken.None);

        // Assert
        response.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetBusinessPlan_WhenUserForbidden_ShouldReturnForbidden()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var error = Error.Forbidden("BusinessPlan.Forbidden", "You do not have access to this business plan");
        var result = Result.Failure<BusinessPlanResponse>(error);

        _businessPlanServiceMock
            .Setup(x => x.GetBusinessPlanAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.GetBusinessPlan(planId, sections: false, CancellationToken.None);

        // Assert
        var objectResult = response as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult!.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        objectResult.Value.Should().Be(error);
    }

    #endregion

    #region GetUserBusinessPlans

    [Fact]
    public async Task GetUserBusinessPlans_ShouldReturnOkWithPlans()
    {
        // Arrange
        var plans = _fixture.CreateMany<BusinessPlanResponse>(3).AsEnumerable();
        var result = Result.Success(plans);

        _businessPlanServiceMock
            .Setup(x => x.GetUserBusinessPlansAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.GetUserBusinessPlans(CancellationToken.None);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(plans);
    }

    [Fact]
    public async Task GetUserBusinessPlans_WhenNoPlans_ShouldReturnOkWithEmptyList()
    {
        // Arrange
        var plans = Enumerable.Empty<BusinessPlanResponse>();
        var result = Result.Success(plans);

        _businessPlanServiceMock
            .Setup(x => x.GetUserBusinessPlansAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.GetUserBusinessPlans(CancellationToken.None);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(plans);
    }

    [Fact]
    public async Task GetUserBusinessPlans_WhenServiceFails_ShouldReturnBadRequest()
    {
        // Arrange
        var error = Error.Failure("BusinessPlan.GetAll.Failed", "Failed to retrieve business plans");
        var result = Result.Failure<IEnumerable<BusinessPlanResponse>>(error);

        _businessPlanServiceMock
            .Setup(x => x.GetUserBusinessPlansAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.GetUserBusinessPlans(CancellationToken.None);

        // Assert
        response.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = response as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be(error);
    }

    #endregion

    #region GetOrganizationBusinessPlans

    [Fact]
    public async Task GetOrganizationBusinessPlans_WithValidOrgId_ShouldReturnOkWithPlans()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var plans = _fixture.CreateMany<BusinessPlanResponse>(2).AsEnumerable();
        var result = Result.Success(plans);

        _businessPlanServiceMock
            .Setup(x => x.GetOrganizationBusinessPlansAsync(organizationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.GetOrganizationBusinessPlans(organizationId, CancellationToken.None);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(plans);
    }

    [Fact]
    public async Task GetOrganizationBusinessPlans_WhenNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var error = Error.NotFound("Organization.NotFound", "Organization not found");
        var result = Result.Failure<IEnumerable<BusinessPlanResponse>>(error);

        _businessPlanServiceMock
            .Setup(x => x.GetOrganizationBusinessPlansAsync(organizationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.GetOrganizationBusinessPlans(organizationId, CancellationToken.None);

        // Assert
        response.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = response as NotFoundObjectResult;
        notFoundResult!.Value.Should().Be(error);
    }

    #endregion

    #region UpdateBusinessPlan

    [Fact]
    public async Task UpdateBusinessPlan_WithValidData_ShouldReturnOkResult()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var request = new UpdateBusinessPlanRequest
        {
            Title = "Updated Business Plan Title",
            Description = "Updated description"
        };
        var businessPlanResponse = _fixture.Create<BusinessPlanResponse>();
        var result = Result.Success(businessPlanResponse);

        _businessPlanServiceMock
            .Setup(x => x.UpdateBusinessPlanAsync(planId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.UpdateBusinessPlan(planId, request, CancellationToken.None);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(businessPlanResponse);
    }

    [Fact]
    public async Task UpdateBusinessPlan_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var request = new UpdateBusinessPlanRequest
        {
            Title = "Updated Title"
        };
        var error = Error.NotFound("BusinessPlan.NotFound", "Business plan not found");
        var result = Result.Failure<BusinessPlanResponse>(error);

        _businessPlanServiceMock
            .Setup(x => x.UpdateBusinessPlanAsync(planId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.UpdateBusinessPlan(planId, request, CancellationToken.None);

        // Assert
        response.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = response as NotFoundObjectResult;
        notFoundResult!.Value.Should().Be(error);
    }

    [Fact]
    public async Task UpdateBusinessPlan_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var request = new UpdateBusinessPlanRequest
        {
            Title = "AB" // Too short
        };
        var error = Error.Validation("BusinessPlan.Validation", "Title must be at least 3 characters");
        var result = Result.Failure<BusinessPlanResponse>(error);

        _businessPlanServiceMock
            .Setup(x => x.UpdateBusinessPlanAsync(planId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.UpdateBusinessPlan(planId, request, CancellationToken.None);

        // Assert
        response.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = response as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be(error);
    }

    [Fact]
    public async Task UpdateBusinessPlan_ShouldPassCorrectIdToService()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var request = new UpdateBusinessPlanRequest
        {
            Title = "Updated Title"
        };
        var businessPlanResponse = _fixture.Create<BusinessPlanResponse>();
        var result = Result.Success(businessPlanResponse);

        _businessPlanServiceMock
            .Setup(x => x.UpdateBusinessPlanAsync(planId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        await _sut.UpdateBusinessPlan(planId, request, CancellationToken.None);

        // Assert
        _businessPlanServiceMock.Verify(
            x => x.UpdateBusinessPlanAsync(planId, request, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region DeleteBusinessPlan

    [Fact]
    public async Task DeleteBusinessPlan_WithValidId_ShouldReturnOkResult()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var result = Result.Success();

        _businessPlanServiceMock
            .Setup(x => x.DeleteBusinessPlanAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.DeleteBusinessPlan(planId, CancellationToken.None);

        // Assert
        response.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task DeleteBusinessPlan_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var error = Error.NotFound("BusinessPlan.NotFound", "Business plan not found");
        var result = Result.Failure(error);

        _businessPlanServiceMock
            .Setup(x => x.DeleteBusinessPlanAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.DeleteBusinessPlan(planId, CancellationToken.None);

        // Assert
        response.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = response as NotFoundObjectResult;
        notFoundResult!.Value.Should().Be(error);
    }

    [Fact]
    public async Task DeleteBusinessPlan_WhenForbidden_ShouldReturnForbidden()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var error = Error.Forbidden("BusinessPlan.Forbidden", "You do not have permission to delete this plan");
        var result = Result.Failure(error);

        _businessPlanServiceMock
            .Setup(x => x.DeleteBusinessPlanAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.DeleteBusinessPlan(planId, CancellationToken.None);

        // Assert
        var objectResult = response as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult!.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        objectResult.Value.Should().Be(error);
    }

    [Fact]
    public async Task DeleteBusinessPlan_ShouldCallServiceExactlyOnce()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var result = Result.Success();

        _businessPlanServiceMock
            .Setup(x => x.DeleteBusinessPlanAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        await _sut.DeleteBusinessPlan(planId, CancellationToken.None);

        // Assert
        _businessPlanServiceMock.Verify(
            x => x.DeleteBusinessPlanAsync(planId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region ArchiveBusinessPlan

    [Fact]
    public async Task ArchiveBusinessPlan_WithValidId_ShouldReturnOkResult()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var businessPlanResponse = _fixture.Create<BusinessPlanResponse>();
        var result = Result.Success(businessPlanResponse);

        _businessPlanServiceMock
            .Setup(x => x.ArchiveBusinessPlanAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.ArchiveBusinessPlan(planId, CancellationToken.None);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(businessPlanResponse);
    }

    [Fact]
    public async Task ArchiveBusinessPlan_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var error = Error.NotFound("BusinessPlan.NotFound", "Business plan not found");
        var result = Result.Failure<BusinessPlanResponse>(error);

        _businessPlanServiceMock
            .Setup(x => x.ArchiveBusinessPlanAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.ArchiveBusinessPlan(planId, CancellationToken.None);

        // Assert
        response.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = response as NotFoundObjectResult;
        notFoundResult!.Value.Should().Be(error);
    }

    #endregion

    #region UnarchiveBusinessPlan

    [Fact]
    public async Task UnarchiveBusinessPlan_WithValidId_ShouldReturnOkResult()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var businessPlanResponse = _fixture.Create<BusinessPlanResponse>();
        var result = Result.Success(businessPlanResponse);

        _businessPlanServiceMock
            .Setup(x => x.UnarchiveBusinessPlanAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.UnarchiveBusinessPlan(planId, CancellationToken.None);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(businessPlanResponse);
    }

    [Fact]
    public async Task UnarchiveBusinessPlan_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var error = Error.NotFound("BusinessPlan.NotFound", "Business plan not found");
        var result = Result.Failure<BusinessPlanResponse>(error);

        _businessPlanServiceMock
            .Setup(x => x.UnarchiveBusinessPlanAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.UnarchiveBusinessPlan(planId, CancellationToken.None);

        // Assert
        response.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = response as NotFoundObjectResult;
        notFoundResult!.Value.Should().Be(error);
    }

    #endregion

    #region DuplicateBusinessPlan

    [Fact]
    public async Task DuplicateBusinessPlan_WithValidId_ShouldReturnOkResult()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var request = new DuplicateBusinessPlanRequest { NewTitle = "Copied Plan" };
        var businessPlanResponse = _fixture.Create<BusinessPlanResponse>();
        var result = Result.Success(businessPlanResponse);

        _businessPlanServiceMock
            .Setup(x => x.DuplicateBusinessPlanAsync(planId, request.NewTitle, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.DuplicateBusinessPlan(planId, request, CancellationToken.None);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(businessPlanResponse);
    }

    [Fact]
    public async Task DuplicateBusinessPlan_WithNullRequest_ShouldReturnOkResult()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var businessPlanResponse = _fixture.Create<BusinessPlanResponse>();
        var result = Result.Success(businessPlanResponse);

        _businessPlanServiceMock
            .Setup(x => x.DuplicateBusinessPlanAsync(planId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.DuplicateBusinessPlan(planId, null, CancellationToken.None);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(businessPlanResponse);
    }

    [Fact]
    public async Task DuplicateBusinessPlan_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var request = new DuplicateBusinessPlanRequest { NewTitle = "Copied Plan" };
        var error = Error.NotFound("BusinessPlan.NotFound", "Business plan not found");
        var result = Result.Failure<BusinessPlanResponse>(error);

        _businessPlanServiceMock
            .Setup(x => x.DuplicateBusinessPlanAsync(planId, request.NewTitle, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.DuplicateBusinessPlan(planId, request, CancellationToken.None);

        // Assert
        response.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = response as NotFoundObjectResult;
        notFoundResult!.Value.Should().Be(error);
    }

    [Fact]
    public async Task DuplicateBusinessPlan_WhenForbidden_ShouldReturnForbidden()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var request = new DuplicateBusinessPlanRequest { NewTitle = "Copied Plan" };
        var error = Error.Forbidden("BusinessPlan.Forbidden", "You do not have access to duplicate this plan");
        var result = Result.Failure<BusinessPlanResponse>(error);

        _businessPlanServiceMock
            .Setup(x => x.DuplicateBusinessPlanAsync(planId, request.NewTitle, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.DuplicateBusinessPlan(planId, request, CancellationToken.None);

        // Assert
        var objectResult = response as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult!.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        objectResult.Value.Should().Be(error);
    }

    #endregion

    #region GetPlanTypes

    [Fact]
    public void GetPlanTypes_ShouldReturnOkWithThreePlanTypes()
    {
        // Act
        var response = _sut.GetPlanTypes();

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        var planTypes = okResult!.Value as List<PlanTypeDto>;
        planTypes.Should().NotBeNull();
        planTypes.Should().HaveCount(3);
    }

    [Fact]
    public void GetPlanTypes_ShouldContainBusinessPlanType()
    {
        // Act
        var response = _sut.GetPlanTypes();

        // Assert
        var okResult = response as OkObjectResult;
        var planTypes = okResult!.Value as List<PlanTypeDto>;
        planTypes.Should().Contain(pt => pt.Name == "BusinessPlan" && pt.Id == 0);
    }

    [Fact]
    public void GetPlanTypes_ShouldContainStrategicPlanType()
    {
        // Act
        var response = _sut.GetPlanTypes();

        // Assert
        var okResult = response as OkObjectResult;
        var planTypes = okResult!.Value as List<PlanTypeDto>;
        planTypes.Should().Contain(pt => pt.Name == "StrategicPlan" && pt.Id == 1);
    }

    [Fact]
    public void GetPlanTypes_ShouldContainLeanCanvasType()
    {
        // Act
        var response = _sut.GetPlanTypes();

        // Assert
        var okResult = response as OkObjectResult;
        var planTypes = okResult!.Value as List<PlanTypeDto>;
        planTypes.Should().Contain(pt => pt.Name == "LeanCanvas" && pt.Id == 2);
    }

    #endregion

    #region Unauthenticated Access

    [Fact]
    public async Task CreateBusinessPlan_WithoutAuthenticatedUser_ControllerShouldStillCallService()
    {
        // Arrange - Set up controller without authenticated claims
        var controller = new BusinessPlanController(
            _businessPlanServiceMock.Object,
            _strategyMapServiceMock.Object,
            _readinessScoreServiceMock.Object,
            _auditServiceMock.Object,
            _contextMock.Object,
            _loggerMock.Object);

        // Set up HttpContext without authenticated identity (no auth type)
        var identity = new ClaimsIdentity(); // No authentication type = unauthenticated
        var claimsPrincipal = new ClaimsPrincipal(identity);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        var request = new CreateBusinessPlanRequest
        {
            Title = "Test Plan",
            PlanType = "BusinessPlan",
            OrganizationId = Guid.NewGuid()
        };
        var error = Error.Unauthorized("User.Unauthorized", "User is not authenticated");
        var result = Result.Failure<BusinessPlanResponse>(error);

        _businessPlanServiceMock
            .Setup(x => x.CreateBusinessPlanAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await controller.CreateBusinessPlan(request, CancellationToken.None);

        // Assert - Service returns Unauthorized error which maps to UnauthorizedObjectResult
        response.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = response as UnauthorizedObjectResult;
        unauthorizedResult!.Value.Should().Be(error);
    }

    [Fact]
    public async Task GetBusinessPlan_WithoutAuthenticatedUser_NoClaimsIdentifier()
    {
        // Arrange - Set up controller without NameIdentifier claim
        var controller = new BusinessPlanController(
            _businessPlanServiceMock.Object,
            _strategyMapServiceMock.Object,
            _readinessScoreServiceMock.Object,
            _auditServiceMock.Object,
            _contextMock.Object,
            _loggerMock.Object);

        // No claims at all
        var identity = new ClaimsIdentity();
        var claimsPrincipal = new ClaimsPrincipal(identity);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        var planId = Guid.NewGuid();
        var error = Error.Unauthorized("User.Unauthorized", "User is not authenticated");
        var result = Result.Failure<BusinessPlanResponse>(error);

        _businessPlanServiceMock
            .Setup(x => x.GetBusinessPlanAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await controller.GetBusinessPlan(planId, sections: false, CancellationToken.None);

        // Assert
        response.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = response as UnauthorizedObjectResult;
        unauthorizedResult!.Value.Should().Be(error);
    }

    [Fact]
    public async Task DeleteBusinessPlan_WithoutAuthenticatedUser_ServiceReturnsUnauthorized()
    {
        // Arrange - Set up controller without valid claims
        var controller = new BusinessPlanController(
            _businessPlanServiceMock.Object,
            _strategyMapServiceMock.Object,
            _readinessScoreServiceMock.Object,
            _auditServiceMock.Object,
            _contextMock.Object,
            _loggerMock.Object);

        var httpContext = new Mock<HttpContext>();
        var user = new Mock<ClaimsPrincipal>();
        user.Setup(x => x.FindFirst(It.IsAny<string>())).Returns((Claim?)null);
        httpContext.Setup(x => x.User).Returns(user.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext.Object
        };

        var planId = Guid.NewGuid();
        var error = Error.Unauthorized("User.Unauthorized", "User is not authenticated");
        var result = Result.Failure(error);

        _businessPlanServiceMock
            .Setup(x => x.DeleteBusinessPlanAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await controller.DeleteBusinessPlan(planId, CancellationToken.None);

        // Assert
        response.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = response as UnauthorizedObjectResult;
        unauthorizedResult!.Value.Should().Be(error);
    }

    #endregion

    #region Error Mapping Verification

    [Fact]
    public async Task CreateBusinessPlan_WhenUnauthorizedError_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new CreateBusinessPlanRequest
        {
            Title = "Test Plan",
            PlanType = "BusinessPlan",
            OrganizationId = Guid.NewGuid()
        };
        var error = Error.Unauthorized("BusinessPlan.Unauthorized", "Not authorized to create plans");
        var result = Result.Failure<BusinessPlanResponse>(error);

        _businessPlanServiceMock
            .Setup(x => x.CreateBusinessPlanAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.CreateBusinessPlan(request, CancellationToken.None);

        // Assert
        response.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = response as UnauthorizedObjectResult;
        unauthorizedResult!.Value.Should().Be(error);
    }

    [Fact]
    public async Task UpdateBusinessPlan_WhenForbiddenError_ShouldReturn403()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var request = new UpdateBusinessPlanRequest { Title = "Updated" };
        var error = Error.Forbidden("BusinessPlan.Forbidden", "Not allowed to update this plan");
        var result = Result.Failure<BusinessPlanResponse>(error);

        _businessPlanServiceMock
            .Setup(x => x.UpdateBusinessPlanAsync(planId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.UpdateBusinessPlan(planId, request, CancellationToken.None);

        // Assert
        var objectResult = response as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult!.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        objectResult.Value.Should().Be(error);
    }

    #endregion
}

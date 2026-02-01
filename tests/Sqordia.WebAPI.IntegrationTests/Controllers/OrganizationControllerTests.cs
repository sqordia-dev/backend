using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Services;
using Sqordia.Contracts.Requests.Organization;
using Sqordia.Contracts.Responses.Organization;
using WebAPI.Controllers;
using Xunit;

namespace Sqordia.WebAPI.IntegrationTests.Controllers;

public class OrganizationControllerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IOrganizationService> _organizationServiceMock;
    private readonly OrganizationController _sut;

    public OrganizationControllerTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _organizationServiceMock = new Mock<IOrganizationService>();

        _sut = new OrganizationController(_organizationServiceMock.Object);
    }

    #region CreateOrganization

    [Fact]
    public async Task CreateOrganization_WithValidData_ShouldReturnOkResult()
    {
        // Arrange
        var request = _fixture.Create<CreateOrganizationRequest>();
        var organizationResponse = _fixture.Create<OrganizationResponse>();
        var result = Result.Success(organizationResponse);

        _organizationServiceMock.Setup(x => x.CreateOrganizationAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.CreateOrganization(request);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(organizationResponse);
    }

    [Fact]
    public async Task CreateOrganization_WithConflictingName_ShouldReturnConflict()
    {
        // Arrange
        var request = _fixture.Create<CreateOrganizationRequest>();
        var error = Error.Conflict("Organization.Conflict", "Organization with this name already exists");
        var result = Result.Failure<OrganizationResponse>(error);

        _organizationServiceMock.Setup(x => x.CreateOrganizationAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.CreateOrganization(request);

        // Assert
        response.Should().BeOfType<ConflictObjectResult>();
        var conflictResult = response as ConflictObjectResult;
        conflictResult!.Value.Should().Be(error);
    }

    [Fact]
    public async Task CreateOrganization_WithValidationError_ShouldReturnBadRequest()
    {
        // Arrange
        var request = _fixture.Create<CreateOrganizationRequest>();
        var error = Error.Validation("Organization.Validation", "Name is required");
        var result = Result.Failure<OrganizationResponse>(error);

        _organizationServiceMock.Setup(x => x.CreateOrganizationAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.CreateOrganization(request);

        // Assert
        response.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = response as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be(error);
    }

    [Fact]
    public async Task CreateOrganization_ShouldCallServiceExactlyOnce()
    {
        // Arrange
        var request = _fixture.Create<CreateOrganizationRequest>();
        var organizationResponse = _fixture.Create<OrganizationResponse>();
        var result = Result.Success(organizationResponse);

        _organizationServiceMock.Setup(x => x.CreateOrganizationAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        await _sut.CreateOrganization(request);

        // Assert
        _organizationServiceMock.Verify(
            x => x.CreateOrganizationAsync(request, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetUserOrganizations

    [Fact]
    public async Task GetUserOrganizations_ShouldReturnOkWithOrganizations()
    {
        // Arrange
        var organizations = _fixture.CreateMany<OrganizationResponse>(3);
        var result = Result.Success<IEnumerable<OrganizationResponse>>(organizations);

        _organizationServiceMock.Setup(x => x.GetUserOrganizationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.GetUserOrganizations();

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().BeAssignableTo<IEnumerable<OrganizationResponse>>();
        var returnedOrganizations = okResult.Value as IEnumerable<OrganizationResponse>;
        returnedOrganizations.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetUserOrganizations_WhenNoOrganizations_ShouldReturnOkWithEmptyList()
    {
        // Arrange
        var organizations = Enumerable.Empty<OrganizationResponse>();
        var result = Result.Success<IEnumerable<OrganizationResponse>>(organizations);

        _organizationServiceMock.Setup(x => x.GetUserOrganizationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.GetUserOrganizations();

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        var returnedOrganizations = okResult!.Value as IEnumerable<OrganizationResponse>;
        returnedOrganizations.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserOrganizations_WhenUnauthorized_ShouldReturnUnauthorized()
    {
        // Arrange
        var error = Error.Unauthorized("Organization.Unauthorized", "User is not authenticated");
        var result = Result.Failure<IEnumerable<OrganizationResponse>>(error);

        _organizationServiceMock.Setup(x => x.GetUserOrganizationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.GetUserOrganizations();

        // Assert
        response.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = response as UnauthorizedObjectResult;
        unauthorizedResult!.Value.Should().Be(error);
    }

    #endregion

    #region GetOrganization

    [Fact]
    public async Task GetOrganization_WithValidId_ShouldReturnOkResult()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var organizationResponse = _fixture.Create<OrganizationResponse>();
        var result = Result.Success(organizationResponse);

        _organizationServiceMock.Setup(x => x.GetOrganizationAsync(organizationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.GetOrganization(organizationId);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(organizationResponse);
    }

    [Fact]
    public async Task GetOrganization_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var error = Error.NotFound("Organization.NotFound", "Organization not found");
        var result = Result.Failure<OrganizationResponse>(error);

        _organizationServiceMock.Setup(x => x.GetOrganizationAsync(organizationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.GetOrganization(organizationId);

        // Assert
        response.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = response as NotFoundObjectResult;
        notFoundResult!.Value.Should().Be(error);
    }

    [Fact]
    public async Task GetOrganization_WhenForbidden_ShouldReturnForbidden()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var error = Error.Forbidden("Organization.Forbidden", "User does not have access to this organization");
        var result = Result.Failure<OrganizationResponse>(error);

        _organizationServiceMock.Setup(x => x.GetOrganizationAsync(organizationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.GetOrganization(organizationId);

        // Assert
        var objectResult = response as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult!.StatusCode.Should().Be(403);
        objectResult.Value.Should().Be(error);
    }

    #endregion

    #region GetOrganizationDetail

    [Fact]
    public async Task GetOrganizationDetail_WithValidId_ShouldReturnOkResult()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var detailResponse = _fixture.Create<OrganizationDetailResponse>();
        var result = Result.Success(detailResponse);

        _organizationServiceMock.Setup(x => x.GetOrganizationDetailAsync(organizationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.GetOrganizationDetail(organizationId);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(detailResponse);
    }

    [Fact]
    public async Task GetOrganizationDetail_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var error = Error.NotFound("Organization.NotFound", "Organization not found");
        var result = Result.Failure<OrganizationDetailResponse>(error);

        _organizationServiceMock.Setup(x => x.GetOrganizationDetailAsync(organizationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.GetOrganizationDetail(organizationId);

        // Assert
        response.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = response as NotFoundObjectResult;
        notFoundResult!.Value.Should().Be(error);
    }

    #endregion

    #region UpdateOrganization

    [Fact]
    public async Task UpdateOrganization_WithValidData_ShouldReturnOkResult()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var request = _fixture.Create<UpdateOrganizationRequest>();
        var organizationResponse = _fixture.Create<OrganizationResponse>();
        var result = Result.Success(organizationResponse);

        _organizationServiceMock.Setup(x => x.UpdateOrganizationAsync(organizationId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.UpdateOrganization(organizationId, request);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(organizationResponse);
    }

    [Fact]
    public async Task UpdateOrganization_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var request = _fixture.Create<UpdateOrganizationRequest>();
        var error = Error.NotFound("Organization.NotFound", "Organization not found");
        var result = Result.Failure<OrganizationResponse>(error);

        _organizationServiceMock.Setup(x => x.UpdateOrganizationAsync(organizationId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.UpdateOrganization(organizationId, request);

        // Assert
        response.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = response as NotFoundObjectResult;
        notFoundResult!.Value.Should().Be(error);
    }

    [Fact]
    public async Task UpdateOrganization_WhenForbidden_ShouldReturnForbidden()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var request = _fixture.Create<UpdateOrganizationRequest>();
        var error = Error.Forbidden("Organization.Forbidden", "User is not allowed to update this organization");
        var result = Result.Failure<OrganizationResponse>(error);

        _organizationServiceMock.Setup(x => x.UpdateOrganizationAsync(organizationId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.UpdateOrganization(organizationId, request);

        // Assert
        var objectResult = response as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult!.StatusCode.Should().Be(403);
        objectResult.Value.Should().Be(error);
    }

    [Fact]
    public async Task UpdateOrganization_ShouldCallServiceWithCorrectParameters()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var request = _fixture.Create<UpdateOrganizationRequest>();
        var organizationResponse = _fixture.Create<OrganizationResponse>();
        var result = Result.Success(organizationResponse);

        _organizationServiceMock.Setup(x => x.UpdateOrganizationAsync(organizationId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        await _sut.UpdateOrganization(organizationId, request);

        // Assert
        _organizationServiceMock.Verify(
            x => x.UpdateOrganizationAsync(organizationId, request, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region DeleteOrganization

    [Fact]
    public async Task DeleteOrganization_WithValidId_ShouldReturnOkResult()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var result = Result.Success();

        _organizationServiceMock.Setup(x => x.DeleteOrganizationAsync(organizationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.DeleteOrganization(organizationId);

        // Assert
        response.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task DeleteOrganization_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var error = Error.NotFound("Organization.NotFound", "Organization not found");
        var result = Result.Failure(error);

        _organizationServiceMock.Setup(x => x.DeleteOrganizationAsync(organizationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.DeleteOrganization(organizationId);

        // Assert
        response.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = response as NotFoundObjectResult;
        notFoundResult!.Value.Should().Be(error);
    }

    #endregion

    #region DeactivateOrganization

    [Fact]
    public async Task DeactivateOrganization_WithValidId_ShouldReturnOkResult()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var result = Result.Success();

        _organizationServiceMock.Setup(x => x.DeactivateOrganizationAsync(organizationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.DeactivateOrganization(organizationId);

        // Assert
        response.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task DeactivateOrganization_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var error = Error.NotFound("Organization.NotFound", "Organization not found");
        var result = Result.Failure(error);

        _organizationServiceMock.Setup(x => x.DeactivateOrganizationAsync(organizationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.DeactivateOrganization(organizationId);

        // Assert
        response.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = response as NotFoundObjectResult;
        notFoundResult!.Value.Should().Be(error);
    }

    #endregion

    #region ReactivateOrganization

    [Fact]
    public async Task ReactivateOrganization_WithValidId_ShouldReturnOkResult()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var result = Result.Success();

        _organizationServiceMock.Setup(x => x.ReactivateOrganizationAsync(organizationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.ReactivateOrganization(organizationId);

        // Assert
        response.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task ReactivateOrganization_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var error = Error.NotFound("Organization.NotFound", "Organization not found");
        var result = Result.Failure(error);

        _organizationServiceMock.Setup(x => x.ReactivateOrganizationAsync(organizationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.ReactivateOrganization(organizationId);

        // Assert
        response.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = response as NotFoundObjectResult;
        notFoundResult!.Value.Should().Be(error);
    }

    #endregion

    #region UpdateOrganizationSettings

    [Fact]
    public async Task UpdateOrganizationSettings_WithValidData_ShouldReturnOkResult()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var request = _fixture.Create<UpdateOrganizationSettingsRequest>();
        var organizationResponse = _fixture.Create<OrganizationResponse>();
        var result = Result.Success(organizationResponse);

        _organizationServiceMock.Setup(x => x.UpdateOrganizationSettingsAsync(organizationId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.UpdateOrganizationSettings(organizationId, request);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(organizationResponse);
    }

    [Fact]
    public async Task UpdateOrganizationSettings_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var request = _fixture.Create<UpdateOrganizationSettingsRequest>();
        var error = Error.NotFound("Organization.NotFound", "Organization not found");
        var result = Result.Failure<OrganizationResponse>(error);

        _organizationServiceMock.Setup(x => x.UpdateOrganizationSettingsAsync(organizationId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.UpdateOrganizationSettings(organizationId, request);

        // Assert
        response.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = response as NotFoundObjectResult;
        notFoundResult!.Value.Should().Be(error);
    }

    #endregion

    #region GetMembers

    [Fact]
    public async Task GetMembers_WithValidOrganizationId_ShouldReturnOkWithMembers()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var members = _fixture.CreateMany<OrganizationMemberResponse>(3);
        var result = Result.Success<IEnumerable<OrganizationMemberResponse>>(members);

        _organizationServiceMock.Setup(x => x.GetMembersAsync(organizationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.GetMembers(organizationId);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().BeAssignableTo<IEnumerable<OrganizationMemberResponse>>();
        var returnedMembers = okResult.Value as IEnumerable<OrganizationMemberResponse>;
        returnedMembers.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetMembers_WithNonExistentOrganization_ShouldReturnNotFound()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var error = Error.NotFound("Organization.NotFound", "Organization not found");
        var result = Result.Failure<IEnumerable<OrganizationMemberResponse>>(error);

        _organizationServiceMock.Setup(x => x.GetMembersAsync(organizationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.GetMembers(organizationId);

        // Assert
        response.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = response as NotFoundObjectResult;
        notFoundResult!.Value.Should().Be(error);
    }

    #endregion

    #region AddMember

    [Fact]
    public async Task AddMember_WithValidData_ShouldReturnOkResult()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var request = _fixture.Create<AddOrganizationMemberRequest>();
        var memberResponse = _fixture.Create<OrganizationMemberResponse>();
        var result = Result.Success(memberResponse);

        _organizationServiceMock.Setup(x => x.AddMemberAsync(organizationId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.AddMember(organizationId, request);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(memberResponse);
    }

    [Fact]
    public async Task AddMember_WhenOrganizationNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var request = _fixture.Create<AddOrganizationMemberRequest>();
        var error = Error.NotFound("Organization.NotFound", "Organization not found");
        var result = Result.Failure<OrganizationMemberResponse>(error);

        _organizationServiceMock.Setup(x => x.AddMemberAsync(organizationId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.AddMember(organizationId, request);

        // Assert
        response.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = response as NotFoundObjectResult;
        notFoundResult!.Value.Should().Be(error);
    }

    [Fact]
    public async Task AddMember_WhenMemberAlreadyExists_ShouldReturnConflict()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var request = _fixture.Create<AddOrganizationMemberRequest>();
        var error = Error.Conflict("Organization.MemberConflict", "User is already a member of this organization");
        var result = Result.Failure<OrganizationMemberResponse>(error);

        _organizationServiceMock.Setup(x => x.AddMemberAsync(organizationId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.AddMember(organizationId, request);

        // Assert
        response.Should().BeOfType<ConflictObjectResult>();
        var conflictResult = response as ConflictObjectResult;
        conflictResult!.Value.Should().Be(error);
    }

    [Fact]
    public async Task AddMember_WhenUnauthorized_ShouldReturnUnauthorized()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var request = _fixture.Create<AddOrganizationMemberRequest>();
        var error = Error.Unauthorized("Organization.Unauthorized", "User is not authorized to add members");
        var result = Result.Failure<OrganizationMemberResponse>(error);

        _organizationServiceMock.Setup(x => x.AddMemberAsync(organizationId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.AddMember(organizationId, request);

        // Assert
        response.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = response as UnauthorizedObjectResult;
        unauthorizedResult!.Value.Should().Be(error);
    }

    [Fact]
    public async Task AddMember_ShouldCallServiceWithCorrectParameters()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var request = _fixture.Create<AddOrganizationMemberRequest>();
        var memberResponse = _fixture.Create<OrganizationMemberResponse>();
        var result = Result.Success(memberResponse);

        _organizationServiceMock.Setup(x => x.AddMemberAsync(organizationId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        await _sut.AddMember(organizationId, request);

        // Assert
        _organizationServiceMock.Verify(
            x => x.AddMemberAsync(organizationId, request, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region UpdateMemberRole

    [Fact]
    public async Task UpdateMemberRole_WithValidData_ShouldReturnOkResult()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var request = _fixture.Create<UpdateMemberRoleRequest>();
        var memberResponse = _fixture.Create<OrganizationMemberResponse>();
        var result = Result.Success(memberResponse);

        _organizationServiceMock.Setup(x => x.UpdateMemberRoleAsync(organizationId, memberId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.UpdateMemberRole(organizationId, memberId, request);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(memberResponse);
    }

    [Fact]
    public async Task UpdateMemberRole_WhenMemberNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var request = _fixture.Create<UpdateMemberRoleRequest>();
        var error = Error.NotFound("Organization.MemberNotFound", "Member not found in this organization");
        var result = Result.Failure<OrganizationMemberResponse>(error);

        _organizationServiceMock.Setup(x => x.UpdateMemberRoleAsync(organizationId, memberId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.UpdateMemberRole(organizationId, memberId, request);

        // Assert
        response.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = response as NotFoundObjectResult;
        notFoundResult!.Value.Should().Be(error);
    }

    #endregion

    #region RemoveMember

    [Fact]
    public async Task RemoveMember_WithValidIds_ShouldReturnOkResult()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var result = Result.Success();

        _organizationServiceMock.Setup(x => x.RemoveMemberAsync(organizationId, memberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.RemoveMember(organizationId, memberId);

        // Assert
        response.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task RemoveMember_WhenMemberNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var error = Error.NotFound("Organization.MemberNotFound", "Member not found in this organization");
        var result = Result.Failure(error);

        _organizationServiceMock.Setup(x => x.RemoveMemberAsync(organizationId, memberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.RemoveMember(organizationId, memberId);

        // Assert
        response.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = response as NotFoundObjectResult;
        notFoundResult!.Value.Should().Be(error);
    }

    [Fact]
    public async Task RemoveMember_WhenForbidden_ShouldReturnForbidden()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var error = Error.Forbidden("Organization.Forbidden", "User is not allowed to remove members");
        var result = Result.Failure(error);

        _organizationServiceMock.Setup(x => x.RemoveMemberAsync(organizationId, memberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.RemoveMember(organizationId, memberId);

        // Assert
        var objectResult = response as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult!.StatusCode.Should().Be(403);
        objectResult.Value.Should().Be(error);
    }

    [Fact]
    public async Task RemoveMember_ShouldCallServiceWithCorrectParameters()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var result = Result.Success();

        _organizationServiceMock.Setup(x => x.RemoveMemberAsync(organizationId, memberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        await _sut.RemoveMember(organizationId, memberId);

        // Assert
        _organizationServiceMock.Verify(
            x => x.RemoveMemberAsync(organizationId, memberId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion
}

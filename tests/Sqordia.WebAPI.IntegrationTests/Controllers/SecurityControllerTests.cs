using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Services;
using Sqordia.Contracts.Responses.Security;
using WebAPI.Controllers;
using System.Security.Claims;
using Xunit;

namespace Sqordia.WebAPI.IntegrationTests.Controllers;

public class SecurityControllerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ISecurityManagementService> _securityManagementServiceMock;
    private readonly SecurityController _sut;

    public SecurityControllerTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _securityManagementServiceMock = new Mock<ISecurityManagementService>();

        _sut = new SecurityController(_securityManagementServiceMock.Object);
    }

    private void SetupAuthenticatedUser(Guid userId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    private void SetupAuthenticatedUserWithSessionToken(Guid userId, string sessionToken)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = claimsPrincipal };
        httpContext.Request.Headers["X-Session-Token"] = sessionToken;

        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    private void SetupUnauthenticatedUser()
    {
        var httpContext = new Mock<HttpContext>();
        var user = new Mock<ClaimsPrincipal>();
        user.Setup(x => x.FindFirst(It.IsAny<string>())).Returns((Claim?)null);
        httpContext.Setup(x => x.User).Returns(user.Object);
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext.Object
        };
    }

    // ========== GetActiveSessions ==========

    [Fact]
    public async Task GetActiveSessions_WithValidUser_ShouldReturnOkResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessions = _fixture.CreateMany<ActiveSessionResponse>(3).ToList();
        var result = Result.Success(sessions);

        _securityManagementServiceMock.Setup(x => x.GetActiveSessionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        SetupAuthenticatedUser(userId);

        // Act
        var response = await _sut.GetActiveSessions(CancellationToken.None);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(sessions);
    }

    [Fact]
    public async Task GetActiveSessions_WhenServiceFails_ShouldReturnBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var error = Error.Failure("Security.Sessions.Failed", "Failed to retrieve sessions");
        var result = Result.Failure<List<ActiveSessionResponse>>(error);

        _securityManagementServiceMock.Setup(x => x.GetActiveSessionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        SetupAuthenticatedUser(userId);

        // Act
        var response = await _sut.GetActiveSessions(CancellationToken.None);

        // Assert
        response.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = response as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be(error);
    }

    [Fact]
    public async Task GetActiveSessions_WithoutValidClaims_ShouldReturnUnauthorized()
    {
        // Arrange
        SetupUnauthenticatedUser();

        // Act
        var response = await _sut.GetActiveSessions(CancellationToken.None);

        // Assert
        response.Should().BeOfType<UnauthorizedResult>();
    }

    // ========== RevokeSession ==========

    [Fact]
    public async Task RevokeSession_WithValidRequest_ShouldReturnOkResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var result = Result.Success();

        _securityManagementServiceMock.Setup(x => x.RevokeSessionAsync(userId, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        SetupAuthenticatedUser(userId);

        // Act
        var response = await _sut.RevokeSession(sessionId, CancellationToken.None);

        // Assert
        response.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task RevokeSession_WhenSessionNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var error = Error.NotFound("Security.Session.NotFound", "Session not found");
        var result = Result.Failure(error);

        _securityManagementServiceMock.Setup(x => x.RevokeSessionAsync(userId, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        SetupAuthenticatedUser(userId);

        // Act
        var response = await _sut.RevokeSession(sessionId, CancellationToken.None);

        // Assert
        response.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = response as NotFoundObjectResult;
        notFoundResult!.Value.Should().Be(error);
    }

    [Fact]
    public async Task RevokeSession_WhenServiceFails_ShouldReturnBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var error = Error.Failure("Security.Session.RevokeFailed", "Failed to revoke session");
        var result = Result.Failure(error);

        _securityManagementServiceMock.Setup(x => x.RevokeSessionAsync(userId, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        SetupAuthenticatedUser(userId);

        // Act
        var response = await _sut.RevokeSession(sessionId, CancellationToken.None);

        // Assert
        response.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = response as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be(error);
    }

    [Fact]
    public async Task RevokeSession_WithoutValidClaims_ShouldReturnUnauthorized()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        SetupUnauthenticatedUser();

        // Act
        var response = await _sut.RevokeSession(sessionId, CancellationToken.None);

        // Assert
        response.Should().BeOfType<UnauthorizedResult>();
    }

    // ========== RevokeAllOtherSessions ==========

    [Fact]
    public async Task RevokeAllOtherSessions_WithValidRequest_ShouldReturnOkResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionToken = "test-session-token-12345";
        var result = Result.Success();

        _securityManagementServiceMock.Setup(x => x.RevokeAllSessionsExceptCurrentAsync(
                userId, sessionToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        SetupAuthenticatedUserWithSessionToken(userId, sessionToken);

        // Act
        var response = await _sut.RevokeAllOtherSessions(CancellationToken.None);

        // Assert
        response.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task RevokeAllOtherSessions_WhenServiceFails_ShouldReturnBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionToken = "test-session-token-12345";
        var error = Error.Failure("Security.Sessions.RevokeOthersFailed", "Failed to revoke other sessions");
        var result = Result.Failure(error);

        _securityManagementServiceMock.Setup(x => x.RevokeAllSessionsExceptCurrentAsync(
                userId, sessionToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        SetupAuthenticatedUserWithSessionToken(userId, sessionToken);

        // Act
        var response = await _sut.RevokeAllOtherSessions(CancellationToken.None);

        // Assert
        response.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = response as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be(error);
    }

    [Fact]
    public async Task RevokeAllOtherSessions_WithoutValidClaims_ShouldReturnUnauthorized()
    {
        // Arrange
        SetupUnauthenticatedUser();

        // Act
        var response = await _sut.RevokeAllOtherSessions(CancellationToken.None);

        // Assert
        response.Should().BeOfType<UnauthorizedResult>();
    }

    // ========== RevokeAllSessions ==========

    [Fact]
    public async Task RevokeAllSessions_WithValidUser_ShouldReturnOkResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var result = Result.Success();

        _securityManagementServiceMock.Setup(x => x.RevokeAllSessionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        SetupAuthenticatedUser(userId);

        // Act
        var response = await _sut.RevokeAllSessions(CancellationToken.None);

        // Assert
        response.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task RevokeAllSessions_WhenServiceFails_ShouldReturnBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var error = Error.Failure("Security.Sessions.RevokeAllFailed", "Failed to revoke all sessions");
        var result = Result.Failure(error);

        _securityManagementServiceMock.Setup(x => x.RevokeAllSessionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        SetupAuthenticatedUser(userId);

        // Act
        var response = await _sut.RevokeAllSessions(CancellationToken.None);

        // Assert
        response.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = response as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be(error);
    }

    [Fact]
    public async Task RevokeAllSessions_WithoutValidClaims_ShouldReturnUnauthorized()
    {
        // Arrange
        SetupUnauthenticatedUser();

        // Act
        var response = await _sut.RevokeAllSessions(CancellationToken.None);

        // Assert
        response.Should().BeOfType<UnauthorizedResult>();
    }

    // ========== GetLoginHistory ==========

    [Fact]
    public async Task GetLoginHistory_WithValidUser_ShouldReturnOkResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var loginHistory = _fixture.CreateMany<LoginHistoryResponse>(5).ToList();
        var result = Result.Success(loginHistory);

        _securityManagementServiceMock.Setup(x => x.GetLoginHistoryAsync(
                userId, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        SetupAuthenticatedUser(userId);

        // Act
        var response = await _sut.GetLoginHistory(1, 20, CancellationToken.None);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(loginHistory);
    }

    [Fact]
    public async Task GetLoginHistory_WithCustomPagination_ShouldReturnOkResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var pageNumber = 2;
        var pageSize = 10;
        var loginHistory = _fixture.CreateMany<LoginHistoryResponse>(10).ToList();
        var result = Result.Success(loginHistory);

        _securityManagementServiceMock.Setup(x => x.GetLoginHistoryAsync(
                userId, pageNumber, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        SetupAuthenticatedUser(userId);

        // Act
        var response = await _sut.GetLoginHistory(pageNumber, pageSize, CancellationToken.None);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(loginHistory);
    }

    [Fact]
    public async Task GetLoginHistory_WhenServiceFails_ShouldReturnBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var error = Error.Failure("Security.LoginHistory.Failed", "Failed to retrieve login history");
        var result = Result.Failure<List<LoginHistoryResponse>>(error);

        _securityManagementServiceMock.Setup(x => x.GetLoginHistoryAsync(
                userId, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        SetupAuthenticatedUser(userId);

        // Act
        var response = await _sut.GetLoginHistory(1, 20, CancellationToken.None);

        // Assert
        response.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = response as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be(error);
    }

    [Fact]
    public async Task GetLoginHistory_WithoutValidClaims_ShouldReturnUnauthorized()
    {
        // Arrange
        SetupUnauthenticatedUser();

        // Act
        var response = await _sut.GetLoginHistory(1, 20, CancellationToken.None);

        // Assert
        response.Should().BeOfType<UnauthorizedResult>();
    }

    // ========== UnlockAccount ==========

    [Fact]
    public async Task UnlockAccount_WithValidUserId_ShouldReturnOkResult()
    {
        // Arrange
        var targetUserId = Guid.NewGuid();
        var result = Result.Success();

        _securityManagementServiceMock.Setup(x => x.UnlockAccountAsync(targetUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.UnlockAccount(targetUserId, CancellationToken.None);

        // Assert
        response.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task UnlockAccount_WhenUserNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var targetUserId = Guid.NewGuid();
        var error = Error.NotFound("Security.Account.NotFound", "User account not found");
        var result = Result.Failure(error);

        _securityManagementServiceMock.Setup(x => x.UnlockAccountAsync(targetUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.UnlockAccount(targetUserId, CancellationToken.None);

        // Assert
        response.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = response as NotFoundObjectResult;
        notFoundResult!.Value.Should().Be(error);
    }

    // ========== ForcePasswordChange ==========

    [Fact]
    public async Task ForcePasswordChange_WithValidUserId_ShouldReturnOkResult()
    {
        // Arrange
        var targetUserId = Guid.NewGuid();
        var result = Result.Success();

        _securityManagementServiceMock.Setup(x => x.ForcePasswordChangeAsync(targetUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.ForcePasswordChange(targetUserId, CancellationToken.None);

        // Assert
        response.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task ForcePasswordChange_WhenUserNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var targetUserId = Guid.NewGuid();
        var error = Error.NotFound("Security.Account.NotFound", "User account not found");
        var result = Result.Failure(error);

        _securityManagementServiceMock.Setup(x => x.ForcePasswordChangeAsync(targetUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.ForcePasswordChange(targetUserId, CancellationToken.None);

        // Assert
        response.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = response as NotFoundObjectResult;
        notFoundResult!.Value.Should().Be(error);
    }

    [Fact]
    public async Task ForcePasswordChange_WhenServiceFails_ShouldReturnBadRequest()
    {
        // Arrange
        var targetUserId = Guid.NewGuid();
        var error = Error.Failure("Security.PasswordChange.Failed", "Failed to force password change");
        var result = Result.Failure(error);

        _securityManagementServiceMock.Setup(x => x.ForcePasswordChangeAsync(targetUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.ForcePasswordChange(targetUserId, CancellationToken.None);

        // Assert
        response.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = response as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be(error);
    }

    // ========== Service Verification ==========

    [Fact]
    public async Task GetActiveSessions_ShouldCallServiceWithCorrectUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessions = _fixture.CreateMany<ActiveSessionResponse>(1).ToList();

        _securityManagementServiceMock.Setup(x => x.GetActiveSessionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(sessions));

        SetupAuthenticatedUser(userId);

        // Act
        await _sut.GetActiveSessions(CancellationToken.None);

        // Assert
        _securityManagementServiceMock.Verify(
            x => x.GetActiveSessionsAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RevokeSession_ShouldCallServiceWithCorrectParameters()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();

        _securityManagementServiceMock.Setup(x => x.RevokeSessionAsync(userId, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        SetupAuthenticatedUser(userId);

        // Act
        await _sut.RevokeSession(sessionId, CancellationToken.None);

        // Assert
        _securityManagementServiceMock.Verify(
            x => x.RevokeSessionAsync(userId, sessionId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RevokeAllOtherSessions_ShouldPassSessionTokenFromHeader()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionToken = "my-current-session-token";

        _securityManagementServiceMock.Setup(x => x.RevokeAllSessionsExceptCurrentAsync(
                userId, sessionToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        SetupAuthenticatedUserWithSessionToken(userId, sessionToken);

        // Act
        await _sut.RevokeAllOtherSessions(CancellationToken.None);

        // Assert
        _securityManagementServiceMock.Verify(
            x => x.RevokeAllSessionsExceptCurrentAsync(userId, sessionToken, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetLoginHistory_ShouldCallServiceWithCorrectPaginationParameters()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var pageNumber = 3;
        var pageSize = 15;
        var loginHistory = _fixture.CreateMany<LoginHistoryResponse>(15).ToList();

        _securityManagementServiceMock.Setup(x => x.GetLoginHistoryAsync(
                userId, pageNumber, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(loginHistory));

        SetupAuthenticatedUser(userId);

        // Act
        await _sut.GetLoginHistory(pageNumber, pageSize, CancellationToken.None);

        // Assert
        _securityManagementServiceMock.Verify(
            x => x.GetLoginHistoryAsync(userId, pageNumber, pageSize, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

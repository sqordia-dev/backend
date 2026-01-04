using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Services;
using Sqordia.Contracts.Requests.Auth;
using Sqordia.Contracts.Responses.Auth;
using WebAPI.Controllers;
using System.Security.Claims;
using Xunit;

namespace Sqordia.WebAPI.IntegrationTests.Controllers;

public class AuthControllerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IAuthenticationService> _authenticationServiceMock;
    private readonly Mock<ILogger<AuthController>> _loggerMock;
    private readonly AuthController _sut;

    public AuthControllerTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _authenticationServiceMock = new Mock<IAuthenticationService>();
        _loggerMock = new Mock<ILogger<AuthController>>();

        _sut = new AuthController(_authenticationServiceMock.Object);
    }

    [Fact]
    public async Task Register_WithValidRequest_ShouldReturnOkResult()
    {
        // Arrange
        var request = _fixture.Create<RegisterRequest>();
        var authResponse = _fixture.Create<AuthResponse>();
        var result = Result.Success(authResponse);

        _authenticationServiceMock.Setup(x => x.RegisterAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.Register(request);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(authResponse);
    }

    [Fact]
    public async Task Register_WithInvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var request = _fixture.Create<RegisterRequest>();
        var error = Error.Conflict("User.AlreadyExists", "User already exists");
        var result = Result.Failure<AuthResponse>(error);

        _authenticationServiceMock.Setup(x => x.RegisterAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.Register(request);

        // Assert
        response.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = response as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be(error);
    }

    [Fact]
    public async Task Login_WithValidRequest_ShouldReturnOkResult()
    {
        // Arrange
        var request = _fixture.Create<LoginRequest>();
        var authResponse = _fixture.Create<AuthResponse>();
        var result = Result.Success(authResponse);

        _authenticationServiceMock.Setup(x => x.LoginAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.Login(request);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(authResponse);
    }

    [Fact]
    public async Task Login_WithInvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var request = _fixture.Create<LoginRequest>();
        var error = Error.Unauthorized("Authentication.InvalidCredentials", "Invalid credentials");
        var result = Result.Failure<AuthResponse>(error);

        _authenticationServiceMock.Setup(x => x.LoginAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.Login(request);

        // Assert
        response.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = response as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be(error);
    }

    [Fact]
    public async Task RefreshToken_WithValidRequest_ShouldReturnOkResult()
    {
        // Arrange
        var request = _fixture.Create<RefreshTokenRequest>();
        var authResponse = _fixture.Create<AuthResponse>();
        var result = Result.Success(authResponse);

        _authenticationServiceMock.Setup(x => x.RefreshTokenAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.RefreshToken(request);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(authResponse);
    }

    [Fact]
    public async Task RefreshToken_WithInvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var request = _fixture.Create<RefreshTokenRequest>();
        var error = Error.Unauthorized("Authentication.InvalidRefreshToken", "Invalid refresh token");
        var result = Result.Failure<AuthResponse>(error);

        _authenticationServiceMock.Setup(x => x.RefreshTokenAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.RefreshToken(request);

        // Assert
        response.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = response as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be(error);
    }

    [Fact]
    public async Task Logout_WithValidRequest_ShouldReturnOkResult()
    {
        // Arrange
        var request = _fixture.Create<LogoutRequest>();
        var result = Result.Success();

        _authenticationServiceMock.Setup(x => x.LogoutAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.Logout(request);

        // Assert
        response.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Logout_WithInvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var request = _fixture.Create<LogoutRequest>();
        var error = Error.Failure("Authentication.Logout.Failed", "Logout failed");
        var result = Result.Failure(error);

        _authenticationServiceMock.Setup(x => x.LogoutAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.Logout(request);

        // Assert
        response.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = response as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be(error);
    }

    [Fact]
    public async Task ForgotPassword_WithValidRequest_ShouldReturnOkResult()
    {
        // Arrange
        var request = _fixture.Create<ForgotPasswordRequest>();
        var result = Result.Success();

        _authenticationServiceMock.Setup(x => x.ForgotPasswordAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.ForgotPassword(request);

        // Assert
        response.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task ForgotPassword_WithInvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var request = _fixture.Create<ForgotPasswordRequest>();
        var error = Error.Failure("Authentication.PasswordReset.Failed", "Password reset failed");
        var result = Result.Failure(error);

        _authenticationServiceMock.Setup(x => x.ForgotPasswordAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.ForgotPassword(request);

        // Assert
        response.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = response as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be(error);
    }

    [Fact]
    public async Task ResetPassword_WithValidRequest_ShouldReturnOkResult()
    {
        // Arrange
        var request = _fixture.Create<ResetPasswordRequest>();
        var result = Result.Success();

        _authenticationServiceMock.Setup(x => x.ResetPasswordAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.ResetPassword(request);

        // Assert
        response.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task ResetPassword_WithInvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var request = _fixture.Create<ResetPasswordRequest>();
        var error = Error.Unauthorized("Authentication.InvalidResetToken", "Invalid reset token");
        var result = Result.Failure(error);

        _authenticationServiceMock.Setup(x => x.ResetPasswordAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.ResetPassword(request);

        // Assert
        response.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = response as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be(error);
    }

    [Fact]
    public async Task VerifyEmail_WithValidRequest_ShouldReturnOkResult()
    {
        // Arrange
        var request = _fixture.Create<VerifyEmailRequest>();
        var result = Result.Success();

        _authenticationServiceMock.Setup(x => x.VerifyEmailAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.VerifyEmail(request);

        // Assert
        response.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task VerifyEmail_WithInvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var request = _fixture.Create<VerifyEmailRequest>();
        var error = Error.Unauthorized("Authentication.InvalidVerificationToken", "Invalid verification token");
        var result = Result.Failure(error);

        _authenticationServiceMock.Setup(x => x.VerifyEmailAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.VerifyEmail(request);

        // Assert
        response.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = response as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be(error);
    }

    [Fact]
    public async Task GetCurrentUser_WithValidUser_ShouldReturnOkResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userResponse = _fixture.Create<UserResponse>();
        var result = Result.Success(userResponse);

        _authenticationServiceMock.Setup(x => x.GetCurrentUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Set up HttpContext with User claims
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

        // Act
        var response = await _sut.GetCurrentUser();

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(userResponse);
    }

    [Fact]
    public async Task GetCurrentUser_WithInvalidUser_ShouldReturnNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var error = Error.NotFound("Authentication.UserNotFound", "User not found");
        var result = Result.Failure<UserResponse>(error);

        _authenticationServiceMock.Setup(x => x.GetCurrentUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Set up HttpContext with User claims
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

        // Act
        var response = await _sut.GetCurrentUser();

        // Assert
        response.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = response as NotFoundObjectResult;
        notFoundResult!.Value.Should().Be(error);
    }

    [Fact]
    public async Task GetCurrentUser_WithoutValidClaims_ShouldReturnUnauthorized()
    {
        // Arrange
        // Create controller without valid user claims
        var controller = new AuthController(_authenticationServiceMock.Object);
        
        // Mock HttpContext without valid claims
        var httpContext = new Mock<HttpContext>();
        var user = new Mock<System.Security.Claims.ClaimsPrincipal>();
        user.Setup(x => x.FindFirst(It.IsAny<string>())).Returns((System.Security.Claims.Claim?)null);
        httpContext.Setup(x => x.User).Returns(user.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext.Object
        };

        // Act
        var response = await controller.GetCurrentUser();

        // Assert
        response.Should().BeOfType<UnauthorizedResult>();
    }
}

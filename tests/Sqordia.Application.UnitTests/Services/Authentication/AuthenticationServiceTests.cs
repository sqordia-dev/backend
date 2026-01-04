using AutoFixture;
using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Common.Security;
using Sqordia.Application.Services.Implementations;
using Sqordia.Contracts.Requests.Auth;
using Sqordia.Contracts.Responses.Auth;
using Sqordia.Domain.Entities.Identity;
using Sqordia.Domain.ValueObjects;
using Sqordia.Persistence.Contexts;
using Xunit;
using AutoMapper;
using Sqordia.Application.UnitTests.Common;

namespace Sqordia.Application.UnitTests.Services.Authentication;

public class AuthenticationServiceTests : IDisposable
{
    private readonly IFixture _fixture;
    private readonly ApplicationDbContext _context;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ISecurityService> _securityServiceMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<HttpContext> _httpContextMock;
    private readonly Mock<ConnectionInfo> _connectionInfoMock;
    private readonly Mock<ILogger<AuthenticationService>> _loggerMock;
    private readonly AuthenticationService _sut;

    public AuthenticationServiceTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _fixture.Customizations.Add(new EmailAddressSpecimenBuilder());
        _fixture.Customizations.Add(new RefreshTokenSpecimenBuilder());
        _fixture.Customizations.Add(new UserSpecimenBuilder());
        _fixture.Customizations.Add(new PasswordResetTokenSpecimenBuilder());
        _fixture.Customizations.Add(new EmailVerificationTokenSpecimenBuilder());

        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        // Setup mocks
        _mapperMock = new Mock<IMapper>();
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();
        _emailServiceMock = new Mock<IEmailService>();
        _securityServiceMock = new Mock<ISecurityService>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _httpContextMock = new Mock<HttpContext>();
        _connectionInfoMock = new Mock<ConnectionInfo>();
        _loggerMock = new Mock<ILogger<AuthenticationService>>();

        // Setup HTTP context mock
        var headers = new HeaderDictionary();
        headers["User-Agent"] = "Test User Agent";
        var requestMock = new Mock<HttpRequest>();
        requestMock.Setup(x => x.Headers).Returns(headers);
        _httpContextMock.Setup(x => x.Request).Returns(requestMock.Object);
        _httpContextMock.Setup(x => x.Connection).Returns(_connectionInfoMock.Object);
        _connectionInfoMock.Setup(x => x.RemoteIpAddress).Returns(System.Net.IPAddress.Parse("127.0.0.1"));
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(_httpContextMock.Object);

        // Create mock for ILocalizationService
        var localizationServiceMock = new Mock<ILocalizationService>();
        localizationServiceMock
            .Setup(x => x.GetString(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns<string, object[]>((key, args) => key); // Return the key itself for testing
        localizationServiceMock
            .Setup(x => x.GetCurrentLanguage())
            .Returns("fr");

        // Create service under test
        _sut = new AuthenticationService(
            _context,
            _mapperMock.Object,
            _jwtTokenServiceMock.Object,
            _emailServiceMock.Object,
            _securityServiceMock.Object,
            _httpContextAccessorMock.Object,
            _loggerMock.Object,
            localizationServiceMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var request = new RegisterRequest
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Password = "TestPassword123!",
            ConfirmPassword = "TestPassword123!",
            UserName = "testuser",
            UserType = "Entrepreneur"
        };
        var authResponse = _fixture.Create<AuthResponse>();
        var refreshToken = _fixture.Create<RefreshToken>();
        var hashedPassword = _fixture.Create<string>();

        _securityServiceMock.Setup(x => x.HashPassword(request.Password))
            .Returns(hashedPassword);
        _jwtTokenServiceMock.Setup(x => x.GenerateAccessTokenAsync(It.IsAny<User>()))
            .ReturnsAsync(authResponse.Token);
        _jwtTokenServiceMock.Setup(x => x.GenerateRefreshTokenAsync(It.IsAny<Guid>(), It.IsAny<string>()))
            .ReturnsAsync(refreshToken);
        _securityServiceMock.Setup(x => x.GenerateSecureToken(It.IsAny<int>()))
            .Returns(_fixture.Create<string>());
        _mapperMock.Setup(x => x.Map<UserDto>(It.IsAny<User>()))
            .Returns(authResponse.User);

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Token.Should().Be(authResponse.Token);
        result.Value.RefreshToken.Should().Be(refreshToken.Token);

        _context.Users.Should().HaveCount(1);
        _context.EmailVerificationTokens.Should().HaveCount(1);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ShouldReturnFailure()
    {
        // Arrange
        var request = new RegisterRequest
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Password = "TestPassword123!",
            ConfirmPassword = "TestPassword123!",
            UserName = "testuser",
            UserType = "Entrepreneur"
        };
        var existingUser = new User("Existing", "User", new EmailAddress(request.Email), "existinguser");
        existingUser.SetPasswordHash("hashedpassword");

        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Auth.Error.EmailAlreadyExists");
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnSuccess()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "TestPassword123!"
        };
        var user = new User("Test", "User", new EmailAddress(request.Email), "testuser");
        var authResponse = _fixture.Create<AuthResponse>();
        var refreshToken = _fixture.Create<RefreshToken>();
        var hashedPassword = _fixture.Create<string>();

        user.SetPasswordHash(hashedPassword);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _securityServiceMock.Setup(x => x.VerifyPassword(request.Password, hashedPassword))
            .Returns(true);
        _jwtTokenServiceMock.Setup(x => x.GenerateAccessTokenAsync(It.IsAny<User>()))
            .ReturnsAsync(authResponse.Token);
        _jwtTokenServiceMock.Setup(x => x.GenerateRefreshTokenAsync(It.IsAny<Guid>(), It.IsAny<string>()))
            .ReturnsAsync(refreshToken);
        _mapperMock.Setup(x => x.Map<UserDto>(It.IsAny<User>()))
            .Returns(authResponse.User);

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Token.Should().Be(authResponse.Token);
        result.Value.RefreshToken.Should().Be(refreshToken.Token);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidCredentials_ShouldReturnFailure()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "WrongPassword123!"
        };
        var user = new User("Test", "User", new EmailAddress(request.Email), "testuser");
        var hashedPassword = _fixture.Create<string>();

        user.SetPasswordHash(hashedPassword);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _securityServiceMock.Setup(x => x.VerifyPassword(request.Password, hashedPassword))
            .Returns(false);

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Auth.Error.InvalidCredentials");
    }

    [Fact]
    public async Task LoginAsync_WithInactiveUser_ShouldReturnFailure()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "TestPassword123!"
        };
        var user = new User("Test", "User", new EmailAddress(request.Email), "testuser");
        var hashedPassword = _fixture.Create<string>();

        user.SetPasswordHash(hashedPassword);
        user.Deactivate(); // Make user inactive
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _securityServiceMock.Setup(x => x.VerifyPassword(request.Password, hashedPassword))
            .Returns(true);

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Auth.Error.EmailNotVerified");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ShouldReturnSuccess()
    {
        // Arrange
        var request = _fixture.Create<RefreshTokenRequest>();
        var user = new User("Test", "User", new EmailAddress("test@example.com"), "testuser");
        user.SetPasswordHash("hashedpassword");
        var refreshToken = new RefreshToken(
            user.Id,
            request.RefreshToken,
            DateTime.UtcNow.AddHours(1),
            "127.0.0.1");
        var authResponse = _fixture.Create<AuthResponse>();
        var newRefreshToken = _fixture.Create<RefreshToken>();

        _context.Users.Add(user);
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        _jwtTokenServiceMock.Setup(x => x.GetRefreshTokenAsync(request.RefreshToken))
            .ReturnsAsync(refreshToken);
        _jwtTokenServiceMock.Setup(x => x.RevokeRefreshTokenAsync(It.IsAny<RefreshToken>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _jwtTokenServiceMock.Setup(x => x.GenerateAccessTokenAsync(It.IsAny<User>()))
            .ReturnsAsync(authResponse.Token);
        _jwtTokenServiceMock.Setup(x => x.GenerateRefreshTokenAsync(It.IsAny<Guid>(), It.IsAny<string>()))
            .ReturnsAsync(newRefreshToken);
        _mapperMock.Setup(x => x.Map<UserDto>(It.IsAny<User>()))
            .Returns(authResponse.User);

        // Act
        var result = await _sut.RefreshTokenAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Token.Should().Be(authResponse.Token);
        result.Value.RefreshToken.Should().Be(newRefreshToken.Token);
    }

    [Fact]
    public async Task RefreshTokenAsync_WithExpiredToken_ShouldReturnFailure()
    {
        // Arrange
        var request = _fixture.Create<RefreshTokenRequest>();
        var refreshToken = new RefreshToken(
            _fixture.Create<Guid>(),
            request.RefreshToken,
            DateTime.UtcNow.AddHours(-1), // Expired
            "127.0.0.1");

        _jwtTokenServiceMock.Setup(x => x.GetRefreshTokenAsync(request.RefreshToken))
            .ReturnsAsync(refreshToken);

        // Act
        var result = await _sut.RefreshTokenAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Auth.Error.InvalidToken");
    }

    [Fact]
    public async Task LogoutAsync_WithValidRefreshToken_ShouldReturnSuccess()
    {
        // Arrange
        var request = _fixture.Create<LogoutRequest>();
        var refreshToken = new RefreshToken(
            _fixture.Create<Guid>(),
            request.RefreshToken,
            DateTime.UtcNow.AddHours(1),
            "127.0.0.1");

        _jwtTokenServiceMock.Setup(x => x.GetRefreshTokenAsync(request.RefreshToken))
            .ReturnsAsync(refreshToken);
        _jwtTokenServiceMock.Setup(x => x.RevokeRefreshTokenAsync(It.IsAny<RefreshToken>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.LogoutAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ForgotPasswordAsync_WithExistingUser_ShouldReturnSuccess()
    {
        // Arrange
        var request = new ForgotPasswordRequest
        {
            Email = "test@example.com"
        };
        var user = new User("Test", "User", new EmailAddress(request.Email), "testuser");
        var resetToken = _fixture.Create<string>();

        user.SetPasswordHash("hashedpassword");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _securityServiceMock.Setup(x => x.GenerateSecureToken(It.IsAny<int>()))
            .Returns(resetToken);

        // Act
        var result = await _sut.ForgotPasswordAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        _context.PasswordResetTokens.Should().HaveCount(1);
    }

    [Fact]
    public async Task ForgotPasswordAsync_WithNonExistingUser_ShouldReturnSuccess()
    {
        // Arrange
        var request = _fixture.Create<ForgotPasswordRequest>();

        // Act
        var result = await _sut.ForgotPasswordAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        _context.PasswordResetTokens.Should().HaveCount(0);
    }

    [Fact]
    public async Task ResetPasswordAsync_WithValidToken_ShouldReturnSuccess()
    {
        // Arrange
        var request = new ResetPasswordRequest
        {
            Token = "valid-reset-token",
            NewPassword = "NewPassword123!"
        };
        var user = new User("Test", "User", new EmailAddress("test@example.com"), "testuser");
        var resetToken = new PasswordResetToken(user.Id, request.Token, DateTime.UtcNow.AddHours(1));
        var hashedPassword = _fixture.Create<string>();

        user.SetPasswordHash("oldhashedpassword");
        _context.Users.Add(user);
        _context.PasswordResetTokens.Add(resetToken);
        await _context.SaveChangesAsync();

        _securityServiceMock.Setup(x => x.HashPassword(request.NewPassword))
            .Returns(hashedPassword);

        // Act
        var result = await _sut.ResetPasswordAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        user.PasswordHash.Should().Be(hashedPassword);
        resetToken.IsUsed.Should().BeTrue();
    }

    [Fact]
    public async Task ResetPasswordAsync_WithExpiredToken_ShouldReturnFailure()
    {
        // Arrange
        var request = new ResetPasswordRequest
        {
            Token = "expired-reset-token",
            NewPassword = "NewPassword123!"
        };
        var user = new User("Test", "User", new EmailAddress("test@example.com"), "testuser");
        var resetToken = new PasswordResetToken(user.Id, request.Token, DateTime.UtcNow.AddHours(-1)); // Expired

        user.SetPasswordHash("oldhashedpassword");
        _context.Users.Add(user);
        _context.PasswordResetTokens.Add(resetToken);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.ResetPasswordAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Auth.Error.InvalidToken");
    }

    [Fact]
    public async Task VerifyEmailAsync_WithValidToken_ShouldReturnSuccess()
    {
        // Arrange
        var request = new VerifyEmailRequest
        {
            Token = "valid-verification-token"
        };
        var user = new User("Test", "User", new EmailAddress("test@example.com"), "testuser");
        var verificationToken = new EmailVerificationToken(user.Id, request.Token, DateTime.UtcNow.AddHours(1));

        user.SetPasswordHash("hashedpassword");
        _context.Users.Add(user);
        _context.EmailVerificationTokens.Add(verificationToken);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.VerifyEmailAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        user.IsEmailConfirmed.Should().BeTrue();
        user.EmailConfirmedAt.Should().NotBeNull();
        verificationToken.IsUsed.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyEmailAsync_WithExpiredToken_ShouldReturnFailure()
    {
        // Arrange
        var request = new VerifyEmailRequest
        {
            Token = "expired-verification-token"
        };
        var user = new User("Test", "User", new EmailAddress("test@example.com"), "testuser");
        var verificationToken = new EmailVerificationToken(user.Id, request.Token, DateTime.UtcNow.AddHours(-1)); // Expired

        user.SetPasswordHash("hashedpassword");
        _context.Users.Add(user);
        _context.EmailVerificationTokens.Add(verificationToken);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.VerifyEmailAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Auth.Error.InvalidToken");
    }

    [Fact]
    public async Task GetCurrentUserAsync_WithValidUserId_ShouldReturnSuccess()
    {
        // Arrange
        var user = new User("Test", "User", new EmailAddress("test@example.com"), "testuser");
        var userResponse = _fixture.Create<UserResponse>();

        user.SetPasswordHash("hashedpassword");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _mapperMock.Setup(x => x.Map<UserResponse>(It.IsAny<User>()))
            .Returns(userResponse);

        // Act
        var result = await _sut.GetCurrentUserAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().Be(userResponse);
    }

    [Fact]
    public async Task GetCurrentUserAsync_WithNonExistingUserId_ShouldReturnFailure()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();

        // Act
        var result = await _sut.GetCurrentUserAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Auth.Error.UserNotFound");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

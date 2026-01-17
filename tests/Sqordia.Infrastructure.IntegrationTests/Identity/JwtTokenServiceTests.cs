using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Sqordia.Domain.Entities.Identity;
using Sqordia.Infrastructure.Identity;
using Sqordia.Infrastructure.IntegrationTests.Common;
using Sqordia.Persistence.Contexts;
using Xunit;

namespace Sqordia.Infrastructure.IntegrationTests.Identity;

public class JwtTokenServiceTests : IDisposable
{
    private readonly IFixture _fixture;
    private readonly ApplicationDbContext _context;
    private readonly Mock<ILogger<JwtTokenService>> _loggerMock;
    private readonly IConfiguration _configuration;
    private readonly JwtTokenService _sut;

    public JwtTokenServiceTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        
        // Register custom specimen builders
        _fixture.Customizations.Add(new EmailAddressSpecimenBuilder());
        _fixture.Customizations.Add(new UserSpecimenBuilder());

        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        // Setup logger mock
        _loggerMock = new Mock<ILogger<JwtTokenService>>();

        // Setup configuration
        var configurationData = new Dictionary<string, string>
        {
            ["JwtSettings:Secret"] = "YourSuperSecretKeyThatIsAtLeast32CharactersLongForSecurity123!",
            ["JwtSettings:Issuer"] = "Sqordia",
            ["JwtSettings:Audience"] = "SqordiaUsers",
            ["JwtSettings:ExpirationInMinutes"] = "60"
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData!)
            .Build();

        // Create service under test
        _sut = new JwtTokenService(_configuration, _context);
    }

    [Fact]
    public async Task GenerateAccessTokenAsync_WithValidUser_ShouldReturnValidToken()
    {
        // Arrange
        var user = _fixture.Create<User>();

        // Act
        var token = await _sut.GenerateAccessTokenAsync(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Split('.').Should().HaveCount(3); // JWT has 3 parts separated by dots
    }

    [Fact]
    public async Task GenerateRefreshTokenAsync_WithValidUserId_ShouldCreateRefreshToken()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var ipAddress = _fixture.Create<string>();

        // Act
        var refreshToken = await _sut.GenerateRefreshTokenAsync(userId, ipAddress);

        // Assert
        refreshToken.Should().NotBeNull();
        refreshToken.UserId.Should().Be(userId);
        refreshToken.CreatedByIp.Should().Be(ipAddress);
        refreshToken.Token.Should().NotBeNullOrEmpty();
        refreshToken.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        refreshToken.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetRefreshTokenAsync_WithValidToken_ShouldReturnRefreshToken()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var ipAddress = _fixture.Create<string>();
        var refreshToken = await _sut.GenerateRefreshTokenAsync(userId, ipAddress);
        await _context.SaveChangesAsync(); // Save the token to the database

        // Act
        var retrievedToken = await _sut.GetRefreshTokenAsync(refreshToken.Token);

        // Assert
        retrievedToken.Should().NotBeNull();
        retrievedToken!.Token.Should().Be(refreshToken.Token);
        retrievedToken.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task GetRefreshTokenAsync_WithInvalidToken_ShouldReturnNull()
    {
        // Arrange
        var invalidToken = _fixture.Create<string>();

        // Act
        var retrievedToken = await _sut.GetRefreshTokenAsync(invalidToken);

        // Assert
        retrievedToken.Should().BeNull();
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_ShouldDeactivateToken()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var ipAddress = _fixture.Create<string>();
        var refreshToken = await _sut.GenerateRefreshTokenAsync(userId, ipAddress);
        await _context.SaveChangesAsync(); // Save the token to the database

        // Act
        await _sut.RevokeRefreshTokenAsync(refreshToken, ipAddress);

        // Assert
        var retrievedToken = await _sut.GetRefreshTokenAsync(refreshToken.Token);
        retrievedToken.Should().NotBeNull();
        retrievedToken!.IsActive.Should().BeFalse();
        retrievedToken.RevokedAt.Should().NotBeNull();
        retrievedToken.RevokedByIp.Should().Be(ipAddress);
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_WithReplacedByToken_ShouldSetReplacedByToken()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var ipAddress = _fixture.Create<string>();
        var refreshToken = await _sut.GenerateRefreshTokenAsync(userId, ipAddress);
        await _context.SaveChangesAsync(); // Save the token to the database
        var replacedByToken = _fixture.Create<string>();

        // Act
        await _sut.RevokeRefreshTokenAsync(refreshToken, ipAddress, replacedByToken);

        // Assert
        var retrievedToken = await _sut.GetRefreshTokenAsync(refreshToken.Token);
        retrievedToken.Should().NotBeNull();
        retrievedToken!.IsActive.Should().BeFalse();
        retrievedToken.ReplacedByToken.Should().Be(replacedByToken);
    }

    [Fact]
    public async Task IsAccessTokenValidAsync_WithValidToken_ShouldReturnTrue()
    {
        // Arrange
        var user = _fixture.Create<User>();
        var token = await _sut.GenerateAccessTokenAsync(user);

        // Act
        var isValid = await _sut.IsAccessTokenValidAsync(token);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public async Task IsAccessTokenValidAsync_WithInvalidToken_ShouldReturnFalse()
    {
        // Arrange
        var invalidToken = _fixture.Create<string>();

        // Act
        var isValid = await _sut.IsAccessTokenValidAsync(invalidToken);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAccessTokenAsync_WithValidToken_ShouldReturnUserId()
    {
        // Arrange
        var user = _fixture.Create<User>();
        var token = await _sut.GenerateAccessTokenAsync(user);

        // Act
        var userId = await _sut.ValidateAccessTokenAsync(token);

        // Assert
        userId.Should().NotBeNull();
        userId.Should().Be(user.Id.ToString());
    }

    [Fact]
    public async Task ValidateAccessTokenAsync_WithInvalidToken_ShouldReturnNull()
    {
        // Arrange
        var invalidToken = _fixture.Create<string>();

        // Act
        var userId = await _sut.ValidateAccessTokenAsync(invalidToken);

        // Assert
        userId.Should().BeNull();
    }

    [Fact]
    public async Task GenerateRefreshTokenAsync_ShouldCreateUniqueTokens()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var ipAddress = _fixture.Create<string>();

        // Act
        var token1 = await _sut.GenerateRefreshTokenAsync(userId, ipAddress);
        var token2 = await _sut.GenerateRefreshTokenAsync(userId, ipAddress);

        // Assert
        token1.Token.Should().NotBe(token2.Token);
    }

    [Fact]
    public async Task GenerateAccessTokenAsync_ShouldCreateUniqueTokens()
    {
        // Arrange
        var user = _fixture.Create<User>();

        // Act
        var token1 = await _sut.GenerateAccessTokenAsync(user);
        await Task.Delay(1000); // Wait 1 second to ensure different timestamps
        var token2 = await _sut.GenerateAccessTokenAsync(user);

        // Assert
        token1.Should().NotBe(token2);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

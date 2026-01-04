using AutoFixture;
using FluentAssertions;
using Sqordia.Domain.Entities.Identity;
using Xunit;

namespace Sqordia.Domain.UnitTests.Entities.Identity;

public class EmailVerificationTokenTests
{
    private readonly IFixture _fixture;

    public EmailVerificationTokenTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateToken()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var token = _fixture.Create<string>();
        var expiresAt = DateTime.UtcNow.AddHours(24);

        // Act
        var emailVerificationToken = new EmailVerificationToken(userId, token, expiresAt);

        // Assert
        emailVerificationToken.UserId.Should().Be(userId);
        emailVerificationToken.Token.Should().Be(token);
        emailVerificationToken.ExpiresAt.Should().Be(expiresAt);
        emailVerificationToken.IsUsed.Should().BeFalse();
        emailVerificationToken.UsedAt.Should().BeNull();
        emailVerificationToken.UsedByIp.Should().BeNull();
        emailVerificationToken.CreatedAt.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public void Constructor_WithNullToken_ShouldThrowArgumentNullException()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var expiresAt = DateTime.UtcNow.AddHours(24);

        // Act & Assert
        var action = () => new EmailVerificationToken(userId, null!, expiresAt);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("token");
    }

    [Fact]
    public void IsExpired_WhenExpiresAtIsInFuture_ShouldReturnFalse()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var token = _fixture.Create<string>();
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var emailVerificationToken = new EmailVerificationToken(userId, token, expiresAt);

        // Act
        var isExpired = emailVerificationToken.IsExpired;

        // Assert
        isExpired.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_WhenExpiresAtIsInPast_ShouldReturnTrue()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var token = _fixture.Create<string>();
        var expiresAt = DateTime.UtcNow.AddHours(-1);
        var emailVerificationToken = new EmailVerificationToken(userId, token, expiresAt);

        // Act
        var isExpired = emailVerificationToken.IsExpired;

        // Assert
        isExpired.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WhenNotUsedAndNotExpired_ShouldReturnTrue()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var token = _fixture.Create<string>();
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var emailVerificationToken = new EmailVerificationToken(userId, token, expiresAt);

        // Act
        var isValid = emailVerificationToken.IsValid;

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WhenUsed_ShouldReturnFalse()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var token = _fixture.Create<string>();
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var emailVerificationToken = new EmailVerificationToken(userId, token, expiresAt);
        emailVerificationToken.MarkAsUsed();

        // Act
        var isValid = emailVerificationToken.IsValid;

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WhenExpired_ShouldReturnFalse()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var token = _fixture.Create<string>();
        var expiresAt = DateTime.UtcNow.AddHours(-1);
        var emailVerificationToken = new EmailVerificationToken(userId, token, expiresAt);

        // Act
        var isValid = emailVerificationToken.IsValid;

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsActive_WhenNotUsedAndNotExpired_ShouldReturnTrue()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var token = _fixture.Create<string>();
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var emailVerificationToken = new EmailVerificationToken(userId, token, expiresAt);

        // Act
        var isActive = emailVerificationToken.IsActive;

        // Assert
        isActive.Should().BeTrue();
    }

    [Fact]
    public void IsActive_WhenUsed_ShouldReturnFalse()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var token = _fixture.Create<string>();
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var emailVerificationToken = new EmailVerificationToken(userId, token, expiresAt);
        emailVerificationToken.MarkAsUsed();

        // Act
        var isActive = emailVerificationToken.IsActive;

        // Assert
        isActive.Should().BeFalse();
    }

    [Fact]
    public void MarkAsUsed_WithoutIpAddress_ShouldMarkAsUsed()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var token = _fixture.Create<string>();
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var emailVerificationToken = new EmailVerificationToken(userId, token, expiresAt);
        var beforeMark = DateTime.UtcNow;

        // Act
        emailVerificationToken.MarkAsUsed();

        // Assert
        emailVerificationToken.IsUsed.Should().BeTrue();
        emailVerificationToken.UsedAt.Should().NotBeNull();
        emailVerificationToken.UsedAt.Should().BeOnOrAfter(beforeMark);
        emailVerificationToken.UsedByIp.Should().BeNull();
    }

    [Fact]
    public void MarkAsUsed_WithIpAddress_ShouldMarkAsUsedWithIp()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var token = _fixture.Create<string>();
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var ipAddress = _fixture.Create<string>();
        var emailVerificationToken = new EmailVerificationToken(userId, token, expiresAt);
        var beforeMark = DateTime.UtcNow;

        // Act
        emailVerificationToken.MarkAsUsed(ipAddress);

        // Assert
        emailVerificationToken.IsUsed.Should().BeTrue();
        emailVerificationToken.UsedAt.Should().NotBeNull();
        emailVerificationToken.UsedAt.Should().BeOnOrAfter(beforeMark);
        emailVerificationToken.UsedByIp.Should().Be(ipAddress);
    }

    [Fact]
    public void Deactivate_ShouldMarkAsUsed()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var token = _fixture.Create<string>();
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var emailVerificationToken = new EmailVerificationToken(userId, token, expiresAt);

        // Act
        emailVerificationToken.Deactivate();

        // Assert
        emailVerificationToken.IsUsed.Should().BeTrue();
        emailVerificationToken.UsedAt.Should().NotBeNull();
    }

    [Fact]
    public void GenerateToken_ShouldReturnValidGuid()
    {
        // Act
        var token = EmailVerificationToken.GenerateToken();

        // Assert
        token.Should().NotBeNullOrEmpty();
        Guid.TryParse(token, out _).Should().BeTrue();
    }
}

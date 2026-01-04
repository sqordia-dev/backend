using AutoFixture;
using FluentAssertions;
using Sqordia.Domain.Entities.Identity;
using Xunit;

namespace Sqordia.Domain.UnitTests.Entities.Identity;

public class PasswordResetTokenTests
{
    private readonly IFixture _fixture;

    public PasswordResetTokenTests()
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
        var expiresAt = DateTime.UtcNow.AddHours(1);

        // Act
        var passwordResetToken = new PasswordResetToken(userId, token, expiresAt);

        // Assert
        passwordResetToken.UserId.Should().Be(userId);
        passwordResetToken.Token.Should().Be(token);
        passwordResetToken.ExpiresAt.Should().Be(expiresAt);
        passwordResetToken.IsUsed.Should().BeFalse();
        passwordResetToken.UsedAt.Should().BeNull();
        passwordResetToken.UsedByIp.Should().BeNull();
        passwordResetToken.CreatedAt.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public void Constructor_WithNullToken_ShouldThrowArgumentNullException()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var expiresAt = DateTime.UtcNow.AddHours(1);

        // Act & Assert
        var action = () => new PasswordResetToken(userId, null!, expiresAt);
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
        var passwordResetToken = new PasswordResetToken(userId, token, expiresAt);

        // Act
        var isExpired = passwordResetToken.IsExpired;

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
        var passwordResetToken = new PasswordResetToken(userId, token, expiresAt);

        // Act
        var isExpired = passwordResetToken.IsExpired;

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
        var passwordResetToken = new PasswordResetToken(userId, token, expiresAt);

        // Act
        var isValid = passwordResetToken.IsValid;

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
        var passwordResetToken = new PasswordResetToken(userId, token, expiresAt);
        passwordResetToken.MarkAsUsed();

        // Act
        var isValid = passwordResetToken.IsValid;

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
        var passwordResetToken = new PasswordResetToken(userId, token, expiresAt);

        // Act
        var isValid = passwordResetToken.IsValid;

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
        var passwordResetToken = new PasswordResetToken(userId, token, expiresAt);

        // Act
        var isActive = passwordResetToken.IsActive;

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
        var passwordResetToken = new PasswordResetToken(userId, token, expiresAt);
        passwordResetToken.MarkAsUsed();

        // Act
        var isActive = passwordResetToken.IsActive;

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
        var passwordResetToken = new PasswordResetToken(userId, token, expiresAt);
        var beforeMark = DateTime.UtcNow;

        // Act
        passwordResetToken.MarkAsUsed();

        // Assert
        passwordResetToken.IsUsed.Should().BeTrue();
        passwordResetToken.UsedAt.Should().NotBeNull();
        passwordResetToken.UsedAt.Should().BeOnOrAfter(beforeMark);
        passwordResetToken.UsedByIp.Should().BeNull();
    }

    [Fact]
    public void MarkAsUsed_WithIpAddress_ShouldMarkAsUsedWithIp()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var token = _fixture.Create<string>();
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var ipAddress = _fixture.Create<string>();
        var passwordResetToken = new PasswordResetToken(userId, token, expiresAt);
        var beforeMark = DateTime.UtcNow;

        // Act
        passwordResetToken.MarkAsUsed(ipAddress);

        // Assert
        passwordResetToken.IsUsed.Should().BeTrue();
        passwordResetToken.UsedAt.Should().NotBeNull();
        passwordResetToken.UsedAt.Should().BeOnOrAfter(beforeMark);
        passwordResetToken.UsedByIp.Should().Be(ipAddress);
    }

    [Fact]
    public void Deactivate_ShouldMarkAsUsed()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var token = _fixture.Create<string>();
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var passwordResetToken = new PasswordResetToken(userId, token, expiresAt);

        // Act
        passwordResetToken.Deactivate();

        // Assert
        passwordResetToken.IsUsed.Should().BeTrue();
        passwordResetToken.UsedAt.Should().NotBeNull();
    }

    [Fact]
    public void GenerateToken_ShouldReturnValidBase64String()
    {
        // Act
        var token = PasswordResetToken.GenerateToken();

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Length.Should().BeGreaterThan(20); // Base64 encoded 32 bytes should be longer
        // Should not contain characters that would be in base64 but not URL-safe
        token.Should().NotContain("+");
        token.Should().NotContain("/");
        token.Should().NotContain("=");
    }
}

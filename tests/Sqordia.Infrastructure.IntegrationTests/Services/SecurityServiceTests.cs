using AutoFixture;
using FluentAssertions;
using Sqordia.Infrastructure.Services;
using Xunit;

namespace Sqordia.Infrastructure.IntegrationTests.Services;

public class SecurityServiceTests
{
    private readonly IFixture _fixture;
    private readonly SecurityService _sut;

    public SecurityServiceTests()
    {
        _fixture = new Fixture();
        _sut = new SecurityService();
    }

    [Fact]
    public void HashPassword_WithValidPassword_ShouldReturnHashedPassword()
    {
        // Arrange
        var password = _fixture.Create<string>();

        // Act
        var hashedPassword = _sut.HashPassword(password);

        // Assert
        hashedPassword.Should().NotBeNullOrEmpty();
        hashedPassword.Should().NotBe(password);
        hashedPassword.Length.Should().BeGreaterThan(50); // BCrypt hashes are typically 60 characters
    }

    [Fact]
    public void HashPassword_WithSamePassword_ShouldReturnDifferentHashes()
    {
        // Arrange
        var password = _fixture.Create<string>();

        // Act
        var hash1 = _sut.HashPassword(password);
        var hash2 = _sut.HashPassword(password);

        // Assert
        hash1.Should().NotBe(hash2); // BCrypt generates different salts each time
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = _fixture.Create<string>();
        var hashedPassword = _sut.HashPassword(password);

        // Act
        var isValid = _sut.VerifyPassword(password, hashedPassword);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var password = _fixture.Create<string>();
        var wrongPassword = _fixture.Create<string>();
        var hashedPassword = _sut.HashPassword(password);

        // Act
        var isValid = _sut.VerifyPassword(wrongPassword, hashedPassword);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_WithNullPassword_ShouldReturnFalse()
    {
        // Arrange
        var hashedPassword = _fixture.Create<string>();

        // Act
        var isValid = _sut.VerifyPassword(null!, hashedPassword);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_WithNullHash_ShouldReturnFalse()
    {
        // Arrange
        var password = _fixture.Create<string>();

        // Act
        var isValid = _sut.VerifyPassword(password, null!);

        // Assert
        isValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(16)]
    [InlineData(32)]
    [InlineData(64)]
    [InlineData(128)]
    public void GenerateSecureToken_WithDifferentLengths_ShouldReturnCorrectLength(int length)
    {
        // Act
        var token = _sut.GenerateSecureToken(length);

        // Assert
        token.Should().NotBeNullOrEmpty();
        // Base64 encoding increases length by ~33%, so we check for reasonable range
        token.Length.Should().BeGreaterOrEqualTo(length);
        token.Length.Should().BeLessOrEqualTo(length * 2);
    }

    [Fact]
    public void GenerateSecureToken_WithDefaultLength_ShouldReturnToken()
    {
        // Act
        var token = _sut.GenerateSecureToken();

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Length.Should().BeGreaterThan(20); // Default is 32 bytes, base64 encoded
    }

    [Fact]
    public void GenerateSecureToken_ShouldGenerateUniqueTokens()
    {
        // Act
        var token1 = _sut.GenerateSecureToken();
        var token2 = _sut.GenerateSecureToken();

        // Assert
        token1.Should().NotBe(token2);
    }

    [Fact]
    public void GenerateSecureToken_ShouldGenerateBase64CompatibleTokens()
    {
        // Act
        var token = _sut.GenerateSecureToken();

        // Assert
        // Should be able to decode as base64
        var action = () => Convert.FromBase64String(token);
        action.Should().NotThrow();
    }

    [Fact]
    public void HashPassword_WithEmptyPassword_ShouldReturnHashedPassword()
    {
        // Arrange
        var password = string.Empty;

        // Act
        var hashedPassword = _sut.HashPassword(password);

        // Assert
        hashedPassword.Should().NotBeNullOrEmpty();
        hashedPassword.Should().NotBe(password);
    }

    [Fact]
    public void VerifyPassword_WithEmptyPassword_ShouldWorkCorrectly()
    {
        // Arrange
        var password = string.Empty;
        var hashedPassword = _sut.HashPassword(password);

        // Act
        var isValid = _sut.VerifyPassword(password, hashedPassword);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void HashPassword_WithSpecialCharacters_ShouldWorkCorrectly()
    {
        // Arrange
        var password = "!@#$%^&*()_+-=[]{}|;':\",./<>?";

        // Act
        var hashedPassword = _sut.HashPassword(password);

        // Assert
        hashedPassword.Should().NotBeNullOrEmpty();
        hashedPassword.Should().NotBe(password);
    }

    [Fact]
    public void VerifyPassword_WithSpecialCharacters_ShouldWorkCorrectly()
    {
        // Arrange
        var password = "!@#$%^&*()_+-=[]{}|;':\",./<>?";
        var hashedPassword = _sut.HashPassword(password);

        // Act
        var isValid = _sut.VerifyPassword(password, hashedPassword);

        // Assert
        isValid.Should().BeTrue();
    }
}

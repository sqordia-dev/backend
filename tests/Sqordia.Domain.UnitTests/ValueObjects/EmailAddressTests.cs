using AutoFixture;
using FluentAssertions;
using Sqordia.Domain.ValueObjects;
using Xunit;

namespace Sqordia.Domain.UnitTests.ValueObjects;

public class EmailAddressTests
{
    private readonly IFixture _fixture;

    public EmailAddressTests()
    {
        _fixture = new Fixture();
    }

    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.co.uk")]
    [InlineData("user+tag@example.org")]
    [InlineData("123@test.com")]
    public void Constructor_WithValidEmail_ShouldCreateEmailAddress(string validEmail)
    {
        // Act
        var emailAddress = new EmailAddress(validEmail);

        // Assert
        emailAddress.Value.Should().Be(validEmail);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    [InlineData("test@.example.com")]
    [InlineData("test@example..com")]
    public void Constructor_WithInvalidEmail_ShouldThrowArgumentException(string invalidEmail)
    {
        // Act & Assert
        var action = () => new EmailAddress(invalidEmail);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WithNullEmail_ShouldThrowArgumentException()
    {
        // Act & Assert
        var action = () => new EmailAddress(null!);
        action.Should().Throw<ArgumentException>()
            .WithParameterName("email");
    }

    [Fact]
    public void Equals_WithSameEmail_ShouldReturnTrue()
    {
        // Arrange
        var email = "test@example.com";
        var emailAddress1 = new EmailAddress(email);
        var emailAddress2 = new EmailAddress(email);

        // Act
        var result = emailAddress1.Equals(emailAddress2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentEmail_ShouldReturnFalse()
    {
        // Arrange
        var emailAddress1 = new EmailAddress("test1@example.com");
        var emailAddress2 = new EmailAddress("test2@example.com");

        // Act
        var result = emailAddress1.Equals(emailAddress2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        var emailAddress = new EmailAddress("test@example.com");

        // Act
        var result = emailAddress.Equals(null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_WithSameEmail_ShouldReturnSameHashCode()
    {
        // Arrange
        var email = "test@example.com";
        var emailAddress1 = new EmailAddress(email);
        var emailAddress2 = new EmailAddress(email);

        // Act
        var hashCode1 = emailAddress1.GetHashCode();
        var hashCode2 = emailAddress2.GetHashCode();

        // Assert
        hashCode1.Should().Be(hashCode2);
    }

    [Fact]
    public void GetHashCode_WithDifferentEmail_ShouldReturnDifferentHashCode()
    {
        // Arrange
        var emailAddress1 = new EmailAddress("test1@example.com");
        var emailAddress2 = new EmailAddress("test2@example.com");

        // Act
        var hashCode1 = emailAddress1.GetHashCode();
        var hashCode2 = emailAddress2.GetHashCode();

        // Assert
        hashCode1.Should().NotBe(hashCode2);
    }

    [Fact]
    public void ToString_ShouldReturnEmailValue()
    {
        // Arrange
        var email = "test@example.com";
        var emailAddress = new EmailAddress(email);

        // Act
        var result = emailAddress.ToString();

        // Assert
        result.Should().Be(email);
    }

    [Fact]
    public void EqualityOperator_WithSameEmail_ShouldReturnTrue()
    {
        // Arrange
        var email = "test@example.com";
        var emailAddress1 = new EmailAddress(email);
        var emailAddress2 = new EmailAddress(email);

        // Act
        var result = emailAddress1 == emailAddress2;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void EqualityOperator_WithDifferentEmail_ShouldReturnFalse()
    {
        // Arrange
        var emailAddress1 = new EmailAddress("test1@example.com");
        var emailAddress2 = new EmailAddress("test2@example.com");

        // Act
        var result = emailAddress1 == emailAddress2;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void InequalityOperator_WithSameEmail_ShouldReturnFalse()
    {
        // Arrange
        var email = "test@example.com";
        var emailAddress1 = new EmailAddress(email);
        var emailAddress2 = new EmailAddress(email);

        // Act
        var result = emailAddress1 != emailAddress2;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void InequalityOperator_WithDifferentEmail_ShouldReturnTrue()
    {
        // Arrange
        var emailAddress1 = new EmailAddress("test1@example.com");
        var emailAddress2 = new EmailAddress("test2@example.com");

        // Act
        var result = emailAddress1 != emailAddress2;

        // Assert
        result.Should().BeTrue();
    }
}

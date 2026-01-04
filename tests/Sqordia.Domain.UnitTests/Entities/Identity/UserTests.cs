using AutoFixture;
using AutoFixture.Xunit2;
using FluentAssertions;
using Sqordia.Domain.Entities.Identity;
using Sqordia.Domain.ValueObjects;
using Sqordia.Domain.UnitTests.Common;
using Xunit;

namespace Sqordia.Domain.UnitTests.Entities.Identity;

public class UserTests
{
    private readonly IFixture _fixture;

    public UserTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _fixture.Customizations.Add(new EmailAddressSpecimenBuilder());
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateUser()
    {
        // Arrange
        var firstName = _fixture.Create<string>();
        var lastName = _fixture.Create<string>();
        var email = _fixture.Create<EmailAddress>();
        var userName = _fixture.Create<string>();

        // Act
        var user = new User(firstName, lastName, email, userName);

        // Assert
        user.FirstName.Should().Be(firstName);
        user.LastName.Should().Be(lastName);
        user.Email.Should().Be(email);
        user.UserName.Should().Be(userName);
        user.IsActive.Should().BeTrue();
        user.IsEmailConfirmed.Should().BeFalse();
        user.LockoutEnabled.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithNullFirstName_ShouldThrowArgumentNullException()
    {
        // Arrange
        var lastName = _fixture.Create<string>();
        var email = _fixture.Create<EmailAddress>();
        var userName = _fixture.Create<string>();

        // Act & Assert
        var action = () => new User(null!, lastName, email, userName);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("firstName");
    }

    [Fact]
    public void Constructor_WithNullLastName_ShouldThrowArgumentNullException()
    {
        // Arrange
        var firstName = _fixture.Create<string>();
        var email = _fixture.Create<EmailAddress>();
        var userName = _fixture.Create<string>();

        // Act & Assert
        var action = () => new User(firstName, null!, email, userName);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("lastName");
    }

    [Fact]
    public void Constructor_WithNullEmail_ShouldThrowArgumentNullException()
    {
        // Arrange
        var firstName = _fixture.Create<string>();
        var lastName = _fixture.Create<string>();
        var userName = _fixture.Create<string>();

        // Act & Assert
        var action = () => new User(firstName, lastName, null!, userName);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("email");
    }

    [Fact]
    public void Constructor_WithNullUserName_ShouldThrowArgumentNullException()
    {
        // Arrange
        var firstName = _fixture.Create<string>();
        var lastName = _fixture.Create<string>();
        var email = _fixture.Create<EmailAddress>();

        // Act & Assert
        var action = () => new User(firstName, lastName, email, null!);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("userName");
    }

    [Fact]
    public void SetPasswordHash_WithValidHash_ShouldSetPasswordHash()
    {
        // Arrange
        var user = _fixture.Create<User>();
        var passwordHash = _fixture.Create<string>();

        // Act
        user.SetPasswordHash(passwordHash);

        // Assert
        user.PasswordHash.Should().Be(passwordHash);
    }

    [Fact]
    public void SetPasswordHash_WithNullHash_ShouldThrowArgumentNullException()
    {
        // Arrange
        var user = _fixture.Create<User>();

        // Act & Assert
        var action = () => user.SetPasswordHash(null!);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("passwordHash");
    }

    [Fact]
    public void ConfirmEmail_ShouldSetEmailAsConfirmed()
    {
        // Arrange
        var user = _fixture.Create<User>();
        var beforeConfirm = DateTime.UtcNow;

        // Act
        user.ConfirmEmail();

        // Assert
        user.IsEmailConfirmed.Should().BeTrue();
        user.EmailConfirmedAt.Should().NotBeNull();
        user.EmailConfirmedAt.Should().BeOnOrAfter(beforeConfirm);
    }

    [Fact]
    public void Deactivate_ShouldSetUserAsInactive()
    {
        // Arrange
        var user = _fixture.Create<User>();

        // Act
        user.Deactivate();

        // Assert
        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_ShouldSetUserAsActive()
    {
        // Arrange
        var user = _fixture.Create<User>();
        user.Deactivate(); // First deactivate

        // Act
        user.Activate();

        // Assert
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void UpdateLastLogin_ShouldSetLastLoginAt()
    {
        // Arrange
        var user = _fixture.Create<User>();
        var beforeUpdate = DateTime.UtcNow;

        // Act
        user.UpdateLastLogin();

        // Assert
        user.LastLoginAt.Should().NotBeNull();
        user.LastLoginAt.Should().BeOnOrAfter(beforeUpdate);
    }

    [Fact]
    public void GetFullName_ShouldReturnConcatenatedName()
    {
        // Arrange
        var firstName = _fixture.Create<string>();
        var lastName = _fixture.Create<string>();
        var email = _fixture.Create<EmailAddress>();
        var userName = _fixture.Create<string>();
        var user = new User(firstName, lastName, email, userName);

        // Act
        var fullName = user.GetFullName();

        // Assert
        fullName.Should().Be($"{firstName} {lastName}");
    }

    [Fact]
    public void IsLockedOut_WhenLockoutEndIsInFuture_ShouldReturnTrue()
    {
        // Arrange
        var user = _fixture.Create<User>();
        user.LockAccount(DateTime.UtcNow.AddHours(1));

        // Act
        var isLockedOut = user.IsLockedOut;

        // Assert
        isLockedOut.Should().BeTrue();
    }

    [Fact]
    public void IsLockedOut_WhenLockoutEndIsInPast_ShouldReturnFalse()
    {
        // Arrange
        var user = _fixture.Create<User>();
        user.LockAccount(DateTime.UtcNow.AddHours(-1));

        // Act
        var isLockedOut = user.IsLockedOut;

        // Assert
        isLockedOut.Should().BeFalse();
    }

    [Fact]
    public void IsLockedOut_WhenLockoutDisabled_ShouldReturnFalse()
    {
        // Arrange
        var user = _fixture.Create<User>();
        user.SetLockoutEnabled(false);
        user.LockAccount(DateTime.UtcNow.AddHours(1));

        // Act
        var isLockedOut = user.IsLockedOut;

        // Assert
        isLockedOut.Should().BeFalse();
    }

    [Fact]
    public void RecordFailedLogin_ShouldIncrementAccessFailedCount()
    {
        // Arrange
        var user = _fixture.Create<User>();
        var initialCount = user.AccessFailedCount;

        // Act
        user.RecordFailedLogin();

        // Assert
        user.AccessFailedCount.Should().Be(initialCount + 1);
    }

    [Fact]
    public void ResetAccessFailedCount_ShouldSetCountToZero()
    {
        // Arrange
        var user = _fixture.Create<User>();
        user.RecordFailedLogin();
        user.RecordFailedLogin();

        // Act
        user.ResetAccessFailedCount();

        // Assert
        user.AccessFailedCount.Should().Be(0);
    }

    [Fact]
    public void LockAccount_ShouldSetLockoutEnd()
    {
        // Arrange
        var user = _fixture.Create<User>();
        var lockoutEnd = DateTime.UtcNow.AddHours(2);

        // Act
        user.LockAccount(lockoutEnd);

        // Assert
        user.LockoutEnd.Should().Be(lockoutEnd);
    }

    [Fact]
    public void UnlockAccount_ShouldClearLockoutAndResetFailedCount()
    {
        // Arrange
        var user = _fixture.Create<User>();
        user.LockAccount(DateTime.UtcNow.AddHours(1));
        user.RecordFailedLogin();
        user.RecordFailedLogin();

        // Act
        user.UnlockAccount();

        // Assert
        user.LockoutEnd.Should().BeNull();
        user.AccessFailedCount.Should().Be(0);
    }

    [Fact]
    public void SetLockoutEnabled_ShouldSetLockoutEnabled()
    {
        // Arrange
        var user = _fixture.Create<User>();

        // Act
        user.SetLockoutEnabled(false);

        // Assert
        user.LockoutEnabled.Should().BeFalse();
    }
}

using AutoFixture;
using AutoMapper;
using FluentAssertions;
using Sqordia.Application.Common.Mappings;
using Sqordia.Application.UnitTests.Common;
using Sqordia.Contracts.Responses.Auth;
using Sqordia.Domain.Entities.Identity;
using Sqordia.Domain.ValueObjects;
using Xunit;

namespace Sqordia.Application.UnitTests.Common.Mappings;

public class UserMappingProfileTests
{
    private readonly IFixture _fixture;
    private readonly IMapper _mapper;

    public UserMappingProfileTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _fixture.Customizations.Add(new EmailAddressSpecimenBuilder());

        // Setup AutoMapper
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<UserMappingProfile>();
        });
        _mapper = configuration.CreateMapper();
    }

    [Fact]
    public void Map_UserToUserDto_ShouldMapCorrectly()
    {
        // Arrange
        var user = new User("John", "Doe", new EmailAddress("john.doe@example.com"), "johndoe");

        // Act
        var userDto = _mapper.Map<UserDto>(user);

        // Assert
        userDto.Should().NotBeNull();
        userDto.Id.Should().Be(user.Id);
        userDto.FirstName.Should().Be(user.FirstName);
        userDto.LastName.Should().Be(user.LastName);
        userDto.Email.Should().Be(user.Email.Value);
        userDto.UserName.Should().Be(user.UserName);
        userDto.Roles.Should().NotBeNull();
    }

    [Fact]
    public void Map_UserToUserResponse_ShouldMapCorrectly()
    {
        // Arrange
        var user = new User("John", "Doe", new EmailAddress("john.doe@example.com"), "johndoe");

        // Act
        var userResponse = _mapper.Map<UserResponse>(user);

        // Assert
        userResponse.Should().NotBeNull();
        userResponse.Id.Should().Be(user.Id);
        userResponse.FirstName.Should().Be(user.FirstName);
        userResponse.LastName.Should().Be(user.LastName);
        userResponse.Email.Should().Be(user.Email.Value);
        userResponse.UserName.Should().Be(user.UserName);
        userResponse.IsEmailVerified.Should().Be(user.IsEmailConfirmed);
        userResponse.Roles.Should().NotBeNull();
    }

    [Fact]
    public void Map_UserWithRoles_ShouldMapRolesCorrectly()
    {
        // Arrange
        var role1 = new Role("Admin", "Administrator role");
        var role2 = new Role("User", "Regular user role");
        
        var user = new User("John", "Doe", new EmailAddress("john.doe@example.com"), "johndoe");
        var userRole1 = new UserRole(user.Id, role1.Id);
        var userRole2 = new UserRole(user.Id, role2.Id);
        
        // Set up the navigation properties manually for testing
        userRole1.GetType().GetProperty("Role")?.SetValue(userRole1, role1);
        userRole2.GetType().GetProperty("Role")?.SetValue(userRole2, role2);
        
        user.GetType().GetProperty("UserRoles")?.SetValue(user, new List<UserRole> { userRole1, userRole2 });

        // Act
        var userDto = _mapper.Map<UserDto>(user);

        // Assert
        userDto.Roles.Should().HaveCount(2);
        userDto.Roles.Should().Contain("Admin");
        userDto.Roles.Should().Contain("User");
    }

    [Fact]
    public void Map_UserWithNoRoles_ShouldMapEmptyRoles()
    {
        // Arrange
        var user = new User("John", "Doe", new EmailAddress("john.doe@example.com"), "johndoe");
        user.GetType().GetProperty("UserRoles")?.SetValue(user, new List<UserRole>());

        // Act
        var userDto = _mapper.Map<UserDto>(user);

        // Assert
        userDto.Roles.Should().NotBeNull();
        userDto.Roles.Should().BeEmpty();
    }

    [Fact]
    public void Map_UserWithNullRoles_ShouldMapEmptyRoles()
    {
        // Arrange
        var user = new User("John", "Doe", new EmailAddress("john.doe@example.com"), "johndoe");
        user.GetType().GetProperty("UserRoles")?.SetValue(user, null);

        // Act
        var userDto = _mapper.Map<UserDto>(user);

        // Assert
        userDto.Roles.Should().NotBeNull();
        userDto.Roles.Should().BeEmpty();
    }
}

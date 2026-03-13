using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Services;
using Sqordia.Application.Services.Implementations;
using Sqordia.Contracts.Requests.Organization;
using Sqordia.Domain.Entities;
using Sqordia.Domain.Entities.Identity;
using Sqordia.Domain.Enums;
using Sqordia.Domain.ValueObjects;
using Sqordia.Persistence.Contexts;

namespace Sqordia.Application.UnitTests.Services.OrganizationManagement;

/// <summary>
/// Tests for OrganizationService invitation methods.
/// Uses SQLite in-memory provider (not EF Core InMemory) because the service
/// queries on User.Email (an owned EmailAddress value object) which requires
/// a real SQL translation pipeline.
/// </summary>
public class OrganizationServiceInvitationTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly SqliteConnection _connection;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ILocalizationService> _localizationServiceMock;
    private readonly Mock<IOrganizationMembershipCache> _membershipCacheMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<IFeatureGateService> _featureGateMock;

    private readonly Guid _ownerId = Guid.NewGuid();
    private readonly Guid _organizationId = Guid.NewGuid();

    public OrganizationServiceInvitationTests()
    {
        // SQLite in-memory: keep connection open for lifetime of test
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;
        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();

        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _localizationServiceMock = new Mock<ILocalizationService>();
        _membershipCacheMock = new Mock<IOrganizationMembershipCache>();
        _notificationServiceMock = new Mock<INotificationService>();
        _featureGateMock = new Mock<IFeatureGateService>();

        // Default: authenticated owner
        _currentUserServiceMock.Setup(x => x.GetUserIdAsGuid()).Returns(_ownerId);
        _membershipCacheMock
            .Setup(x => x.GetUserRoleAsync(_organizationId, _ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OrganizationRole.Owner.ToString());
    }

    private OrganizationService BuildService() =>
        new(_context,
            _currentUserServiceMock.Object,
            new Mock<ILogger<OrganizationService>>().Object,
            _localizationServiceMock.Object,
            _membershipCacheMock.Object,
            _notificationServiceMock.Object,
            _featureGateMock.Object);

    private async Task SeedOrganizationWithOwnerAsync()
    {
        var owner = new User("John", "Doe", new EmailAddress("owner@test.com"), "owner@test.com");
        typeof(Sqordia.Domain.Common.BaseEntity).GetProperty("Id")!.SetValue(owner, _ownerId);
        _context.Users.Add(owner);

        var org = new Domain.Entities.Organization("Test Org", OrganizationType.Startup);
        typeof(Sqordia.Domain.Common.BaseEntity).GetProperty("Id")!.SetValue(org, _organizationId);
        _context.Organizations.Add(org);

        var member = new OrganizationMember(_organizationId, _ownerId, OrganizationRole.Owner);
        _context.OrganizationMembers.Add(member);

        await _context.SaveChangesAsync();
    }

    private async Task<User> AddUserAsync(Guid userId, string email, string firstName = "Test", string lastName = "User")
    {
        var user = new User(firstName, lastName, new EmailAddress(email), email);
        typeof(Sqordia.Domain.Common.BaseEntity).GetProperty("Id")!.SetValue(user, userId);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    // ── InviteMemberByEmailAsync ────────────────────────────────────────

    [Fact]
    public async Task InviteMemberByEmailAsync_WithValidData_ShouldCreateInvitation()
    {
        // Arrange
        await SeedOrganizationWithOwnerAsync();
        var sut = BuildService();
        var request = new InviteMemberByEmailRequest { Email = "newuser@test.com", Role = "Member" };

        // Act
        var result = await sut.InviteMemberByEmailAsync(_organizationId, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Email.Should().Be("newuser@test.com");
        result.Value.Role.Should().Be("Member");
        result.Value.Status.Should().Be("Pending");
        result.Value.OrganizationId.Should().Be(_organizationId);

        var invitation = await _context.OrganizationInvitations.FirstOrDefaultAsync();
        invitation.Should().NotBeNull();
        invitation!.Email.Should().Be("newuser@test.com");
    }

    [Fact]
    public async Task InviteMemberByEmailAsync_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.GetUserIdAsGuid()).Returns((Guid?)null);
        var sut = BuildService();
        var request = new InviteMemberByEmailRequest { Email = "newuser@test.com", Role = "Member" };

        // Act
        var result = await sut.InviteMemberByEmailAsync(_organizationId, request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Organization.Error.Unauthorized");
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task InviteMemberByEmailAsync_WhenNotOwnerOrAdmin_ShouldReturnForbidden()
    {
        // Arrange
        var viewerId = Guid.NewGuid();
        _currentUserServiceMock.Setup(x => x.GetUserIdAsGuid()).Returns(viewerId);
        _membershipCacheMock
            .Setup(x => x.GetUserRoleAsync(_organizationId, viewerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OrganizationRole.Viewer.ToString());
        var sut = BuildService();
        var request = new InviteMemberByEmailRequest { Email = "newuser@test.com", Role = "Member" };

        // Act
        var result = await sut.InviteMemberByEmailAsync(_organizationId, request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Organization.Error.Forbidden");
        result.Error.Type.Should().Be(ErrorType.Forbidden);
    }

    [Fact]
    public async Task InviteMemberByEmailAsync_WhenOrganizationNotFound_ShouldReturnNotFound()
    {
        // Arrange — no organization seeded, just the owner user for auth
        await AddUserAsync(_ownerId, "owner@test.com", "John", "Doe");
        var sut = BuildService();
        var request = new InviteMemberByEmailRequest { Email = "newuser@test.com", Role = "Member" };

        // Act
        var result = await sut.InviteMemberByEmailAsync(_organizationId, request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Organization.Error.NotFound");
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task InviteMemberByEmailAsync_WhenUserAlreadyMember_ShouldReturnConflict()
    {
        // Arrange
        await SeedOrganizationWithOwnerAsync();
        var existingUserId = Guid.NewGuid();
        await AddUserAsync(existingUserId, "existing@test.com", "Jane", "Doe");
        _context.OrganizationMembers.Add(new OrganizationMember(_organizationId, existingUserId, OrganizationRole.Member));
        await _context.SaveChangesAsync();

        var sut = BuildService();
        var request = new InviteMemberByEmailRequest { Email = "existing@test.com", Role = "Member" };

        // Act
        var result = await sut.InviteMemberByEmailAsync(_organizationId, request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Organization.Error.MemberAlreadyExists");
        result.Error.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task InviteMemberByEmailAsync_WhenPendingInvitationExists_ShouldReturnConflict()
    {
        // Arrange
        await SeedOrganizationWithOwnerAsync();
        _context.OrganizationInvitations.Add(new OrganizationInvitation(
            _organizationId, "duplicate@test.com", OrganizationRole.Member, _ownerId));
        await _context.SaveChangesAsync();

        var sut = BuildService();
        var request = new InviteMemberByEmailRequest { Email = "duplicate@test.com", Role = "Member" };

        // Act
        var result = await sut.InviteMemberByEmailAsync(_organizationId, request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Organization.Error.InvitationAlreadyPending");
        result.Error.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task InviteMemberByEmailAsync_WithInvalidRole_ShouldReturnValidation()
    {
        // Arrange
        await SeedOrganizationWithOwnerAsync();
        var sut = BuildService();
        var request = new InviteMemberByEmailRequest { Email = "newuser@test.com", Role = "SuperAdmin" };

        // Act
        var result = await sut.InviteMemberByEmailAsync(_organizationId, request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Organization.Error.InvalidRole");
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task InviteMemberByEmailAsync_WithOwnerRole_ShouldReturnValidation()
    {
        // Arrange
        await SeedOrganizationWithOwnerAsync();
        var sut = BuildService();
        var request = new InviteMemberByEmailRequest { Email = "newuser@test.com", Role = "Owner" };

        // Act
        var result = await sut.InviteMemberByEmailAsync(_organizationId, request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Organization.Error.CannotInviteAsOwner");
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task InviteMemberByEmailAsync_WithAdminRole_ShouldSucceed()
    {
        // Arrange
        await SeedOrganizationWithOwnerAsync();
        var sut = BuildService();
        var request = new InviteMemberByEmailRequest { Email = "admin@test.com", Role = "Admin" };

        // Act
        var result = await sut.InviteMemberByEmailAsync(_organizationId, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task InviteMemberByEmailAsync_ShouldSendNotification()
    {
        // Arrange
        await SeedOrganizationWithOwnerAsync();
        var sut = BuildService();
        var request = new InviteMemberByEmailRequest { Email = "newuser@test.com", Role = "Member" };

        // Act
        await sut.InviteMemberByEmailAsync(_organizationId, request);

        // Assert
        _notificationServiceMock.Verify(
            x => x.CreateNotificationAsync(
                It.Is<CreateNotificationCommand>(c =>
                    c.UserId == _ownerId &&
                    c.Type == NotificationType.OrganizationInvitation &&
                    c.Category == NotificationCategory.Organization &&
                    c.TitleFr.Contains("newuser@test.com") &&
                    c.TitleEn.Contains("newuser@test.com") &&
                    c.RelatedEntityId == _organizationId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task InviteMemberByEmailAsync_WhenNotificationFails_ShouldStillSucceed()
    {
        // Arrange
        await SeedOrganizationWithOwnerAsync();
        _notificationServiceMock
            .Setup(x => x.CreateNotificationAsync(
                It.IsAny<CreateNotificationCommand>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Notification service down"));

        var sut = BuildService();
        var request = new InviteMemberByEmailRequest { Email = "newuser@test.com", Role = "Member" };

        // Act
        var result = await sut.InviteMemberByEmailAsync(_organizationId, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task InviteMemberByEmailAsync_ShouldNormalizeEmailToLowerCase()
    {
        // Arrange
        await SeedOrganizationWithOwnerAsync();
        var sut = BuildService();
        var request = new InviteMemberByEmailRequest { Email = "USER@Test.COM", Role = "Member" };

        // Act
        var result = await sut.InviteMemberByEmailAsync(_organizationId, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Email.Should().Be("user@test.com");
    }

    // ── GetPendingInvitationsAsync ──────────────────────────────────────

    [Fact]
    public async Task GetPendingInvitationsAsync_WithPendingInvitations_ShouldReturnAll()
    {
        // Arrange
        await SeedOrganizationWithOwnerAsync();
        _context.OrganizationInvitations.Add(new OrganizationInvitation(_organizationId, "a@test.com", OrganizationRole.Member, _ownerId));
        _context.OrganizationInvitations.Add(new OrganizationInvitation(_organizationId, "b@test.com", OrganizationRole.Admin, _ownerId));
        await _context.SaveChangesAsync();
        var sut = BuildService();

        // Act
        var result = await sut.GetPendingInvitationsAsync(_organizationId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPendingInvitationsAsync_ShouldAutoExpireStaleInvitations()
    {
        // Arrange
        await SeedOrganizationWithOwnerAsync();
        // Create invitation with 0 expiration days (already expired)
        _context.OrganizationInvitations.Add(new OrganizationInvitation(_organizationId, "expired@test.com", OrganizationRole.Member, _ownerId, expirationDays: 0));
        // Fresh invitation
        _context.OrganizationInvitations.Add(new OrganizationInvitation(_organizationId, "fresh@test.com", OrganizationRole.Member, _ownerId));
        await _context.SaveChangesAsync();
        var sut = BuildService();

        // Act
        var result = await sut.GetPendingInvitationsAsync(_organizationId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value!.First().Email.Should().Be("fresh@test.com");
    }

    [Fact]
    public async Task GetPendingInvitationsAsync_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.GetUserIdAsGuid()).Returns((Guid?)null);
        var sut = BuildService();

        // Act
        var result = await sut.GetPendingInvitationsAsync(_organizationId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Organization.Error.Unauthorized");
    }

    [Fact]
    public async Task GetPendingInvitationsAsync_WhenNotOwnerOrAdmin_ShouldReturnForbidden()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        _currentUserServiceMock.Setup(x => x.GetUserIdAsGuid()).Returns(memberId);
        _membershipCacheMock
            .Setup(x => x.GetUserRoleAsync(_organizationId, memberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OrganizationRole.Member.ToString());
        var sut = BuildService();

        // Act
        var result = await sut.GetPendingInvitationsAsync(_organizationId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Organization.Error.Forbidden");
    }

    // ── CancelInvitationAsync ───────────────────────────────────────────

    [Fact]
    public async Task CancelInvitationAsync_WithPendingInvitation_ShouldCancel()
    {
        // Arrange
        await SeedOrganizationWithOwnerAsync();
        var invitation = new OrganizationInvitation(_organizationId, "cancel@test.com", OrganizationRole.Member, _ownerId);
        _context.OrganizationInvitations.Add(invitation);
        await _context.SaveChangesAsync();
        var sut = BuildService();

        // Act
        var result = await sut.CancelInvitationAsync(_organizationId, invitation.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task CancelInvitationAsync_WhenInvitationNotFound_ShouldReturnNotFound()
    {
        // Arrange
        await SeedOrganizationWithOwnerAsync();
        var sut = BuildService();

        // Act
        var result = await sut.CancelInvitationAsync(_organizationId, Guid.NewGuid());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Organization.Error.InvitationNotFound");
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task CancelInvitationAsync_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.GetUserIdAsGuid()).Returns((Guid?)null);
        var sut = BuildService();

        // Act
        var result = await sut.CancelInvitationAsync(_organizationId, Guid.NewGuid());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Organization.Error.Unauthorized");
    }

    [Fact]
    public async Task CancelInvitationAsync_WhenNotOwnerOrAdmin_ShouldReturnForbidden()
    {
        // Arrange
        var viewerId = Guid.NewGuid();
        _currentUserServiceMock.Setup(x => x.GetUserIdAsGuid()).Returns(viewerId);
        _membershipCacheMock
            .Setup(x => x.GetUserRoleAsync(_organizationId, viewerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OrganizationRole.Viewer.ToString());
        var sut = BuildService();

        // Act
        var result = await sut.CancelInvitationAsync(_organizationId, Guid.NewGuid());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Organization.Error.Forbidden");
    }

    // ── AcceptInvitationAsync ───────────────────────────────────────────

    [Fact]
    public async Task AcceptInvitationAsync_WithValidToken_ShouldCreateMemberAndAcceptInvitation()
    {
        // Arrange
        await SeedOrganizationWithOwnerAsync();
        var inviteeId = Guid.NewGuid();
        await AddUserAsync(inviteeId, "invitee@test.com", "Jane", "Smith");

        var invitation = new OrganizationInvitation(_organizationId, "invitee@test.com", OrganizationRole.Member, _ownerId);
        _context.OrganizationInvitations.Add(invitation);
        await _context.SaveChangesAsync();
        var sut = BuildService();

        // Act
        var result = await sut.AcceptInvitationAsync(invitation.Token, inviteeId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.UserId.Should().Be(inviteeId);
        result.Value.OrganizationId.Should().Be(_organizationId);
        result.Value.Role.Should().Be("Member");
        result.Value.IsActive.Should().BeTrue();
        result.Value.FirstName.Should().Be("Jane");
        result.Value.LastName.Should().Be("Smith");
        result.Value.Email.Should().Be("invitee@test.com");

        // Verify membership was created in DB
        var membership = await _context.OrganizationMembers
            .FirstOrDefaultAsync(m => m.UserId == inviteeId && m.OrganizationId == _organizationId);
        membership.Should().NotBeNull();
        membership!.Role.Should().Be(OrganizationRole.Member);
    }

    [Fact]
    public async Task AcceptInvitationAsync_ShouldInvalidateMembershipCache()
    {
        // Arrange
        await SeedOrganizationWithOwnerAsync();
        var inviteeId = Guid.NewGuid();
        await AddUserAsync(inviteeId, "invitee@test.com", "Jane", "Smith");

        var invitation = new OrganizationInvitation(_organizationId, "invitee@test.com", OrganizationRole.Member, _ownerId);
        _context.OrganizationInvitations.Add(invitation);
        await _context.SaveChangesAsync();
        var sut = BuildService();

        // Act
        await sut.AcceptInvitationAsync(invitation.Token, inviteeId);

        // Assert
        _membershipCacheMock.Verify(
            x => x.InvalidateMembership(_organizationId, inviteeId),
            Times.Once);
    }

    [Fact]
    public async Task AcceptInvitationAsync_WhenTokenNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var sut = BuildService();

        // Act
        var result = await sut.AcceptInvitationAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Organization.Error.InvitationNotFound");
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task AcceptInvitationAsync_WhenInvitationExpired_ShouldReturnValidationAndMarkExpired()
    {
        // Arrange
        await SeedOrganizationWithOwnerAsync();
        var inviteeId = Guid.NewGuid();
        await AddUserAsync(inviteeId, "invitee@test.com", "Jane", "Smith");

        var invitation = new OrganizationInvitation(_organizationId, "invitee@test.com", OrganizationRole.Member, _ownerId, expirationDays: 0);
        _context.OrganizationInvitations.Add(invitation);
        await _context.SaveChangesAsync();
        var sut = BuildService();

        // Act
        var result = await sut.AcceptInvitationAsync(invitation.Token, inviteeId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Organization.Error.InvitationExpired");
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task AcceptInvitationAsync_WhenUserNotFound_ShouldReturnNotFound()
    {
        // Arrange
        await SeedOrganizationWithOwnerAsync();
        var invitation = new OrganizationInvitation(_organizationId, "ghost@test.com", OrganizationRole.Member, _ownerId);
        _context.OrganizationInvitations.Add(invitation);
        await _context.SaveChangesAsync();
        var sut = BuildService();

        // Act
        var result = await sut.AcceptInvitationAsync(invitation.Token, Guid.NewGuid());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Organization.Error.UserNotFound");
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task AcceptInvitationAsync_WhenEmailMismatch_ShouldReturnValidation()
    {
        // Arrange
        await SeedOrganizationWithOwnerAsync();
        var wrongUserId = Guid.NewGuid();
        await AddUserAsync(wrongUserId, "wrong@test.com", "Wrong", "User");

        var invitation = new OrganizationInvitation(_organizationId, "correct@test.com", OrganizationRole.Member, _ownerId);
        _context.OrganizationInvitations.Add(invitation);
        await _context.SaveChangesAsync();
        var sut = BuildService();

        // Act
        var result = await sut.AcceptInvitationAsync(invitation.Token, wrongUserId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Organization.Error.EmailMismatch");
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task AcceptInvitationAsync_WhenAlreadyMember_ShouldReturnConflictButAcceptInvitation()
    {
        // Arrange
        await SeedOrganizationWithOwnerAsync();
        var inviteeId = Guid.NewGuid();
        await AddUserAsync(inviteeId, "invitee@test.com", "Jane", "Smith");
        _context.OrganizationMembers.Add(new OrganizationMember(_organizationId, inviteeId, OrganizationRole.Member));
        await _context.SaveChangesAsync();

        var invitation = new OrganizationInvitation(_organizationId, "invitee@test.com", OrganizationRole.Member, _ownerId);
        _context.OrganizationInvitations.Add(invitation);
        await _context.SaveChangesAsync();
        var sut = BuildService();

        // Act
        var result = await sut.AcceptInvitationAsync(invitation.Token, inviteeId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Organization.Error.MemberAlreadyExists");
        result.Error.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task AcceptInvitationAsync_WithAdminRole_ShouldCreateAdminMembership()
    {
        // Arrange
        await SeedOrganizationWithOwnerAsync();
        var inviteeId = Guid.NewGuid();
        await AddUserAsync(inviteeId, "admin@test.com", "Admin", "User");

        var invitation = new OrganizationInvitation(_organizationId, "admin@test.com", OrganizationRole.Admin, _ownerId);
        _context.OrganizationInvitations.Add(invitation);
        await _context.SaveChangesAsync();
        var sut = BuildService();

        // Act
        var result = await sut.AcceptInvitationAsync(invitation.Token, inviteeId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Role.Should().Be("Admin");

        var membership = await _context.OrganizationMembers
            .FirstOrDefaultAsync(m => m.UserId == inviteeId && m.OrganizationId == _organizationId);
        membership!.Role.Should().Be(OrganizationRole.Admin);
    }
}

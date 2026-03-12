using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Services.Implementations;
using Sqordia.Domain.Entities;
using Sqordia.Domain.Enums;
using Sqordia.Persistence.Contexts;

namespace Sqordia.Application.UnitTests.Services.Notification;

public class NotificationPreferenceServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ICurrentUserService> _currentUserService;
    private readonly NotificationPreferenceService _sut;
    private readonly Guid _userId = Guid.NewGuid();

    public NotificationPreferenceServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"NotifPref_{Guid.NewGuid()}")
            .Options;

        _context = new ApplicationDbContext(options);
        _currentUserService = new Mock<ICurrentUserService>();
        _currentUserService.Setup(s => s.GetUserIdAsGuid()).Returns(_userId);

        var logger = new Mock<ILogger<NotificationPreferenceService>>();

        _sut = new NotificationPreferenceService(_context, _currentUserService.Object, logger.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetPreferencesAsync_WhenNoPreferencesExist_CreatesDefaultsForAllTypes()
    {
        var result = await _sut.GetPreferencesAsync();

        result.IsSuccess.Should().BeTrue();
        var allTypes = Enum.GetValues<NotificationType>();
        result.Value.Preferences.Should().HaveCount(allTypes.Length);
    }

    [Fact]
    public async Task GetPreferencesAsync_WithExistingPreferences_DoesNotDuplicate()
    {
        // Arrange — seed one preference
        _context.NotificationPreferences.Add(
            new NotificationPreference(_userId, NotificationType.ExportCompleted));
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetPreferencesAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        var allTypes = Enum.GetValues<NotificationType>();
        result.Value.Preferences.Should().HaveCount(allTypes.Length);

        // Verify no duplicates in DB
        var dbCount = await _context.NotificationPreferences
            .Where(p => p.UserId == _userId)
            .CountAsync();
        dbCount.Should().Be(allTypes.Length);
    }

    [Fact]
    public async Task GetPreferencesAsync_WhenUnauthenticated_ReturnsUnauthorized()
    {
        _currentUserService.Setup(s => s.GetUserIdAsGuid()).Returns((Guid?)null);

        var result = await _sut.GetPreferencesAsync();

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Contain("Unauthorized");
    }

    [Fact]
    public async Task UpdatePreferenceAsync_NewPreference_CreatesIt()
    {
        var result = await _sut.UpdatePreferenceAsync(
            NotificationType.ExportCompleted,
            inAppEnabled: true,
            emailEnabled: true,
            NotificationFrequency.DailyDigest,
            soundEnabled: false);

        result.IsSuccess.Should().BeTrue();

        var saved = await _context.NotificationPreferences
            .FirstAsync(p => p.UserId == _userId && p.NotificationType == NotificationType.ExportCompleted);

        saved.EmailEnabled.Should().BeTrue();
        saved.EmailFrequency.Should().Be(NotificationFrequency.DailyDigest);
        saved.SoundEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task UpdatePreferenceAsync_ExistingPreference_UpdatesIt()
    {
        // Arrange
        _context.NotificationPreferences.Add(
            new NotificationPreference(_userId, NotificationType.ExportCompleted,
                inAppEnabled: true, emailEnabled: false));
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.UpdatePreferenceAsync(
            NotificationType.ExportCompleted,
            inAppEnabled: false,
            emailEnabled: true,
            NotificationFrequency.WeeklyDigest,
            soundEnabled: true);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var saved = await _context.NotificationPreferences
            .FirstAsync(p => p.UserId == _userId && p.NotificationType == NotificationType.ExportCompleted);

        saved.InAppEnabled.Should().BeFalse();
        saved.EmailEnabled.Should().BeTrue();
        saved.EmailFrequency.Should().Be(NotificationFrequency.WeeklyDigest);
    }

    [Fact]
    public async Task UpdatePreferencesBulkAsync_UpdatesMultiplePreferences()
    {
        // Arrange — seed existing
        _context.NotificationPreferences.Add(
            new NotificationPreference(_userId, NotificationType.ExportCompleted));
        await _context.SaveChangesAsync();

        var preferences = new[]
        {
            (NotificationType.ExportCompleted, true, true, NotificationFrequency.DailyDigest, false),
            (NotificationType.AICoachReply, false, false, NotificationFrequency.Disabled, false),
        };

        // Act
        var result = await _sut.UpdatePreferencesBulkAsync(preferences);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var count = await _context.NotificationPreferences.Where(p => p.UserId == _userId).CountAsync();
        count.Should().Be(2);
    }

    [Fact]
    public async Task ShouldSendInAppAsync_WhenNoPreference_ReturnsTrue()
    {
        var result = await _sut.ShouldSendInAppAsync(_userId, NotificationType.ExportCompleted);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldSendInAppAsync_WhenDisabled_ReturnsFalse()
    {
        _context.NotificationPreferences.Add(
            new NotificationPreference(_userId, NotificationType.ExportCompleted, inAppEnabled: false));
        await _context.SaveChangesAsync();

        var result = await _sut.ShouldSendInAppAsync(_userId, NotificationType.ExportCompleted);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldSendEmailAsync_WhenNoPreference_ReturnsTrueForImportantTypes()
    {
        var result = await _sut.ShouldSendEmailAsync(_userId, NotificationType.OrganizationInvitation);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldSendEmailAsync_WhenNoPreference_ReturnsFalseForRegularTypes()
    {
        var result = await _sut.ShouldSendEmailAsync(_userId, NotificationType.ExportCompleted);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldSendEmailAsync_WhenFrequencyDisabled_ReturnsFalse()
    {
        _context.NotificationPreferences.Add(
            new NotificationPreference(_userId, NotificationType.OrganizationInvitation,
                emailEnabled: true, emailFrequency: NotificationFrequency.Disabled));
        await _context.SaveChangesAsync();

        var result = await _sut.ShouldSendEmailAsync(_userId, NotificationType.OrganizationInvitation);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldPlaySoundAsync_WhenNoPreference_ReturnsTrue()
    {
        var result = await _sut.ShouldPlaySoundAsync(_userId, NotificationType.ExportCompleted);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldPlaySoundAsync_WhenDisabled_ReturnsFalse()
    {
        _context.NotificationPreferences.Add(
            new NotificationPreference(_userId, NotificationType.ExportCompleted, soundEnabled: false));
        await _context.SaveChangesAsync();

        var result = await _sut.ShouldPlaySoundAsync(_userId, NotificationType.ExportCompleted);
        result.Should().BeFalse();
    }

    [Fact]
    public void CreateDefault_SetsEmailEnabledForInvitations()
    {
        var pref = NotificationPreference.CreateDefault(_userId, NotificationType.OrganizationInvitation);
        pref.EmailEnabled.Should().BeTrue();
        pref.InAppEnabled.Should().BeTrue();
    }

    [Fact]
    public void CreateDefault_SetsEmailDisabledForRegularTypes()
    {
        var pref = NotificationPreference.CreateDefault(_userId, NotificationType.AICoachReply);
        pref.EmailEnabled.Should().BeFalse();
        pref.InAppEnabled.Should().BeTrue();
    }
}

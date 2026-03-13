using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Sqordia.Application.Services.Implementations;
using Sqordia.Domain.Entities;
using Sqordia.Domain.Enums;
using Sqordia.Persistence.Contexts;

namespace Sqordia.Application.UnitTests.Services.Notification;

public class NotificationAnalyticsServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly NotificationAnalyticsService _sut;

    public NotificationAnalyticsServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"NotifAnalytics_{Guid.NewGuid()}")
            .Options;

        _context = new ApplicationDbContext(options);
        var logger = new Mock<ILogger<NotificationAnalyticsService>>();

        _sut = new NotificationAnalyticsService(_context, logger.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private Domain.Entities.Notification CreateNotification(
        Guid? userId = null,
        NotificationType type = NotificationType.ExportCompleted,
        NotificationPriority priority = NotificationPriority.Normal,
        bool isRead = false,
        DateTime? created = null)
    {
        var n = new Domain.Entities.Notification(
            userId ?? Guid.NewGuid(),
            type,
            NotificationCategory.System,
            "Titre", "Title",
            "Message FR", "Message EN",
            priority: priority);

        if (created.HasValue)
        {
            n.Created = created.Value;
        }

        if (isRead)
        {
            n.MarkAsRead();
        }

        return n;
    }

    [Fact]
    public async Task GetAnalyticsAsync_WithNoNotifications_ReturnsZeros()
    {
        var result = await _sut.GetAnalyticsAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalSent.Should().Be(0);
        result.Value.TotalRead.Should().Be(0);
        result.Value.ReadRate.Should().Be(0);
        result.Value.ActiveUsersWithNotifications.Should().Be(0);
    }

    [Fact]
    public async Task GetAnalyticsAsync_CalculatesReadRateCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _context.Notifications.Add(CreateNotification(userId, isRead: true));
        _context.Notifications.Add(CreateNotification(userId, isRead: true));
        _context.Notifications.Add(CreateNotification(userId, isRead: false));
        _context.Notifications.Add(CreateNotification(userId, isRead: false));
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetAnalyticsAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalSent.Should().Be(4);
        result.Value.TotalRead.Should().Be(2);
        result.Value.ReadRate.Should().Be(50.0);
    }

    [Fact]
    public async Task GetAnalyticsAsync_GroupsByType()
    {
        var userId = Guid.NewGuid();
        _context.Notifications.Add(CreateNotification(userId, type: NotificationType.ExportCompleted));
        _context.Notifications.Add(CreateNotification(userId, type: NotificationType.ExportCompleted, isRead: true));
        _context.Notifications.Add(CreateNotification(userId, type: NotificationType.AICoachReply));
        await _context.SaveChangesAsync();

        var result = await _sut.GetAnalyticsAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.ByType.Should().HaveCount(2);

        var exportStats = result.Value.ByType.First(t => t.Type == "ExportCompleted");
        exportStats.Sent.Should().Be(2);
        exportStats.Read.Should().Be(1);
        exportStats.ReadRate.Should().Be(50.0);
    }

    [Fact]
    public async Task GetAnalyticsAsync_GroupsByPriority()
    {
        var userId = Guid.NewGuid();
        _context.Notifications.Add(CreateNotification(userId, priority: NotificationPriority.Normal));
        _context.Notifications.Add(CreateNotification(userId, priority: NotificationPriority.High));
        _context.Notifications.Add(CreateNotification(userId, priority: NotificationPriority.Urgent));
        await _context.SaveChangesAsync();

        var result = await _sut.GetAnalyticsAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.ByPriority.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAnalyticsAsync_CountsActiveUsers()
    {
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        _context.Notifications.Add(CreateNotification(user1));
        _context.Notifications.Add(CreateNotification(user1));
        _context.Notifications.Add(CreateNotification(user2));
        await _context.SaveChangesAsync();

        var result = await _sut.GetAnalyticsAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.ActiveUsersWithNotifications.Should().Be(2);
    }

    [Fact]
    public async Task GetAnalyticsAsync_RespectsLookbackWindow()
    {
        var userId = Guid.NewGuid();
        // One within window
        _context.Notifications.Add(CreateNotification(userId));
        // One outside 7-day window
        _context.Notifications.Add(CreateNotification(userId,
            created: DateTime.UtcNow.AddDays(-10)));
        await _context.SaveChangesAsync();

        var result = await _sut.GetAnalyticsAsync(days: 7);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalSent.Should().Be(1);
    }

    [Fact]
    public async Task GetAnalyticsAsync_CalculatesDailyTrend()
    {
        var userId = Guid.NewGuid();
        var today = DateTime.UtcNow;
        var yesterday = DateTime.UtcNow.AddDays(-1);

        _context.Notifications.Add(CreateNotification(userId, created: today));
        _context.Notifications.Add(CreateNotification(userId, created: today));
        _context.Notifications.Add(CreateNotification(userId, created: yesterday));
        await _context.SaveChangesAsync();

        var result = await _sut.GetAnalyticsAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.DailyTrend.Should().HaveCount(2);
        result.Value.DailyTrend.Should().BeInAscendingOrder(d => d.Date);
    }

    [Fact]
    public async Task GetAnalyticsAsync_IncludesSoftDeletedNotifications()
    {
        var userId = Guid.NewGuid();
        var notification = CreateNotification(userId);
        notification.SoftDelete(); // Soft delete it
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        var result = await _sut.GetAnalyticsAsync();

        // Should still be counted (IgnoreQueryFilters)
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalSent.Should().Be(1);
    }
}

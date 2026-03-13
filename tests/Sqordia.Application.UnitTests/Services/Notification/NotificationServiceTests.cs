using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Services;
using Sqordia.Application.Services.Implementations;
using Sqordia.Domain.Entities;
using Sqordia.Domain.Entities.Identity;
using Sqordia.Domain.Enums;
using Sqordia.Domain.ValueObjects;
using Sqordia.Persistence.Contexts;

namespace Sqordia.Application.UnitTests.Services.Notification;

public class NotificationServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ICurrentUserService> _currentUserService;
    private readonly Mock<INotificationHubService> _hubService;
    private readonly Mock<INotificationPreferenceService> _preferenceService;
    private readonly Mock<IEmailService> _emailService;
    private readonly NotificationService _sut;
    private readonly Guid _userId = Guid.NewGuid();

    public NotificationServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"NotifSvc_{Guid.NewGuid()}")
            .Options;

        _context = new ApplicationDbContext(options);
        _currentUserService = new Mock<ICurrentUserService>();
        _currentUserService.Setup(s => s.GetUserIdAsGuid()).Returns(_userId);

        _hubService = new Mock<INotificationHubService>();
        _preferenceService = new Mock<INotificationPreferenceService>();
        _emailService = new Mock<IEmailService>();

        // Default: allow in-app notifications
        _preferenceService
            .Setup(s => s.ShouldSendInAppAsync(It.IsAny<Guid>(), It.IsAny<NotificationType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _preferenceService
            .Setup(s => s.ShouldSendEmailAsync(It.IsAny<Guid>(), It.IsAny<NotificationType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _preferenceService
            .Setup(s => s.GetUsersWithInAppDisabledAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<NotificationType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<Guid>());

        var logger = new Mock<ILogger<NotificationService>>();

        _sut = new NotificationService(
            _context, _currentUserService.Object, _hubService.Object,
            _preferenceService.Object, _emailService.Object, logger.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    // ── Create ──────────────────────────────────────────

    [Fact]
    public async Task CreateNotificationAsync_WithValidData_CreatesNotification()
    {
        var result = await _sut.CreateNotificationAsync(
            new CreateNotificationCommand(
                _userId,
                NotificationType.ExportCompleted,
                NotificationCategory.BusinessPlan,
                "Export terminé", "Export completed",
                "Votre export est prêt", "Your export is ready"));

        // Allow fire-and-forget SignalR push to complete (avoids DbContext concurrency)
        await Task.Delay(150);

        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be(NotificationType.ExportCompleted);

        var count = await _context.Notifications.CountAsync();
        count.Should().Be(1);
    }

    [Fact]
    public async Task CreateNotificationAsync_WithPriority_SetsPriorityCorrectly()
    {
        var result = await _sut.CreateNotificationAsync(
            new CreateNotificationCommand(
                _userId,
                NotificationType.SubscriptionExpiring,
                NotificationCategory.System,
                "Abonnement", "Subscription",
                "Expire bientôt", "Expiring soon",
                Priority: NotificationPriority.High));

        await Task.Delay(150);
        result.IsSuccess.Should().BeTrue();
        result.Value.Priority.Should().Be(NotificationPriority.High);
    }

    [Fact]
    public async Task CreateNotificationAsync_WhenInAppDisabled_SkipsCreation()
    {
        _preferenceService
            .Setup(s => s.ShouldSendInAppAsync(_userId, NotificationType.ExportCompleted, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _sut.CreateNotificationAsync(
            new CreateNotificationCommand(
                _userId,
                NotificationType.ExportCompleted,
                NotificationCategory.BusinessPlan,
                "Export", "Export",
                "Message FR", "Message EN"));

        result.IsSuccess.Should().BeTrue();
        var count = await _context.Notifications.CountAsync();
        count.Should().Be(0); // Not persisted
    }

    [Fact]
    public async Task CreateNotificationAsync_WithGroupKey_BatchesDuplicates()
    {
        // First notification
        await _sut.CreateNotificationAsync(
            new CreateNotificationCommand(
                _userId,
                NotificationType.AICoachReply,
                NotificationCategory.AI,
                "Coach", "Coach",
                "Message 1", "Message 1",
                GroupKey: "ai-coach-123"));

        // Allow fire-and-forget SignalR push to complete (avoids DbContext concurrency)
        await Task.Delay(150);

        // Second notification with same group key (within 2-min window)
        var result = await _sut.CreateNotificationAsync(
            new CreateNotificationCommand(
                _userId,
                NotificationType.AICoachReply,
                NotificationCategory.AI,
                "Coach", "Coach",
                "Message 2", "Message 2",
                GroupKey: "ai-coach-123"));

        result.IsSuccess.Should().BeTrue();

        // Should only have 1 notification (second was batched)
        var count = await _context.Notifications.CountAsync();
        count.Should().Be(1);
    }

    // ── Get ──────────────────────────────────────────

    [Fact]
    public async Task GetNotificationsAsync_ReturnsUserNotifications()
    {
        _context.Notifications.Add(new Domain.Entities.Notification(
            _userId, NotificationType.ExportCompleted, NotificationCategory.BusinessPlan,
            "Titre", "Title", "Msg FR", "Msg EN"));
        _context.Notifications.Add(new Domain.Entities.Notification(
            Guid.NewGuid(), NotificationType.ExportCompleted, NotificationCategory.BusinessPlan,
            "Other", "Other", "Other", "Other")); // Different user
        await _context.SaveChangesAsync();

        var result = await _sut.GetNotificationsAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetNotificationsAsync_FiltersByReadStatus()
    {
        var n1 = new Domain.Entities.Notification(
            _userId, NotificationType.ExportCompleted, NotificationCategory.BusinessPlan,
            "Read", "Read", "Read", "Read");
        n1.MarkAsRead();
        _context.Notifications.Add(n1);
        _context.Notifications.Add(new Domain.Entities.Notification(
            _userId, NotificationType.AICoachReply, NotificationCategory.AI,
            "Unread", "Unread", "Unread", "Unread"));
        await _context.SaveChangesAsync();

        var unreadResult = await _sut.GetNotificationsAsync(isRead: false);
        unreadResult.Value.Items.Should().HaveCount(1);
        unreadResult.Value.Items[0].TitleEn.Should().Be("Unread");

        var readResult = await _sut.GetNotificationsAsync(isRead: true);
        readResult.Value.Items.Should().HaveCount(1);
        readResult.Value.Items[0].TitleEn.Should().Be("Read");
    }

    [Fact]
    public async Task GetNotificationsAsync_SortsByPriorityThenDate()
    {
        _context.Notifications.Add(new Domain.Entities.Notification(
            _userId, NotificationType.ExportCompleted, NotificationCategory.BusinessPlan,
            "Normal", "Normal", "Msg", "Msg", priority: NotificationPriority.Normal));
        _context.Notifications.Add(new Domain.Entities.Notification(
            _userId, NotificationType.SubscriptionExpiring, NotificationCategory.System,
            "Urgent", "Urgent", "Msg", "Msg", priority: NotificationPriority.Urgent));
        await _context.SaveChangesAsync();

        var result = await _sut.GetNotificationsAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Items[0].Priority.Should().Be(NotificationPriority.Urgent);
        result.Value.Items[1].Priority.Should().Be(NotificationPriority.Normal);
    }

    [Fact]
    public async Task GetUnreadCountAsync_ReturnsCorrectCount()
    {
        var n1 = new Domain.Entities.Notification(
            _userId, NotificationType.ExportCompleted, NotificationCategory.BusinessPlan,
            "Read", "Read", "Read", "Read");
        n1.MarkAsRead();
        _context.Notifications.Add(n1);
        _context.Notifications.Add(new Domain.Entities.Notification(
            _userId, NotificationType.AICoachReply, NotificationCategory.AI,
            "Unread", "Unread", "Unread", "Unread"));
        _context.Notifications.Add(new Domain.Entities.Notification(
            _userId, NotificationType.SystemAnnouncement, NotificationCategory.System,
            "Unread2", "Unread2", "Unread2", "Unread2"));
        await _context.SaveChangesAsync();

        var result = await _sut.GetUnreadCountAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Count.Should().Be(2);
    }

    [Fact]
    public async Task GetUnreadCountAsync_WhenUnauthenticated_ReturnsUnauthorized()
    {
        _currentUserService.Setup(s => s.GetUserIdAsGuid()).Returns((Guid?)null);

        var result = await _sut.GetUnreadCountAsync();

        result.IsSuccess.Should().BeFalse();
    }

    // ── Mark As Read ──────────────────────────────────────────

    [Fact]
    public async Task MarkAsReadAsync_MarksNotificationAsRead()
    {
        var notification = new Domain.Entities.Notification(
            _userId, NotificationType.ExportCompleted, NotificationCategory.BusinessPlan,
            "Titre", "Title", "Msg FR", "Msg EN");
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        var result = await _sut.MarkAsReadAsync(notification.Id);

        result.IsSuccess.Should().BeTrue();
        var updated = await _context.Notifications.FindAsync(notification.Id);
        updated!.IsRead.Should().BeTrue();
        updated.ReadAt.Should().NotBeNull();
    }

    [Fact]
    public async Task MarkAsReadAsync_WhenNotFound_ReturnsNotFoundError()
    {
        var result = await _sut.MarkAsReadAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Contain("NotFound");
    }

    [Fact]
    public async Task MarkAsReadAsync_WhenDifferentUser_ReturnsForbidden()
    {
        var notification = new Domain.Entities.Notification(
            Guid.NewGuid(), // Different user
            NotificationType.ExportCompleted, NotificationCategory.BusinessPlan,
            "Titre", "Title", "Msg FR", "Msg EN");
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        var result = await _sut.MarkAsReadAsync(notification.Id);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Contain("Forbidden");
    }

    // ── Delete ──────────────────────────────────────────

    [Fact]
    public async Task DeleteNotificationAsync_SoftDeletesNotification()
    {
        var notification = new Domain.Entities.Notification(
            _userId, NotificationType.ExportCompleted, NotificationCategory.BusinessPlan,
            "Titre", "Title", "Msg FR", "Msg EN");
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        var result = await _sut.DeleteNotificationAsync(notification.Id);

        result.IsSuccess.Should().BeTrue();

        // Soft-deleted: not returned by default query filter
        var found = await _context.Notifications.FindAsync(notification.Id);
        // InMemory doesn't apply query filters, so check the flag directly
        found!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteNotificationAsync_WhenNotFound_ReturnsNotFoundError()
    {
        var result = await _sut.DeleteNotificationAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
    }

    // ── Bulk ──────────────────────────────────────────

    [Fact]
    public async Task CreateBulkNotificationsAsync_CreatesForAllUsers()
    {
        var userIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        var result = await _sut.CreateBulkNotificationsAsync(
            userIds,
            new CreateNotificationCommand(
                Guid.Empty,
                NotificationType.SystemAnnouncement,
                NotificationCategory.System,
                "Annonce", "Announcement",
                "Message FR", "Message EN"));

        result.IsSuccess.Should().BeTrue();
        var count = await _context.Notifications.CountAsync();
        count.Should().Be(3);
    }

    // ── System Announcement ──────────────────────────────────────────

    [Fact]
    public async Task CreateSystemAnnouncementAsync_CreatesForAllActiveUsers()
    {
        // Seed users
        var user1 = new User("User", "One", new EmailAddress("user1@test.com"), "user1");
        var user2 = new User("User", "Two", new EmailAddress("user2@test.com"), "user2");
        var deletedUser = new User("Deleted", "User", new EmailAddress("deleted@test.com"), "deleted");
        deletedUser.SoftDelete();

        _context.Users.AddRange(user1, user2, deletedUser);
        await _context.SaveChangesAsync();

        var result = await _sut.CreateSystemAnnouncementAsync(
            "Annonce système", "System announcement",
            "Contenu FR", "Content EN",
            NotificationPriority.High);

        result.IsSuccess.Should().BeTrue();

        // Only non-deleted users should get notifications
        var count = await _context.Notifications.CountAsync();
        count.Should().Be(2);
    }
}

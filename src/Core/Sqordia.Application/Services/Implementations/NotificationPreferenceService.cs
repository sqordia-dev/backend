using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Responses.Notification;
using Sqordia.Domain.Entities;
using Sqordia.Domain.Enums;

namespace Sqordia.Application.Services.Implementations;

public class NotificationPreferenceService : INotificationPreferenceService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<NotificationPreferenceService> _logger;

    public NotificationPreferenceService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<NotificationPreferenceService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<NotificationPreferencesListResponse>> GetPreferencesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _currentUserService.GetUserIdAsGuid();
            if (userId == null)
                return Result.Failure<NotificationPreferencesListResponse>(
                    Error.Unauthorized("NotificationPreference.Unauthorized", "Authentication required"));

            var existing = await _context.NotificationPreferences
                .Where(p => p.UserId == userId.Value)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Ensure all notification types have preferences (create defaults for missing)
            var allTypes = Enum.GetValues<NotificationType>();
            var existingTypes = existing.Select(p => p.NotificationType).ToHashSet();
            var missing = allTypes.Where(t => !existingTypes.Contains(t)).ToList();

            if (missing.Count > 0)
            {
                var defaults = missing.Select(t => NotificationPreference.CreateDefault(userId.Value, t)).ToList();
                _context.NotificationPreferences.AddRange(defaults);
                await _context.SaveChangesAsync(cancellationToken);
                existing.AddRange(defaults);
            }

            var response = new NotificationPreferencesListResponse
            {
                Preferences = existing.Select(p => new NotificationPreferenceResponse
                {
                    Id = p.Id,
                    NotificationType = p.NotificationType.ToString(),
                    InAppEnabled = p.InAppEnabled,
                    EmailEnabled = p.EmailEnabled,
                    EmailFrequency = p.EmailFrequency.ToString(),
                    SoundEnabled = p.SoundEnabled
                }).ToList()
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notification preferences");
            return Result.Failure<NotificationPreferencesListResponse>(
                Error.Failure("NotificationPreference.Error.GetFailed", "Failed to retrieve preferences"));
        }
    }

    public async Task<Result> UpdatePreferenceAsync(
        NotificationType type,
        bool inAppEnabled,
        bool emailEnabled,
        NotificationFrequency emailFrequency,
        bool soundEnabled,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _currentUserService.GetUserIdAsGuid();
            if (userId == null)
                return Result.Failure(Error.Unauthorized("NotificationPreference.Unauthorized", "Authentication required"));

            var preference = await _context.NotificationPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId.Value && p.NotificationType == type, cancellationToken);

            if (preference == null)
            {
                preference = new NotificationPreference(userId.Value, type, inAppEnabled, emailEnabled, emailFrequency, soundEnabled);
                _context.NotificationPreferences.Add(preference);
            }
            else
            {
                preference.Update(inAppEnabled, emailEnabled, emailFrequency, soundEnabled);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating notification preference for type {Type}", type);
            return Result.Failure(Error.Failure("NotificationPreference.Error.UpdateFailed", "Failed to update preference"));
        }
    }

    public async Task<Result> UpdatePreferencesBulkAsync(
        IEnumerable<(NotificationType Type, bool InAppEnabled, bool EmailEnabled, NotificationFrequency EmailFrequency, bool SoundEnabled)> preferences,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _currentUserService.GetUserIdAsGuid();
            if (userId == null)
                return Result.Failure(Error.Unauthorized("NotificationPreference.Unauthorized", "Authentication required"));

            var existing = await _context.NotificationPreferences
                .Where(p => p.UserId == userId.Value)
                .ToListAsync(cancellationToken);

            var existingDict = existing.ToDictionary(p => p.NotificationType);

            foreach (var pref in preferences)
            {
                if (existingDict.TryGetValue(pref.Type, out var existingPref))
                {
                    existingPref.Update(pref.InAppEnabled, pref.EmailEnabled, pref.EmailFrequency, pref.SoundEnabled);
                }
                else
                {
                    var newPref = new NotificationPreference(userId.Value, pref.Type, pref.InAppEnabled, pref.EmailEnabled, pref.EmailFrequency, pref.SoundEnabled);
                    _context.NotificationPreferences.Add(newPref);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating notification preferences in bulk");
            return Result.Failure(Error.Failure("NotificationPreference.Error.BulkUpdateFailed", "Failed to update preferences"));
        }
    }

    private async Task<NotificationPreference?> GetPreferenceAsync(
        Guid userId, NotificationType type, CancellationToken cancellationToken)
    {
        return await _context.NotificationPreferences
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId && p.NotificationType == type, cancellationToken);
    }

    public async Task<bool> ShouldSendInAppAsync(Guid userId, NotificationType type, CancellationToken cancellationToken = default)
    {
        var preference = await GetPreferenceAsync(userId, type, cancellationToken);
        return preference?.InAppEnabled ?? true;
    }

    public async Task<bool> ShouldSendEmailAsync(Guid userId, NotificationType type, CancellationToken cancellationToken = default)
    {
        var preference = await GetPreferenceAsync(userId, type, cancellationToken);

        if (preference == null)
        {
            return type is NotificationType.OrganizationInvitation
                or NotificationType.BusinessPlanShared
                or NotificationType.SubscriptionExpiring;
        }

        return preference.EmailEnabled && preference.EmailFrequency != NotificationFrequency.Disabled;
    }

    public async Task<bool> ShouldPlaySoundAsync(Guid userId, NotificationType type, CancellationToken cancellationToken = default)
    {
        var preference = await GetPreferenceAsync(userId, type, cancellationToken);
        return preference?.SoundEnabled ?? true;
    }

    public async Task<HashSet<Guid>> GetUsersWithInAppDisabledAsync(
        IReadOnlyList<Guid> userIds,
        NotificationType type,
        CancellationToken cancellationToken = default)
    {
        var disabledUserIds = await _context.NotificationPreferences
            .AsNoTracking()
            .Where(p => userIds.Contains(p.UserId)
                && p.NotificationType == type
                && !p.InAppEnabled)
            .Select(p => p.UserId)
            .ToListAsync(cancellationToken);

        return disabledUserIds.ToHashSet();
    }
}

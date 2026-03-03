using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Models;
using Sqordia.Application.Services;
using Sqordia.Contracts.Requests.Settings;
using Sqordia.Contracts.Responses.Settings;
using Sqordia.Domain.Entities;
using Sqordia.Domain.Enums;

namespace Sqordia.Application.Services.Implementations;

/// <summary>
/// Service for managing feature flags with full metadata support
/// </summary>
public class FeatureFlagsService : IFeatureFlagsService
{
    private const string FeatureKeyPrefix = "Features:";
    private const int DefaultCacheDurationMinutes = 5;
    private const int StaleDaysThreshold = 30;
    private const int VeryStaleThreshold = 60;

    private readonly ISettingsService _settingsService;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<FeatureFlagsService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public FeatureFlagsService(
        ISettingsService settingsService,
        IApplicationDbContext context,
        ILogger<FeatureFlagsService> logger)
    {
        _settingsService = settingsService;
        _context = context;
        _logger = logger;
    }

    public async Task<Result<bool>> IsEnabledAsync(string featureName, CancellationToken cancellationToken = default)
    {
        try
        {
            var featureKey = GetFeatureKey(featureName);
            var setting = await _context.Settings
                .FirstOrDefaultAsync(s => s.Key == featureKey && !s.IsDeleted, cancellationToken);

            if (setting == null)
            {
                return Result.Success(false); // Default to disabled
            }

            // Try to parse as JSON metadata first
            var metadata = TryParseMetadata(setting.Value);
            if (metadata != null)
            {
                return Result.Success(metadata.IsEnabled);
            }

            // Fallback to simple boolean parsing
            return Result.Success(bool.TryParse(setting.Value, out var result) && result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking feature flag '{FeatureName}'", featureName);
            return Result.Success(false); // Default to disabled on error
        }
    }

    public async Task<Result> EnableFeatureAsync(string featureName, CancellationToken cancellationToken = default)
    {
        try
        {
            var featureKey = GetFeatureKey(featureName);
            var setting = await _context.Settings
                .FirstOrDefaultAsync(s => s.Key == featureKey && !s.IsDeleted, cancellationToken);

            if (setting == null)
            {
                // Create new feature with default metadata
                var metadata = FeatureFlagMetadata.CreateDefault(true);
                return await SaveFeatureAsync(featureName, metadata, cancellationToken);
            }

            // Update existing feature
            var existingMetadata = TryParseMetadata(setting.Value) ?? FeatureFlagMetadata.CreateDefault(true);
            existingMetadata.IsEnabled = true;

            return await SaveFeatureAsync(featureName, existingMetadata, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling feature '{FeatureName}'", featureName);
            return Result.Failure(Error.Failure("FeatureFlags.Error", "An error occurred while enabling the feature"));
        }
    }

    public async Task<Result> DisableFeatureAsync(string featureName, CancellationToken cancellationToken = default)
    {
        try
        {
            var featureKey = GetFeatureKey(featureName);
            var setting = await _context.Settings
                .FirstOrDefaultAsync(s => s.Key == featureKey && !s.IsDeleted, cancellationToken);

            if (setting == null)
            {
                // Create new feature with default metadata (disabled)
                var metadata = FeatureFlagMetadata.CreateDefault(false);
                return await SaveFeatureAsync(featureName, metadata, cancellationToken);
            }

            // Update existing feature
            var existingMetadata = TryParseMetadata(setting.Value) ?? FeatureFlagMetadata.CreateDefault(false);
            existingMetadata.IsEnabled = false;

            return await SaveFeatureAsync(featureName, existingMetadata, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling feature '{FeatureName}'", featureName);
            return Result.Failure(Error.Failure("FeatureFlags.Error", "An error occurred while disabling the feature"));
        }
    }

    public async Task<Result<Dictionary<string, bool>>> GetAllFeaturesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var settings = await _context.Settings
                .Where(s => s.Category == "Features" && !s.IsDeleted)
                .ToListAsync(cancellationToken);

            var features = new Dictionary<string, bool>();
            foreach (var setting in settings)
            {
                var featureName = setting.Key.Replace(FeatureKeyPrefix, "", StringComparison.OrdinalIgnoreCase);
                var metadata = TryParseMetadata(setting.Value);

                if (metadata != null)
                {
                    features[featureName] = metadata.IsEnabled;
                }
                else if (bool.TryParse(setting.Value, out var isEnabled))
                {
                    features[featureName] = isEnabled;
                }
            }

            return Result.Success(features);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all feature flags");
            return Result.Failure<Dictionary<string, bool>>(Error.Failure("FeatureFlags.Error", "An error occurred while retrieving feature flags"));
        }
    }

    public async Task<Result<FeatureFlagListResponse>> GetAllFeaturesDetailedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var settings = await _context.Settings
                .Where(s => s.Category == "Features" && !s.IsDeleted)
                .OrderBy(s => s.Key)
                .ToListAsync(cancellationToken);

            var flags = new List<FeatureFlagResponse>();

            foreach (var setting in settings)
            {
                var flag = MapSettingToResponse(setting);
                flags.Add(flag);
            }

            var response = new FeatureFlagListResponse
            {
                Flags = flags,
                TotalCount = flags.Count,
                EnabledCount = flags.Count(f => f.IsEnabled),
                DisabledCount = flags.Count(f => !f.IsEnabled),
                StaleCount = flags.Count(f => f.State == FeatureFlagState.Stale || f.State == FeatureFlagState.PotentiallyStale)
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving detailed feature flags");
            return Result.Failure<FeatureFlagListResponse>(Error.Failure("FeatureFlags.Error", "An error occurred while retrieving feature flags"));
        }
    }

    public async Task<Result<FeatureFlagResponse>> GetFeatureDetailedAsync(string featureName, CancellationToken cancellationToken = default)
    {
        try
        {
            var featureKey = GetFeatureKey(featureName);
            var setting = await _context.Settings
                .FirstOrDefaultAsync(s => s.Key == featureKey && !s.IsDeleted, cancellationToken);

            if (setting == null)
            {
                return Result.Failure<FeatureFlagResponse>(Error.NotFound("FeatureFlags.NotFound", $"Feature flag '{featureName}' not found"));
            }

            return Result.Success(MapSettingToResponse(setting));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving feature flag '{FeatureName}'", featureName);
            return Result.Failure<FeatureFlagResponse>(Error.Failure("FeatureFlags.Error", "An error occurred while retrieving the feature flag"));
        }
    }

    public async Task<Result<FeatureFlagResponse>> CreateFeatureAsync(CreateFeatureFlagRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var featureKey = GetFeatureKey(request.Name);

            // Check if feature already exists
            var existing = await _context.Settings
                .FirstOrDefaultAsync(s => s.Key == featureKey && !s.IsDeleted, cancellationToken);

            if (existing != null)
            {
                return Result.Failure<FeatureFlagResponse>(Error.Conflict("FeatureFlags.AlreadyExists", $"Feature flag '{request.Name}' already exists"));
            }

            // Validate temporary flag has expiration
            if (request.Type == FeatureFlagType.Temporary && !request.ExpiresAt.HasValue)
            {
                return Result.Failure<FeatureFlagResponse>(Error.Validation("FeatureFlags.ExpirationRequired", "Temporary feature flags must have an expiration date"));
            }

            var metadata = new FeatureFlagMetadata
            {
                IsEnabled = request.IsEnabled,
                Description = request.Description,
                Category = request.Category,
                Tags = request.Tags,
                Type = request.Type,
                State = FeatureFlagState.Active,
                ExpiresAt = request.ExpiresAt
            };

            var result = await SaveFeatureAsync(request.Name, metadata, cancellationToken);
            if (!result.IsSuccess)
            {
                return Result.Failure<FeatureFlagResponse>(result.Error!);
            }

            return await GetFeatureDetailedAsync(request.Name, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating feature flag '{FeatureName}'", request.Name);
            return Result.Failure<FeatureFlagResponse>(Error.Failure("FeatureFlags.Error", "An error occurred while creating the feature flag"));
        }
    }

    public async Task<Result<FeatureFlagResponse>> UpdateFeatureMetadataAsync(string featureName, UpdateFeatureFlagRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var featureKey = GetFeatureKey(featureName);
            var setting = await _context.Settings
                .FirstOrDefaultAsync(s => s.Key == featureKey && !s.IsDeleted, cancellationToken);

            if (setting == null)
            {
                return Result.Failure<FeatureFlagResponse>(Error.NotFound("FeatureFlags.NotFound", $"Feature flag '{featureName}' not found"));
            }

            var metadata = TryParseMetadata(setting.Value) ?? FeatureFlagMetadata.CreateDefault();

            // Update only provided fields
            if (request.Description != null)
                metadata.Description = request.Description;
            if (request.Category != null)
                metadata.Category = request.Category;
            if (request.Tags != null)
                metadata.Tags = request.Tags;
            if (request.Type.HasValue)
                metadata.Type = request.Type.Value;
            if (request.State.HasValue)
                metadata.State = request.State.Value;
            if (request.ExpiresAt.HasValue)
                metadata.ExpiresAt = request.ExpiresAt;

            var result = await SaveFeatureAsync(featureName, metadata, cancellationToken);
            if (!result.IsSuccess)
            {
                return Result.Failure<FeatureFlagResponse>(result.Error!);
            }

            return await GetFeatureDetailedAsync(featureName, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating feature flag '{FeatureName}'", featureName);
            return Result.Failure<FeatureFlagResponse>(Error.Failure("FeatureFlags.Error", "An error occurred while updating the feature flag"));
        }
    }

    public async Task<Result> MarkAsStaleAsync(string featureName, CancellationToken cancellationToken = default)
    {
        var updateRequest = new UpdateFeatureFlagRequest { State = FeatureFlagState.Stale };
        var result = await UpdateFeatureMetadataAsync(featureName, updateRequest, cancellationToken);

        if (!result.IsSuccess)
        {
            return Result.Failure(result.Error!);
        }

        _logger.LogWarning("Feature flag '{FeatureName}' marked as stale", featureName);
        return Result.Success();
    }

    public async Task<Result> ArchiveFeatureAsync(string featureName, CancellationToken cancellationToken = default)
    {
        try
        {
            var featureKey = GetFeatureKey(featureName);
            var result = await _settingsService.DeleteSettingAsync(featureKey, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogWarning("Feature flag '{FeatureName}' archived", featureName);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving feature flag '{FeatureName}'", featureName);
            return Result.Failure(Error.Failure("FeatureFlags.Error", "An error occurred while archiving the feature flag"));
        }
    }

    private async Task<Result> SaveFeatureAsync(string featureName, FeatureFlagMetadata metadata, CancellationToken cancellationToken)
    {
        var featureKey = GetFeatureKey(featureName);
        var json = JsonSerializer.Serialize(metadata, JsonOptions);

        return await _settingsService.UpsertSettingAsync(
            featureKey,
            json,
            "Features",
            metadata.Description ?? $"Feature flag for {featureName}",
            isPublic: true,
            settingType: SettingType.FeatureFlag,
            dataType: SettingDataType.Json,
            encrypt: false,
            cacheDurationMinutes: DefaultCacheDurationMinutes,
            isCritical: false,
            cancellationToken);
    }

    private FeatureFlagResponse MapSettingToResponse(Settings setting)
    {
        var featureName = setting.Key.Replace(FeatureKeyPrefix, "", StringComparison.OrdinalIgnoreCase);
        var metadata = TryParseMetadata(setting.Value);

        if (metadata != null)
        {
            // Calculate state based on age for temporary flags
            var calculatedState = CalculateState(metadata, setting.Created);

            return new FeatureFlagResponse
            {
                Name = featureName,
                Key = featureName,
                IsEnabled = metadata.IsEnabled,
                Description = metadata.Description ?? setting.Description,
                Category = metadata.Category,
                Tags = metadata.Tags,
                Type = metadata.Type,
                State = calculatedState,
                Created = setting.Created,
                CreatedBy = setting.CreatedBy,
                LastModified = setting.LastModified,
                LastModifiedBy = setting.LastModifiedBy,
                ExpiresAt = metadata.ExpiresAt
            };
        }

        // Fallback for simple boolean values (legacy format)
        var isEnabled = bool.TryParse(setting.Value, out var result) && result;

        return new FeatureFlagResponse
        {
            Name = featureName,
            Key = featureName,
            IsEnabled = isEnabled,
            Description = setting.Description,
            Category = "General",
            Tags = Array.Empty<string>(),
            Type = FeatureFlagType.Permanent,
            State = FeatureFlagState.Active,
            Created = setting.Created,
            CreatedBy = setting.CreatedBy,
            LastModified = setting.LastModified,
            LastModifiedBy = setting.LastModifiedBy,
            ExpiresAt = null
        };
    }

    private static FeatureFlagState CalculateState(FeatureFlagMetadata metadata, DateTime created)
    {
        // If manually set to Stale or Archived, respect that
        if (metadata.State == FeatureFlagState.Stale || metadata.State == FeatureFlagState.Archived)
        {
            return metadata.State;
        }

        // Only auto-calculate for Temporary flags
        if (metadata.Type != FeatureFlagType.Temporary)
        {
            return metadata.State;
        }

        var ageInDays = (DateTime.UtcNow - created).TotalDays;

        if (ageInDays >= VeryStaleThreshold)
        {
            return FeatureFlagState.Stale;
        }

        if (ageInDays >= StaleDaysThreshold)
        {
            return FeatureFlagState.PotentiallyStale;
        }

        return FeatureFlagState.Active;
    }

    private static FeatureFlagMetadata? TryParseMetadata(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        // Check if it looks like JSON
        if (!value.TrimStart().StartsWith('{'))
            return null;

        try
        {
            return JsonSerializer.Deserialize<FeatureFlagMetadata>(value, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private static string GetFeatureKey(string featureName)
    {
        if (featureName.StartsWith(FeatureKeyPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return featureName;
        }
        return $"{FeatureKeyPrefix}{featureName}";
    }
}

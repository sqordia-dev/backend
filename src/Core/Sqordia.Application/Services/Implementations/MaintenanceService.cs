using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.Maintenance;
using Sqordia.Contracts.Responses.Maintenance;
using Sqordia.Domain.Enums;

namespace Sqordia.Application.Services.Implementations;

/// <summary>
/// Service for managing maintenance mode status.
/// Stores maintenance state in the Settings table as JSON.
/// </summary>
public class MaintenanceService : IMaintenanceService
{
    private const string MaintenanceStatusKey = "System:MaintenanceStatus";
    private const string CacheKey = "MaintenanceStatus";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(10);

    private readonly ISettingsService _settingsService;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<MaintenanceService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public MaintenanceService(
        ISettingsService settingsService,
        IMemoryCache memoryCache,
        ILogger<MaintenanceService> logger)
    {
        _settingsService = settingsService;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<Result<MaintenanceStatusResponse>> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Try cache first
            if (_memoryCache.TryGetValue(CacheKey, out MaintenanceStatusResponse? cachedStatus) && cachedStatus != null)
            {
                return Result.Success(cachedStatus);
            }

            // Fetch from settings
            var result = await _settingsService.GetSettingAsync(MaintenanceStatusKey, cancellationToken);

            MaintenanceStatusResponse status;
            if (!result.IsSuccess || string.IsNullOrEmpty(result.Value))
            {
                // No maintenance status set, return default (disabled)
                status = CreateDefaultStatus();
            }
            else
            {
                status = JsonSerializer.Deserialize<MaintenanceStatusResponse>(result.Value, JsonOptions)
                         ?? CreateDefaultStatus();
            }

            // Cache the result
            _memoryCache.Set(CacheKey, status, CacheDuration);

            return Result.Success(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving maintenance status");
            // On error, return disabled status (fail-open for status check)
            return Result.Success(CreateDefaultStatus());
        }
    }

    public async Task<Result> EnableMaintenanceAsync(UpdateMaintenanceStatusRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogWarning("Enabling maintenance mode. Reason: {Reason}, DeploymentId: {DeploymentId}",
                request.Reason, request.DeploymentId);

            var status = new MaintenanceStatusResponse
            {
                IsEnabled = true,
                Reason = request.Reason,
                StartedAt = DateTime.UtcNow,
                EstimatedEnd = request.EstimatedEnd,
                ProgressPercent = request.ProgressPercent ?? 0,
                CurrentStep = request.CurrentStep ?? "Initializing...",
                DeploymentId = request.DeploymentId,
                Type = request.Type ?? "Deployment",
                AllowAdminAccess = request.AllowAdminAccess,
                AutoDisableAt = request.AutoDisableMinutes.HasValue
                    ? DateTime.UtcNow.AddMinutes(request.AutoDisableMinutes.Value)
                    : DateTime.UtcNow.AddMinutes(30), // Default 30 minute timeout
                Content = CreateDefaultContent()
            };

            var json = JsonSerializer.Serialize(status, JsonOptions);

            var saveResult = await _settingsService.UpsertSystemSettingAsync(
                MaintenanceStatusKey,
                json,
                "System",
                "Maintenance mode status",
                isPublic: true,
                settingType: SettingType.Config,
                dataType: SettingDataType.Json,
                cancellationToken);

            if (!saveResult.IsSuccess)
            {
                return saveResult;
            }

            // Invalidate cache
            _memoryCache.Remove(CacheKey);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling maintenance mode");
            return Result.Failure(Error.Failure("Maintenance.EnableError", "Failed to enable maintenance mode"));
        }
    }

    public async Task<Result> DisableMaintenanceAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogWarning("Disabling maintenance mode");

            var status = CreateDefaultStatus();
            var json = JsonSerializer.Serialize(status, JsonOptions);

            var saveResult = await _settingsService.UpsertSystemSettingAsync(
                MaintenanceStatusKey,
                json,
                "System",
                "Maintenance mode status",
                isPublic: true,
                settingType: SettingType.Config,
                dataType: SettingDataType.Json,
                cancellationToken);

            if (!saveResult.IsSuccess)
            {
                return saveResult;
            }

            // Invalidate cache
            _memoryCache.Remove(CacheKey);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling maintenance mode");
            return Result.Failure(Error.Failure("Maintenance.DisableError", "Failed to disable maintenance mode"));
        }
    }

    public async Task<Result> UpdateProgressAsync(UpdateDeploymentProgressRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var statusResult = await GetStatusAsync(cancellationToken);
            if (!statusResult.IsSuccess)
            {
                return Result.Failure(statusResult.Error!);
            }

            var status = statusResult.Value!;

            // Verify deployment ID matches (security check)
            if (!string.IsNullOrEmpty(status.DeploymentId) && status.DeploymentId != request.DeploymentId)
            {
                _logger.LogWarning("Deployment ID mismatch. Expected: {Expected}, Got: {Got}",
                    status.DeploymentId, request.DeploymentId);
                return Result.Failure(Error.Validation("Maintenance.DeploymentMismatch", "Deployment ID does not match"));
            }

            // Update progress
            status.ProgressPercent = Math.Clamp(request.ProgressPercent, 0, 100);
            status.CurrentStep = request.CurrentStep;
            if (request.EstimatedEnd.HasValue)
            {
                status.EstimatedEnd = request.EstimatedEnd;
            }

            var json = JsonSerializer.Serialize(status, JsonOptions);

            var saveResult = await _settingsService.UpsertSystemSettingAsync(
                MaintenanceStatusKey,
                json,
                "System",
                "Maintenance mode status",
                isPublic: true,
                settingType: SettingType.Config,
                dataType: SettingDataType.Json,
                cancellationToken);

            if (!saveResult.IsSuccess)
            {
                return saveResult;
            }

            // Invalidate cache
            _memoryCache.Remove(CacheKey);

            _logger.LogInformation("Updated maintenance progress: {Progress}% - {Step}",
                request.ProgressPercent, request.CurrentStep);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating maintenance progress");
            return Result.Failure(Error.Failure("Maintenance.UpdateError", "Failed to update maintenance progress"));
        }
    }

    public async Task<Result<bool>> IsInMaintenanceAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var statusResult = await GetStatusAsync(cancellationToken);
            if (!statusResult.IsSuccess)
            {
                // On error, assume not in maintenance (fail-open)
                return Result.Success(false);
            }

            return Result.Success(statusResult.Value?.IsEnabled ?? false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking maintenance status");
            return Result.Success(false); // Fail-open
        }
    }

    public async Task<Result> CheckAndAutoDisableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var statusResult = await GetStatusAsync(cancellationToken);
            if (!statusResult.IsSuccess || statusResult.Value == null)
            {
                return Result.Success();
            }

            var status = statusResult.Value;
            if (!status.IsEnabled)
            {
                return Result.Success();
            }

            // Check if auto-disable time has passed
            if (status.AutoDisableAt.HasValue && DateTime.UtcNow > status.AutoDisableAt.Value)
            {
                _logger.LogWarning("Auto-disabling maintenance mode after timeout. DeploymentId: {DeploymentId}",
                    status.DeploymentId);
                return await DisableMaintenanceAsync(cancellationToken);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during auto-disable check");
            return Result.Failure(Error.Failure("Maintenance.AutoDisableError", "Failed to check auto-disable"));
        }
    }

    private static MaintenanceStatusResponse CreateDefaultStatus()
    {
        return new MaintenanceStatusResponse
        {
            IsEnabled = false,
            ProgressPercent = 0,
            Type = "Deployment",
            AllowAdminAccess = true,
            Content = CreateDefaultContent()
        };
    }

    private static MaintenanceContentResponse CreateDefaultContent()
    {
        return new MaintenanceContentResponse
        {
            En = new MaintenanceLocalizedContent
            {
                Title = "Under Maintenance",
                Subtitle = "We're improving Sqordia",
                Description = "Our team is working hard to bring you a better experience. Please check back shortly."
            },
            Fr = new MaintenanceLocalizedContent
            {
                Title = "En maintenance",
                Subtitle = "Nous ameliorons Sqordia",
                Description = "Notre equipe travaille pour vous offrir une meilleure experience. Veuillez revenir sous peu."
            }
        };
    }
}

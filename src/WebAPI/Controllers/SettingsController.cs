using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Services;
using Sqordia.Contracts.Requests.Settings;
using Sqordia.Contracts.Responses.Settings;
using Sqordia.Domain.Enums;
using WebAPI.Controllers;

namespace WebAPI.Controllers;

/// <summary>
/// Controller for managing application settings
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class SettingsController : BaseApiController
{
    private readonly ISettingsService _settingsService;
    private readonly IFeatureFlagsService _featureFlagsService;
    private readonly ILogger<SettingsController> _logger;

    public SettingsController(
        ISettingsService settingsService,
        IFeatureFlagsService featureFlagsService,
        ILogger<SettingsController> logger)
    {
        _settingsService = settingsService;
        _featureFlagsService = featureFlagsService;
        _logger = logger;
    }

    /// <summary>
    /// Get all public settings
    /// </summary>
    /// <returns>Dictionary of public settings</returns>
    [HttpGet]
    [ProducesResponseType(typeof(Dictionary<string, string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPublicSettings(CancellationToken cancellationToken)
    {
        var result = await _settingsService.GetPublicSettingsAsync(cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get a specific setting by key
    /// </summary>
    /// <param name="key">Setting key</param>
    /// <returns>Setting value</returns>
    [HttpGet("{key}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSetting(string key, CancellationToken cancellationToken)
    {
        var result = await _settingsService.GetSettingAsync(key, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get settings by category
    /// </summary>
    /// <param name="category">Category name</param>
    /// <returns>Dictionary of settings in the category</returns>
    [HttpGet("category/{category}")]
    [ProducesResponseType(typeof(Dictionary<string, string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSettingsByCategory(string category, CancellationToken cancellationToken)
    {
        var result = await _settingsService.GetSettingsByCategoryAsync(category, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get all settings (admin only)
    /// </summary>
    /// <returns>Dictionary of all settings with metadata</returns>
    [HttpGet("all")]
    [Authorize(Roles = "Admin,Administrator")]
    [ProducesResponseType(typeof(Dictionary<string, object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllSettings(CancellationToken cancellationToken)
    {
        var result = await _settingsService.GetAllSettingsAsync(cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Create or update a setting (admin only)
    /// </summary>
    /// <param name="request">Setting data</param>
    /// <returns>Success result</returns>
    [HttpPost]
    [Authorize(Roles = "Admin,Administrator")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpsertSetting([FromBody] UpsertSettingRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _settingsService.UpsertSettingAsync(
            request.Key,
            request.Value,
            request.Category,
            request.Description,
            request.IsPublic,
            request.SettingType,
            request.DataType,
            request.Encrypt,
            request.CacheDurationMinutes,
            request.IsCritical,
            cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Update a setting value (admin only)
    /// </summary>
    /// <param name="key">Setting key</param>
    /// <param name="request">Update request</param>
    /// <returns>Success result</returns>
    [HttpPut("{key}")]
    [Authorize(Roles = "Admin,Administrator")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSetting(string key, [FromBody] UpdateSettingRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _settingsService.UpdateSettingValueAsync(key, request.Value, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Delete a setting (admin only)
    /// </summary>
    /// <param name="key">Setting key</param>
    /// <returns>Success result</returns>
    [HttpDelete("{key}")]
    [Authorize(Roles = "Admin,Administrator")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSetting(string key, CancellationToken cancellationToken)
    {
        var result = await _settingsService.DeleteSettingAsync(key, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get all feature flags
    /// </summary>
    /// <returns>Dictionary of feature flags</returns>
    [HttpGet("features")]
    [ProducesResponseType(typeof(Dictionary<string, bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllFeatures(CancellationToken cancellationToken)
    {
        var result = await _featureFlagsService.GetAllFeaturesAsync(cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Check if a feature is enabled
    /// </summary>
    /// <param name="featureName">Feature name</param>
    /// <returns>True if enabled, false otherwise</returns>
    [HttpGet("features/{featureName}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> IsFeatureEnabled(string featureName, CancellationToken cancellationToken)
    {
        var result = await _featureFlagsService.IsEnabledAsync(featureName, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Enable a feature flag (admin only)
    /// </summary>
    /// <param name="featureName">Feature name</param>
    /// <returns>Success result</returns>
    [HttpPost("features/{featureName}/enable")]
    [Authorize(Roles = "Admin,Administrator")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> EnableFeature(string featureName, CancellationToken cancellationToken)
    {
        var result = await _featureFlagsService.EnableFeatureAsync(featureName, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Disable a feature flag (admin only)
    /// </summary>
    /// <param name="featureName">Feature name</param>
    /// <returns>Success result</returns>
    [HttpPost("features/{featureName}/disable")]
    [Authorize(Roles = "Admin,Administrator")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DisableFeature(string featureName, CancellationToken cancellationToken)
    {
        var result = await _featureFlagsService.DisableFeatureAsync(featureName, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get all feature flags with detailed metadata (admin only)
    /// </summary>
    /// <returns>List of feature flags with metadata</returns>
    [HttpGet("features/detailed")]
    [Authorize(Roles = "Admin,Administrator")]
    [ProducesResponseType(typeof(FeatureFlagListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllFeaturesDetailed(CancellationToken cancellationToken)
    {
        var result = await _featureFlagsService.GetAllFeaturesDetailedAsync(cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get a specific feature flag with detailed metadata (admin only)
    /// </summary>
    /// <param name="featureName">Feature name</param>
    /// <returns>Feature flag with metadata</returns>
    [HttpGet("features/{featureName}/detailed")]
    [Authorize(Roles = "Admin,Administrator")]
    [ProducesResponseType(typeof(FeatureFlagResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetFeatureDetailed(string featureName, CancellationToken cancellationToken)
    {
        var result = await _featureFlagsService.GetFeatureDetailedAsync(featureName, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a new feature flag with metadata (admin only)
    /// </summary>
    /// <param name="request">Feature flag creation request</param>
    /// <returns>Created feature flag</returns>
    [HttpPost("features")]
    [Authorize(Roles = "Admin,Administrator")]
    [ProducesResponseType(typeof(FeatureFlagResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateFeature([FromBody] CreateFeatureFlagRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _featureFlagsService.CreateFeatureAsync(request, cancellationToken);

        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetFeatureDetailed), new { featureName = request.Name }, result.Value);
        }

        return HandleResult(result);
    }

    /// <summary>
    /// Update a feature flag's metadata (admin only)
    /// </summary>
    /// <param name="featureName">Feature name</param>
    /// <param name="request">Update request</param>
    /// <returns>Updated feature flag</returns>
    [HttpPatch("features/{featureName}")]
    [Authorize(Roles = "Admin,Administrator")]
    [ProducesResponseType(typeof(FeatureFlagResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateFeatureMetadata(string featureName, [FromBody] UpdateFeatureFlagRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _featureFlagsService.UpdateFeatureMetadataAsync(featureName, request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Toggle a feature flag's enabled state (admin only)
    /// </summary>
    /// <param name="featureName">Feature name</param>
    /// <param name="request">Toggle request</param>
    /// <returns>Updated feature flag</returns>
    [HttpPost("features/{featureName}/toggle")]
    [Authorize(Roles = "Admin,Administrator")]
    [ProducesResponseType(typeof(FeatureFlagResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ToggleFeature(string featureName, [FromBody] ToggleFeatureFlagRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        Result result;
        if (request.IsEnabled)
        {
            result = await _featureFlagsService.EnableFeatureAsync(featureName, cancellationToken);
        }
        else
        {
            result = await _featureFlagsService.DisableFeatureAsync(featureName, cancellationToken);
        }

        if (!result.IsSuccess)
        {
            return HandleResult(result);
        }

        var detailedResult = await _featureFlagsService.GetFeatureDetailedAsync(featureName, cancellationToken);
        return HandleResult(detailedResult);
    }

    /// <summary>
    /// Mark a feature flag as stale (admin only)
    /// </summary>
    /// <param name="featureName">Feature name</param>
    /// <returns>Success result</returns>
    [HttpPost("features/{featureName}/stale")]
    [Authorize(Roles = "Admin,Administrator")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> MarkFeatureAsStale(string featureName, CancellationToken cancellationToken)
    {
        var result = await _featureFlagsService.MarkAsStaleAsync(featureName, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Archive a feature flag (admin only)
    /// </summary>
    /// <param name="featureName">Feature name</param>
    /// <returns>Success result</returns>
    [HttpDelete("features/{featureName}/archive")]
    [Authorize(Roles = "Admin,Administrator")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ArchiveFeature(string featureName, CancellationToken cancellationToken)
    {
        var result = await _featureFlagsService.ArchiveFeatureAsync(featureName, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Store an encrypted secret (admin only)
    /// </summary>
    /// <param name="key">Secret key</param>
    /// <param name="request">Secret data</param>
    /// <returns>Success result</returns>
    [HttpPost("secrets/{key}")]
    [Authorize(Roles = "Admin,Administrator")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> StoreSecret(string key, [FromBody] StoreSecretRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _settingsService.EncryptAndStoreSecretAsync(
            key,
            request.Secret,
            request.Category ?? "Secrets",
            request.Description,
            cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Refresh cache for a setting or all critical settings (admin only)
    /// </summary>
    /// <param name="key">Optional setting key to refresh</param>
    /// <returns>Success result</returns>
    [HttpPost("cache/refresh")]
    [Authorize(Roles = "Admin,Administrator")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RefreshCache([FromQuery] string? key, CancellationToken cancellationToken)
    {
        var result = await _settingsService.RefreshCacheAsync(key, cancellationToken);
        return HandleResult(result);
    }
}

/// <summary>
/// Request model for creating or updating a setting
/// </summary>
public class UpsertSettingRequest
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPublic { get; set; } = false;
    public SettingType SettingType { get; set; } = SettingType.Config;
    public SettingDataType DataType { get; set; } = SettingDataType.String;
    public bool Encrypt { get; set; } = false;
    public int? CacheDurationMinutes { get; set; }
    public bool IsCritical { get; set; } = false;
}

/// <summary>
/// Request model for updating a setting value
/// </summary>
public class UpdateSettingRequest
{
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// Request model for storing a secret
/// </summary>
public class StoreSecretRequest
{
    public string Secret { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Description { get; set; }
}


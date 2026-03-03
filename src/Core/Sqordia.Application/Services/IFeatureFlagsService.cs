using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.Settings;
using Sqordia.Contracts.Responses.Settings;

namespace Sqordia.Application.Services;

/// <summary>
/// Service interface for managing feature flags
/// </summary>
public interface IFeatureFlagsService
{
    /// <summary>
    /// Check if a feature is enabled
    /// </summary>
    Task<Result<bool>> IsEnabledAsync(string featureName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enable a feature flag
    /// </summary>
    Task<Result> EnableFeatureAsync(string featureName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Disable a feature flag
    /// </summary>
    Task<Result> DisableFeatureAsync(string featureName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all feature flags (simple format - just name and enabled state)
    /// </summary>
    Task<Result<Dictionary<string, bool>>> GetAllFeaturesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all feature flags with full metadata including audit information
    /// </summary>
    Task<Result<FeatureFlagListResponse>> GetAllFeaturesDetailedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a single feature flag with full metadata
    /// </summary>
    Task<Result<FeatureFlagResponse>> GetFeatureDetailedAsync(string featureName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new feature flag with metadata
    /// </summary>
    Task<Result<FeatureFlagResponse>> CreateFeatureAsync(CreateFeatureFlagRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update a feature flag's metadata (not the enabled state)
    /// </summary>
    Task<Result<FeatureFlagResponse>> UpdateFeatureMetadataAsync(string featureName, UpdateFeatureFlagRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark a feature flag as stale (needing cleanup)
    /// </summary>
    Task<Result> MarkAsStaleAsync(string featureName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Archive a feature flag (soft delete for audit trail)
    /// </summary>
    Task<Result> ArchiveFeatureAsync(string featureName, CancellationToken cancellationToken = default);
}


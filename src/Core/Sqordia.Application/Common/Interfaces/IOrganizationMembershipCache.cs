namespace Sqordia.Application.Common.Interfaces;

/// <summary>
/// Cached service for organization membership checks.
/// Provides fast, cached lookups for frequently queried membership data.
/// </summary>
public interface IOrganizationMembershipCache
{
    /// <summary>
    /// Checks if a user is an active member of an organization.
    /// Results are cached for performance.
    /// </summary>
    Task<bool> IsUserMemberAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user is an owner of an organization.
    /// Results are cached for performance.
    /// </summary>
    Task<bool> IsUserOwnerAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the user's role in an organization.
    /// Returns null if user is not a member.
    /// </summary>
    Task<string?> GetUserRoleAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all organization IDs where the user is an active member.
    /// Results are cached for performance.
    /// </summary>
    Task<IReadOnlyList<Guid>> GetUserOrganizationIdsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates all cached data for a specific user-organization pair.
    /// Call this when membership changes.
    /// </summary>
    void InvalidateMembership(Guid organizationId, Guid userId);

    /// <summary>
    /// Invalidates all cached data for a user across all organizations.
    /// Call this when user is deleted or deactivated.
    /// </summary>
    void InvalidateUser(Guid userId);
}

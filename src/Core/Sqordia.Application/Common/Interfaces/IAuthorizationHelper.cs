using Sqordia.Application.Common.Models;

namespace Sqordia.Application.Common.Interfaces;

/// <summary>
/// Helper service for common authorization checks.
/// Provides reusable methods to reduce boilerplate in service implementations.
/// </summary>
public interface IAuthorizationHelper
{
    /// <summary>
    /// Gets the current user ID if authenticated.
    /// Returns Unauthorized error if not authenticated.
    /// </summary>
    Result<Guid> RequireAuthenticatedUser();

    /// <summary>
    /// Verifies the current user is an active member of the specified organization.
    /// Returns Forbidden error if not a member.
    /// </summary>
    Task<Result<Guid>> RequireOrganizationMemberAsync(Guid organizationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies the current user is an owner of the specified organization.
    /// Returns Forbidden error if not an owner.
    /// </summary>
    Task<Result<Guid>> RequireOrganizationOwnerAsync(Guid organizationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current user ID and verifies membership in the organization associated with a business plan.
    /// Returns the user ID and organization ID if authorized.
    /// </summary>
    Task<Result<AuthorizationContext>> RequireBusinessPlanAccessAsync(Guid businessPlanId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the current user is a member of the specified organization.
    /// Does not throw or return errors - just returns true/false.
    /// </summary>
    Task<bool> IsOrganizationMemberAsync(Guid organizationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the current user is an owner of the specified organization.
    /// Does not throw or return errors - just returns true/false.
    /// </summary>
    Task<bool> IsOrganizationOwnerAsync(Guid organizationId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Context returned from successful authorization checks.
/// Contains user and resource information.
/// </summary>
public record AuthorizationContext
{
    /// <summary>
    /// The authenticated user's ID.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// The organization ID for the accessed resource.
    /// </summary>
    public required Guid OrganizationId { get; init; }

    /// <summary>
    /// The business plan ID if applicable.
    /// </summary>
    public Guid? BusinessPlanId { get; init; }

    /// <summary>
    /// The user's role in the organization.
    /// </summary>
    public string? OrganizationRole { get; init; }
}

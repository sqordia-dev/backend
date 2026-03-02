using Microsoft.EntityFrameworkCore;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;

namespace Sqordia.Application.Services.Implementations;

/// <summary>
/// Implementation of authorization helper for common permission checks.
/// Uses caching for performance optimization.
/// </summary>
public class AuthorizationHelper : IAuthorizationHelper
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IOrganizationMembershipCache _membershipCache;
    private readonly IApplicationDbContext _context;
    private readonly ILocalizationService _localizationService;

    public AuthorizationHelper(
        ICurrentUserService currentUserService,
        IOrganizationMembershipCache membershipCache,
        IApplicationDbContext context,
        ILocalizationService localizationService)
    {
        _currentUserService = currentUserService;
        _membershipCache = membershipCache;
        _context = context;
        _localizationService = localizationService;
    }

    public Result<Guid> RequireAuthenticatedUser()
    {
        var userId = _currentUserService.GetUserIdAsGuid();
        if (!userId.HasValue)
        {
            return Result.Failure<Guid>(
                Error.Unauthorized("General.Unauthorized", _localizationService.GetString("General.Unauthorized")));
        }

        return Result.Success(userId.Value);
    }

    public async Task<Result<Guid>> RequireOrganizationMemberAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var userResult = RequireAuthenticatedUser();
        if (!userResult.IsSuccess)
        {
            return userResult;
        }

        var userId = userResult.Value;
        var isMember = await _membershipCache.IsUserMemberAsync(organizationId, userId, cancellationToken);

        if (!isMember)
        {
            return Result.Failure<Guid>(
                Error.Forbidden("Organization.Error.Forbidden", _localizationService.GetString("Organization.Error.Forbidden")));
        }

        return Result.Success(userId);
    }

    public async Task<Result<Guid>> RequireOrganizationOwnerAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var userResult = RequireAuthenticatedUser();
        if (!userResult.IsSuccess)
        {
            return userResult;
        }

        var userId = userResult.Value;
        var isOwner = await _membershipCache.IsUserOwnerAsync(organizationId, userId, cancellationToken);

        if (!isOwner)
        {
            return Result.Failure<Guid>(
                Error.Forbidden("Organization.Error.OwnerRequired", _localizationService.GetString("Organization.Error.OwnerRequired")));
        }

        return Result.Success(userId);
    }

    public async Task<Result<AuthorizationContext>> RequireBusinessPlanAccessAsync(Guid businessPlanId, CancellationToken cancellationToken = default)
    {
        var userResult = RequireAuthenticatedUser();
        if (!userResult.IsSuccess)
        {
            return Result.Failure<AuthorizationContext>(userResult.Error);
        }

        var userId = userResult.Value;

        // Get business plan to find organization
        var businessPlan = await _context.BusinessPlans
            .AsNoTracking()
            .Where(bp => bp.Id == businessPlanId && !bp.IsDeleted)
            .Select(bp => new { bp.Id, bp.OrganizationId })
            .FirstOrDefaultAsync(cancellationToken);

        if (businessPlan == null)
        {
            return Result.Failure<AuthorizationContext>(
                Error.NotFound("BusinessPlan.Error.NotFound", _localizationService.GetString("BusinessPlan.Error.NotFound")));
        }

        // Check organization membership
        var isMember = await _membershipCache.IsUserMemberAsync(businessPlan.OrganizationId, userId, cancellationToken);
        if (!isMember)
        {
            return Result.Failure<AuthorizationContext>(
                Error.Forbidden("BusinessPlan.Error.Forbidden", _localizationService.GetString("BusinessPlan.Error.Forbidden")));
        }

        // Get role for context
        var role = await _membershipCache.GetUserRoleAsync(businessPlan.OrganizationId, userId, cancellationToken);

        return Result.Success(new AuthorizationContext
        {
            UserId = userId,
            OrganizationId = businessPlan.OrganizationId,
            BusinessPlanId = businessPlanId,
            OrganizationRole = role
        });
    }

    public async Task<bool> IsOrganizationMemberAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.GetUserIdAsGuid();
        if (!userId.HasValue)
        {
            return false;
        }

        return await _membershipCache.IsUserMemberAsync(organizationId, userId.Value, cancellationToken);
    }

    public async Task<bool> IsOrganizationOwnerAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.GetUserIdAsGuid();
        if (!userId.HasValue)
        {
            return false;
        }

        return await _membershipCache.IsUserOwnerAsync(organizationId, userId.Value, cancellationToken);
    }
}

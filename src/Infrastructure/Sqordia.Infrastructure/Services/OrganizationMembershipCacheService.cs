using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Domain.Enums;

namespace Sqordia.Infrastructure.Services;

/// <summary>
/// Cached implementation for organization membership lookups.
/// Uses IMemoryCache to reduce database hits for frequently checked permissions.
/// </summary>
public class OrganizationMembershipCacheService : IOrganizationMembershipCache
{
    private readonly IApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<OrganizationMembershipCacheService> _logger;

    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(5);
    private const string MembershipCacheKeyPrefix = "org-member:";
    private const string RoleCacheKeyPrefix = "org-role:";
    private const string UserOrgsCacheKeyPrefix = "user-orgs:";

    public OrganizationMembershipCacheService(
        IApplicationDbContext context,
        IMemoryCache cache,
        ILogger<OrganizationMembershipCacheService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> IsUserMemberAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken = default)
    {
        var cacheKey = GetMembershipCacheKey(organizationId, userId);

        if (_cache.TryGetValue(cacheKey, out bool isMember))
        {
            _logger.LogDebug("Cache hit for membership check: {OrgId}:{UserId}", organizationId, userId);
            return isMember;
        }

        _logger.LogDebug("Cache miss for membership check: {OrgId}:{UserId}", organizationId, userId);

        isMember = await _context.OrganizationMembers
            .AsNoTracking()
            .AnyAsync(om => om.OrganizationId == organizationId &&
                           om.UserId == userId &&
                           om.IsActive, cancellationToken);

        _cache.Set(cacheKey, isMember, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CacheExpiration,
            SlidingExpiration = TimeSpan.FromMinutes(2)
        });

        return isMember;
    }

    public async Task<bool> IsUserOwnerAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken = default)
    {
        var role = await GetUserRoleAsync(organizationId, userId, cancellationToken);
        return role == OrganizationRole.Owner.ToString();
    }

    public async Task<string?> GetUserRoleAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken = default)
    {
        var cacheKey = GetRoleCacheKey(organizationId, userId);

        if (_cache.TryGetValue(cacheKey, out string? role))
        {
            _logger.LogDebug("Cache hit for role check: {OrgId}:{UserId}", organizationId, userId);
            return role;
        }

        _logger.LogDebug("Cache miss for role check: {OrgId}:{UserId}", organizationId, userId);

        var member = await _context.OrganizationMembers
            .AsNoTracking()
            .Where(om => om.OrganizationId == organizationId &&
                        om.UserId == userId &&
                        om.IsActive)
            .Select(om => om.Role)
            .FirstOrDefaultAsync(cancellationToken);

        role = member.ToString();

        // Cache even null results (user not a member) to avoid repeated lookups
        _cache.Set(cacheKey, role, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CacheExpiration,
            SlidingExpiration = TimeSpan.FromMinutes(2)
        });

        return role;
    }

    public async Task<IReadOnlyList<Guid>> GetUserOrganizationIdsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cacheKey = GetUserOrgsCacheKey(userId);

        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<Guid>? orgIds) && orgIds != null)
        {
            _logger.LogDebug("Cache hit for user organizations: {UserId}", userId);
            return orgIds;
        }

        _logger.LogDebug("Cache miss for user organizations: {UserId}", userId);

        var organizationIds = await _context.OrganizationMembers
            .AsNoTracking()
            .Where(om => om.UserId == userId && om.IsActive)
            .Select(om => om.OrganizationId)
            .Distinct()
            .ToListAsync(cancellationToken);

        _cache.Set(cacheKey, (IReadOnlyList<Guid>)organizationIds, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CacheExpiration,
            SlidingExpiration = TimeSpan.FromMinutes(2)
        });

        return organizationIds;
    }

    public void InvalidateMembership(Guid organizationId, Guid userId)
    {
        _cache.Remove(GetMembershipCacheKey(organizationId, userId));
        _cache.Remove(GetRoleCacheKey(organizationId, userId));
        _cache.Remove(GetUserOrgsCacheKey(userId));
        _logger.LogDebug("Invalidated membership cache for {OrgId}:{UserId}", organizationId, userId);
    }

    public void InvalidateUser(Guid userId)
    {
        _cache.Remove(GetUserOrgsCacheKey(userId));
        _logger.LogDebug("Invalidated user organizations cache for {UserId}", userId);
    }

    private static string GetMembershipCacheKey(Guid organizationId, Guid userId)
        => $"{MembershipCacheKeyPrefix}{organizationId}:{userId}";

    private static string GetRoleCacheKey(Guid organizationId, Guid userId)
        => $"{RoleCacheKeyPrefix}{organizationId}:{userId}";

    private static string GetUserOrgsCacheKey(Guid userId)
        => $"{UserOrgsCacheKeyPrefix}{userId}";
}

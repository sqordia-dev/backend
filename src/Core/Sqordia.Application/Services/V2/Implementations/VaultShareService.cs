using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Common.Security;
using Sqordia.Contracts.Requests.V2.Share;
using Sqordia.Contracts.Responses.V2.Share;
using Sqordia.Domain.Entities.BusinessPlan;
using Sqordia.Domain.Enums;

namespace Sqordia.Application.Services.V2.Implementations;

/// <summary>
/// Secure vault share service implementation
/// </summary>
public class VaultShareService : IVaultShareService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ISecurityService _securityService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<VaultShareService> _logger;

    public VaultShareService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ISecurityService securityService,
        IConfiguration configuration,
        ILogger<VaultShareService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _securityService = securityService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Result<VaultShareResponse>> CreateVaultShareAsync(
        Guid businessPlanId,
        CreateVaultShareRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating vault share for business plan {PlanId}", businessPlanId);

            var businessPlan = await GetBusinessPlanWithAccessCheckAsync(businessPlanId, cancellationToken);
            if (businessPlan == null)
            {
                return Result.Failure<VaultShareResponse>(
                    Error.NotFound("BusinessPlan.NotFound", "Business plan not found or access denied."));
            }

            // Create the share
            var share = new BusinessPlanShare(
                businessPlanId,
                SharePermission.ReadOnly,
                isPublic: true,
                expiresAt: request.ExpiresAt);

            // Configure vault share features
            string? passwordHash = null;
            if (!string.IsNullOrEmpty(request.Password))
            {
                passwordHash = _securityService.HashPassword(request.Password);
            }

            share.ConfigureAsVaultShare(
                enableWatermark: request.EnableWatermark,
                watermarkText: request.WatermarkText,
                allowDownload: request.AllowDownload,
                trackViews: request.TrackViews,
                requireEmailVerification: request.RequireEmailVerification,
                passwordHash: passwordHash,
                maxViews: request.MaxViews);

            _context.BusinessPlanShares.Add(share);
            await _context.SaveChangesAsync(cancellationToken);

            var baseUrl = _configuration["Frontend:BaseUrl"] ?? "https://sqordia.app";
            var shareUrl = $"{baseUrl}/vault/{share.PublicToken}";

            var response = MapToResponse(share, shareUrl);

            _logger.LogInformation("Vault share created with ID {ShareId}", share.Id);

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating vault share for business plan {PlanId}", businessPlanId);
            return Result.Failure<VaultShareResponse>(
                Error.InternalServerError("VaultShare.CreateError", "An error occurred creating the vault share."));
        }
    }

    public async Task<Result<VaultShareResponse>> GetVaultShareAsync(
        Guid shareId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var share = await _context.BusinessPlanShares
                .Include(s => s.BusinessPlan)
                .FirstOrDefaultAsync(s => s.Id == shareId && s.IsVaultShare, cancellationToken);

            if (share == null)
            {
                return Result.Failure<VaultShareResponse>(
                    Error.NotFound("VaultShare.NotFound", "Vault share not found."));
            }

            // Check user has access to the business plan
            if (!await HasAccessToBusinessPlanAsync(share.BusinessPlanId, cancellationToken))
            {
                return Result.Failure<VaultShareResponse>(
                    Error.Forbidden("VaultShare.AccessDenied", "You do not have access to this share."));
            }

            var baseUrl = _configuration["Frontend:BaseUrl"] ?? "https://sqordia.app";
            var shareUrl = $"{baseUrl}/vault/{share.PublicToken}";

            return Result.Success(MapToResponse(share, shareUrl));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vault share {ShareId}", shareId);
            return Result.Failure<VaultShareResponse>(
                Error.InternalServerError("VaultShare.GetError", "An error occurred getting the vault share."));
        }
    }

    public async Task<Result<IEnumerable<VaultShareResponse>>> GetVaultSharesAsync(
        Guid businessPlanId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await HasAccessToBusinessPlanAsync(businessPlanId, cancellationToken))
            {
                return Result.Failure<IEnumerable<VaultShareResponse>>(
                    Error.NotFound("BusinessPlan.NotFound", "Business plan not found or access denied."));
            }

            var shares = await _context.BusinessPlanShares
                .Where(s => s.BusinessPlanId == businessPlanId && s.IsVaultShare && s.IsActive)
                .OrderByDescending(s => s.Created)
                .ToListAsync(cancellationToken);

            var baseUrl = _configuration["Frontend:BaseUrl"] ?? "https://sqordia.app";
            var responses = shares.Select(s => MapToResponse(s, $"{baseUrl}/vault/{s.PublicToken}"));

            return Result.Success(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vault shares for business plan {PlanId}", businessPlanId);
            return Result.Failure<IEnumerable<VaultShareResponse>>(
                Error.InternalServerError("VaultShare.ListError", "An error occurred listing vault shares."));
        }
    }

    public async Task<Result<VaultShareAnalyticsResponse>> GetVaultShareAnalyticsAsync(
        Guid shareId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var share = await _context.BusinessPlanShares
                .FirstOrDefaultAsync(s => s.Id == shareId && s.IsVaultShare, cancellationToken);

            if (share == null)
            {
                return Result.Failure<VaultShareAnalyticsResponse>(
                    Error.NotFound("VaultShare.NotFound", "Vault share not found."));
            }

            if (!await HasAccessToBusinessPlanAsync(share.BusinessPlanId, cancellationToken))
            {
                return Result.Failure<VaultShareAnalyticsResponse>(
                    Error.Forbidden("VaultShare.AccessDenied", "You do not have access to this share."));
            }

            // For now, return basic analytics from the share record
            // In a full implementation, you'd have a separate ViewActivity table
            var response = new VaultShareAnalyticsResponse
            {
                ShareId = share.Id,
                TotalViews = share.AccessCount,
                UniqueViewers = share.AccessCount, // Simplified - would need viewer tracking table
                LastViewedAt = share.LastAccessedAt,
                RecentActivity = new List<ViewerActivity>()
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting analytics for vault share {ShareId}", shareId);
            return Result.Failure<VaultShareAnalyticsResponse>(
                Error.InternalServerError("VaultShare.AnalyticsError", "An error occurred getting analytics."));
        }
    }

    public async Task<Result<bool>> RecordViewAsync(
        string token,
        string? viewerEmail = null,
        string? viewerName = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var share = await _context.BusinessPlanShares
                .FirstOrDefaultAsync(s => s.PublicToken == token && s.IsVaultShare, cancellationToken);

            if (share == null)
            {
                return Result.Failure<bool>(
                    Error.NotFound("VaultShare.NotFound", "Vault share not found."));
            }

            if (!share.CanAccess())
            {
                return Result.Failure<bool>(
                    Error.Forbidden("VaultShare.Expired", "This share link has expired or reached its view limit."));
            }

            if (share.TrackViews)
            {
                share.RecordAccess();
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("View recorded for vault share {Token}", token);
            }

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording view for vault share {Token}", token);
            return Result.Failure<bool>(
                Error.InternalServerError("VaultShare.RecordViewError", "An error occurred recording the view."));
        }
    }

    public async Task<Result<bool>> ValidatePasswordAsync(
        string token,
        string password,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var share = await _context.BusinessPlanShares
                .FirstOrDefaultAsync(s => s.PublicToken == token && s.IsVaultShare, cancellationToken);

            if (share == null)
            {
                return Result.Failure<bool>(
                    Error.NotFound("VaultShare.NotFound", "Vault share not found."));
            }

            if (string.IsNullOrEmpty(share.PasswordHash))
            {
                return Result.Success(true); // No password required
            }

            var isValid = _securityService.VerifyPassword(password, share.PasswordHash);
            return Result.Success(isValid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating password for vault share {Token}", token);
            return Result.Failure<bool>(
                Error.InternalServerError("VaultShare.PasswordError", "An error occurred validating the password."));
        }
    }

    public async Task<Result<bool>> RevokeVaultShareAsync(
        Guid shareId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var share = await _context.BusinessPlanShares
                .FirstOrDefaultAsync(s => s.Id == shareId && s.IsVaultShare, cancellationToken);

            if (share == null)
            {
                return Result.Failure<bool>(
                    Error.NotFound("VaultShare.NotFound", "Vault share not found."));
            }

            if (!await HasAccessToBusinessPlanAsync(share.BusinessPlanId, cancellationToken))
            {
                return Result.Failure<bool>(
                    Error.Forbidden("VaultShare.AccessDenied", "You do not have access to revoke this share."));
            }

            share.Revoke();
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Vault share {ShareId} revoked", shareId);

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking vault share {ShareId}", shareId);
            return Result.Failure<bool>(
                Error.InternalServerError("VaultShare.RevokeError", "An error occurred revoking the vault share."));
        }
    }

    private async Task<BusinessPlan?> GetBusinessPlanWithAccessCheckAsync(Guid businessPlanId, CancellationToken cancellationToken)
    {
        var currentUserIdString = _currentUserService.UserId;
        if (string.IsNullOrEmpty(currentUserIdString) || !Guid.TryParse(currentUserIdString, out var currentUserId))
        {
            return null;
        }

        var businessPlan = await _context.BusinessPlans
            .FirstOrDefaultAsync(bp => bp.Id == businessPlanId && !bp.IsDeleted, cancellationToken);

        if (businessPlan == null)
        {
            return null;
        }

        var isMember = await _context.OrganizationMembers
            .AnyAsync(om => om.OrganizationId == businessPlan.OrganizationId &&
                           om.UserId == currentUserId &&
                           om.IsActive, cancellationToken);

        return isMember ? businessPlan : null;
    }

    private async Task<bool> HasAccessToBusinessPlanAsync(Guid businessPlanId, CancellationToken cancellationToken)
    {
        var plan = await GetBusinessPlanWithAccessCheckAsync(businessPlanId, cancellationToken);
        return plan != null;
    }

    private static VaultShareResponse MapToResponse(BusinessPlanShare share, string shareUrl)
    {
        return new VaultShareResponse
        {
            ShareId = share.Id,
            BusinessPlanId = share.BusinessPlanId,
            ShareUrl = shareUrl,
            Token = share.PublicToken ?? "",
            ExpiresAt = share.ExpiresAt,
            EnableWatermark = share.EnableWatermark,
            WatermarkText = share.WatermarkText,
            AllowDownload = share.AllowDownload,
            TrackViews = share.TrackViews,
            RequireEmailVerification = share.RequireEmailVerification,
            HasPassword = !string.IsNullOrEmpty(share.PasswordHash),
            MaxViews = share.MaxViews,
            CurrentViews = share.AccessCount,
            CreatedAt = share.Created
        };
    }
}

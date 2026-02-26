using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Enums;
using Sqordia.Contracts.Requests.BusinessPlan;
using Sqordia.Contracts.Responses.BusinessPlan;
using Sqordia.Domain.Entities.BusinessPlan;
using Sqordia.Domain.Enums;

namespace Sqordia.Application.Services.Implementations;

public class BusinessPlanShareService : IBusinessPlanShareService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<BusinessPlanShareService> _logger;
    private readonly ILocalizationService _localizationService;

    public BusinessPlanShareService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<BusinessPlanShareService> logger,
        ILocalizationService localizationService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
        _localizationService = localizationService;
    }

    public async Task<Result<BusinessPlanShareResponse>> ShareBusinessPlanAsync(
        Guid businessPlanId,
        ShareBusinessPlanRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUserId = _currentUserService.GetUserIdAsGuid();
            if (!currentUserId.HasValue)
            {
                return Result.Failure<BusinessPlanShareResponse>(Error.Unauthorized("General.Unauthorized", _localizationService.GetString("General.Unauthorized")));
            }

            var businessPlan = await _context.BusinessPlans
                .Include(bp => bp.Organization)
                .FirstOrDefaultAsync(bp => bp.Id == businessPlanId, cancellationToken);

            if (businessPlan == null)
            {
                return Result.Failure<BusinessPlanShareResponse>(Error.NotFound("BusinessPlan.Error.NotFound", _localizationService.GetString("BusinessPlan.Error.NotFound")));
            }

            // Verify user has access and is Owner or Admin
            var member = await _context.OrganizationMembers
                .FirstOrDefaultAsync(om => om.OrganizationId == businessPlan.OrganizationId && 
                                          om.UserId == currentUserId.Value && 
                                          om.IsActive, cancellationToken);

            if (member == null || (member.Role != OrganizationRole.Owner && member.Role != OrganizationRole.Admin))
            {
                return Result.Failure<BusinessPlanShareResponse>(Error.Forbidden("BusinessPlan.Error.Forbidden", _localizationService.GetString("BusinessPlan.Error.Forbidden")));
            }

            // Validate that either SharedWithUserId or Email is provided
            if (!request.SharedWithUserId.HasValue && string.IsNullOrWhiteSpace(request.Email))
            {
                return Result.Failure<BusinessPlanShareResponse>(Error.Validation("Share.InvalidRequest", "Either SharedWithUserId or Email must be provided"));
            }

            var permission = (Domain.Enums.SharePermission)request.Permission;

            Domain.Entities.Identity.User? sharedUser = null;
            Guid? sharedWithUserId = null;
            string? sharedWithEmail = null;

            // Handle email-based sharing
            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                sharedWithEmail = request.Email.Trim().ToLowerInvariant();
                
                // Try to find user by email
                sharedUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.Value.ToLower() == sharedWithEmail, cancellationToken);
                
                if (sharedUser != null)
                {
                    sharedWithUserId = sharedUser.Id;
                }
            }
            else if (request.SharedWithUserId.HasValue)
            {
                // Handle user ID-based sharing
                sharedUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == request.SharedWithUserId.Value, cancellationToken);

                if (sharedUser == null)
                {
                    return Result.Failure<BusinessPlanShareResponse>(Error.NotFound("User.Error.NotFound", _localizationService.GetString("User.Error.NotFound")));
                }

                sharedWithUserId = sharedUser.Id;
                sharedWithEmail = sharedUser.Email.Value;
            }

            // Check if share already exists (by user ID or email)
            var existingShare = await _context.BusinessPlanShares
                .FirstOrDefaultAsync(bs => bs.BusinessPlanId == businessPlanId && 
                                          bs.IsActive && 
                                          ((sharedWithUserId.HasValue && bs.SharedWithUserId == sharedWithUserId) ||
                                           (!string.IsNullOrEmpty(sharedWithEmail) && bs.SharedWithEmail != null && bs.SharedWithEmail.ToLower() == sharedWithEmail)), 
                                          cancellationToken);

            if (existingShare != null)
            {
                // Update existing share
                existingShare.UpdatePermission(permission);
                
                // Update user ID if we found a user by email
                if (sharedWithUserId.HasValue && !existingShare.SharedWithUserId.HasValue)
                {
                    // This would require a method to update the user ID, but for now we'll just update permission
                    // The share will be linked to the user when they accept the invitation
                }
                
                await _context.SaveChangesAsync(cancellationToken);

                var response = new BusinessPlanShareResponse
                {
                    Id = existingShare.Id,
                    BusinessPlanId = existingShare.BusinessPlanId,
                    SharedWithUserId = existingShare.SharedWithUserId ?? sharedWithUserId,
                    SharedWithEmail = existingShare.SharedWithEmail ?? sharedWithEmail,
                    SharedWithUserName = sharedUser != null ? $"{sharedUser.FirstName} {sharedUser.LastName}" : null,
                    Permission = (Sqordia.Contracts.Enums.SharePermission)existingShare.Permission,
                    PermissionName = existingShare.Permission.ToString(),
                    IsPublic = existingShare.IsPublic,
                    PublicToken = existingShare.PublicToken,
                    ExpiresAt = existingShare.ExpiresAt,
                    IsActive = existingShare.IsActive,
                    LastAccessedAt = existingShare.LastAccessedAt,
                    AccessCount = existingShare.AccessCount,
                    Created = existingShare.Created,
                    CreatedBy = existingShare.CreatedBy ?? string.Empty
                };

                return Result.Success(response);
            }

            // Create new share
            var share = new BusinessPlanShare(
                businessPlanId,
                permission,
                sharedWithUserId,
                sharedWithEmail,
                false,
                request.ExpiresAt);
            share.CreatedBy = currentUserId.Value.ToString();
            _context.BusinessPlanShares.Add(share);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Business plan {PlanId} shared with {Identifier} by {OwnerId}", 
                businessPlanId, sharedWithUserId?.ToString() ?? sharedWithEmail ?? "unknown", currentUserId.Value);

            var shareResponse = new BusinessPlanShareResponse
            {
                Id = share.Id,
                BusinessPlanId = share.BusinessPlanId,
                SharedWithUserId = share.SharedWithUserId,
                SharedWithEmail = share.SharedWithEmail,
                SharedWithUserName = sharedUser != null ? $"{sharedUser.FirstName} {sharedUser.LastName}" : null,
                Permission = (Sqordia.Contracts.Enums.SharePermission)share.Permission,
                PermissionName = share.Permission.ToString(),
                IsPublic = share.IsPublic,
                PublicToken = share.PublicToken,
                ExpiresAt = share.ExpiresAt,
                IsActive = share.IsActive,
                LastAccessedAt = share.LastAccessedAt,
                AccessCount = share.AccessCount,
                Created = share.Created,
                CreatedBy = share.CreatedBy ?? string.Empty
            };

            return Result.Success(shareResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sharing business plan {PlanId}", businessPlanId);
            return Result.Failure<BusinessPlanShareResponse>(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    public async Task<Result<BusinessPlanShareResponse>> CreatePublicShareAsync(
        Guid businessPlanId, 
        CreatePublicShareRequest request, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUserId = _currentUserService.GetUserIdAsGuid();
            if (!currentUserId.HasValue)
            {
                return Result.Failure<BusinessPlanShareResponse>(Error.Unauthorized("General.Unauthorized", _localizationService.GetString("General.Unauthorized")));
            }

            var businessPlan = await _context.BusinessPlans
                .FirstOrDefaultAsync(bp => bp.Id == businessPlanId, cancellationToken);

            if (businessPlan == null)
            {
                return Result.Failure<BusinessPlanShareResponse>(Error.NotFound("BusinessPlan.Error.NotFound", _localizationService.GetString("BusinessPlan.Error.NotFound")));
            }

            // Verify user has access and is Owner or Admin
            var member = await _context.OrganizationMembers
                .FirstOrDefaultAsync(om => om.OrganizationId == businessPlan.OrganizationId && 
                                          om.UserId == currentUserId.Value && 
                                          om.IsActive, cancellationToken);

            if (member == null || (member.Role != OrganizationRole.Owner && member.Role != OrganizationRole.Admin))
            {
                return Result.Failure<BusinessPlanShareResponse>(Error.Forbidden("BusinessPlan.Error.Forbidden", _localizationService.GetString("BusinessPlan.Error.Forbidden")));
            }

            // Check if public share already exists
            var existingShare = await _context.BusinessPlanShares
                .FirstOrDefaultAsync(bs => bs.BusinessPlanId == businessPlanId && 
                                          bs.IsPublic && 
                                          bs.IsActive, cancellationToken);

            if (existingShare != null)
            {
                // Return existing share
                var response = new BusinessPlanShareResponse
                {
                    Id = existingShare.Id,
                    BusinessPlanId = existingShare.BusinessPlanId,
                    Permission = (Sqordia.Contracts.Enums.SharePermission)existingShare.Permission,
                    PermissionName = existingShare.Permission.ToString(),
                    IsPublic = existingShare.IsPublic,
                    PublicToken = existingShare.PublicToken,
                    ExpiresAt = existingShare.ExpiresAt,
                    IsActive = existingShare.IsActive,
                    LastAccessedAt = existingShare.LastAccessedAt,
                    AccessCount = existingShare.AccessCount,
                    Created = existingShare.Created,
                    CreatedBy = existingShare.CreatedBy ?? string.Empty
                };
                return Result.Success(response);
            }

            var permission = (Domain.Enums.SharePermission)request.Permission;

            // Create new public share
            var share = new BusinessPlanShare(
                businessPlanId,
                permission,
                null,
                null,
                true,
                request.ExpiresAt);
            share.CreatedBy = currentUserId.Value.ToString();
            _context.BusinessPlanShares.Add(share);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Public share created for business plan {PlanId} by {UserId}", 
                businessPlanId, currentUserId.Value);

            var shareResponse = new BusinessPlanShareResponse
            {
                Id = share.Id,
                BusinessPlanId = share.BusinessPlanId,
                Permission = (Sqordia.Contracts.Enums.SharePermission)share.Permission,
                PermissionName = share.Permission.ToString(),
                IsPublic = share.IsPublic,
                PublicToken = share.PublicToken,
                ExpiresAt = share.ExpiresAt,
                IsActive = share.IsActive,
                LastAccessedAt = share.LastAccessedAt,
                AccessCount = share.AccessCount,
                Created = share.Created,
                CreatedBy = share.CreatedBy ?? string.Empty
            };

            return Result.Success(shareResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating public share for business plan {PlanId}", businessPlanId);
            return Result.Failure<BusinessPlanShareResponse>(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    public async Task<Result<IEnumerable<BusinessPlanShareResponse>>> GetSharesAsync(
        Guid businessPlanId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUserId = _currentUserService.GetUserIdAsGuid();
            if (!currentUserId.HasValue)
            {
                return Result.Failure<IEnumerable<BusinessPlanShareResponse>>(Error.Unauthorized("General.Unauthorized", _localizationService.GetString("General.Unauthorized")));
            }

            var businessPlan = await _context.BusinessPlans
                .FirstOrDefaultAsync(bp => bp.Id == businessPlanId, cancellationToken);

            if (businessPlan == null)
            {
                return Result.Failure<IEnumerable<BusinessPlanShareResponse>>(Error.NotFound("BusinessPlan.Error.NotFound", _localizationService.GetString("BusinessPlan.Error.NotFound")));
            }

            // Verify user has access
            var member = await _context.OrganizationMembers
                .FirstOrDefaultAsync(om => om.OrganizationId == businessPlan.OrganizationId && 
                                          om.UserId == currentUserId.Value && 
                                          om.IsActive, cancellationToken);

            if (member == null || (member.Role != OrganizationRole.Owner && member.Role != OrganizationRole.Admin))
            {
                return Result.Failure<IEnumerable<BusinessPlanShareResponse>>(Error.Forbidden("BusinessPlan.Error.Forbidden", _localizationService.GetString("BusinessPlan.Error.Forbidden")));
            }

            var shares = await _context.BusinessPlanShares
                .Include(bs => bs.SharedWithUser)
                .Where(bs => bs.BusinessPlanId == businessPlanId && bs.IsActive)
                .ToListAsync(cancellationToken);

            var responses = shares.Select(share => new BusinessPlanShareResponse
            {
                Id = share.Id,
                BusinessPlanId = share.BusinessPlanId,
                SharedWithUserId = share.SharedWithUserId,
                SharedWithEmail = share.SharedWithEmail,
                SharedWithUserName = share.SharedWithUser != null ? $"{share.SharedWithUser.FirstName} {share.SharedWithUser.LastName}" : null,
                Permission = (Sqordia.Contracts.Enums.SharePermission)share.Permission,
                PermissionName = share.Permission.ToString(),
                IsPublic = share.IsPublic,
                PublicToken = share.PublicToken,
                ExpiresAt = share.ExpiresAt,
                IsActive = share.IsActive,
                LastAccessedAt = share.LastAccessedAt,
                AccessCount = share.AccessCount,
                Created = share.Created,
                CreatedBy = share.CreatedBy ?? string.Empty
            });

            return Result.Success(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting shares for business plan {PlanId}", businessPlanId);
            return Result.Failure<IEnumerable<BusinessPlanShareResponse>>(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    public async Task<Result> RevokeShareAsync(
        Guid businessPlanId, 
        Guid shareId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUserId = _currentUserService.GetUserIdAsGuid();
            if (!currentUserId.HasValue)
            {
                return Result.Failure(Error.Unauthorized("General.Unauthorized", _localizationService.GetString("General.Unauthorized")));
            }

            var share = await _context.BusinessPlanShares
                .Include(bs => bs.BusinessPlan)
                .FirstOrDefaultAsync(bs => bs.Id == shareId && bs.BusinessPlanId == businessPlanId, cancellationToken);

            if (share == null)
            {
                return Result.Failure(Error.NotFound("Share.Error.NotFound", "Share not found"));
            }

            // Verify user has access
            var member = await _context.OrganizationMembers
                .FirstOrDefaultAsync(om => om.OrganizationId == share.BusinessPlan.OrganizationId && 
                                          om.UserId == currentUserId.Value && 
                                          om.IsActive, cancellationToken);

            if (member == null || (member.Role != OrganizationRole.Owner && member.Role != OrganizationRole.Admin))
            {
                return Result.Failure(Error.Forbidden("BusinessPlan.Error.Forbidden", _localizationService.GetString("BusinessPlan.Error.Forbidden")));
            }

            share.Revoke();
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Share {ShareId} revoked for business plan {PlanId} by {UserId}", 
                shareId, businessPlanId, currentUserId.Value);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking share {ShareId} for business plan {PlanId}", shareId, businessPlanId);
            return Result.Failure(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    public async Task<Result<BusinessPlanResponse>> GetBusinessPlanByPublicTokenAsync(
        string token, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var share = await _context.BusinessPlanShares
                .Include(bs => bs.BusinessPlan)
                    .ThenInclude(bp => bp.Organization)
                .FirstOrDefaultAsync(bs => bs.PublicToken == token && bs.IsPublic && bs.IsActive, cancellationToken);

            if (share == null || !share.CanAccess())
            {
                return Result.Failure<BusinessPlanResponse>(Error.NotFound("Share.Error.NotFound", "Invalid or expired share token"));
            }

            // Record access
            share.RecordAccess();
            await _context.SaveChangesAsync(cancellationToken);

            var businessPlan = share.BusinessPlan;
            var response = new BusinessPlanResponse
            {
                Id = businessPlan.Id,
                Title = businessPlan.Title,
                Description = businessPlan.Description,
                PlanType = businessPlan.PlanType.ToString(),
                Status = businessPlan.Status.ToString(),
                OrganizationId = businessPlan.OrganizationId,
                OrganizationName = businessPlan.Organization.Name,
                Version = businessPlan.Version,
                TotalQuestions = businessPlan.TotalQuestions,
                CompletedQuestions = businessPlan.CompletedQuestions,
                CompletionPercentage = businessPlan.CompletionPercentage,
                QuestionnaireCompletedAt = businessPlan.QuestionnaireCompletedAt,
                GenerationStartedAt = businessPlan.GenerationStartedAt,
                GenerationCompletedAt = businessPlan.GenerationCompletedAt,
                FinalizedAt = businessPlan.FinalizedAt,
                Created = businessPlan.Created,
                LastModified = businessPlan.LastModified,
                CreatedBy = businessPlan.CreatedBy ?? string.Empty
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting business plan by public token");
            return Result.Failure<BusinessPlanResponse>(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    public async Task<Result> UpdateSharePermissionAsync(
        Guid businessPlanId, 
        Guid shareId, 
        UpdateSharePermissionRequest request, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUserId = _currentUserService.GetUserIdAsGuid();
            if (!currentUserId.HasValue)
            {
                return Result.Failure(Error.Unauthorized("General.Unauthorized", _localizationService.GetString("General.Unauthorized")));
            }

            var share = await _context.BusinessPlanShares
                .Include(bs => bs.BusinessPlan)
                .FirstOrDefaultAsync(bs => bs.Id == shareId && bs.BusinessPlanId == businessPlanId, cancellationToken);

            if (share == null)
            {
                return Result.Failure(Error.NotFound("Share.Error.NotFound", "Share not found"));
            }

            // Verify user has access
            var member = await _context.OrganizationMembers
                .FirstOrDefaultAsync(om => om.OrganizationId == share.BusinessPlan.OrganizationId && 
                                          om.UserId == currentUserId.Value && 
                                          om.IsActive, cancellationToken);

            if (member == null || (member.Role != OrganizationRole.Owner && member.Role != OrganizationRole.Admin))
            {
                return Result.Failure(Error.Forbidden("BusinessPlan.Error.Forbidden", _localizationService.GetString("BusinessPlan.Error.Forbidden")));
            }

            share.UpdatePermission((Domain.Enums.SharePermission)request.Permission);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Share {ShareId} permission updated for business plan {PlanId} by {UserId}", 
                shareId, businessPlanId, currentUserId.Value);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating share permission {ShareId} for business plan {PlanId}", shareId, businessPlanId);
            return Result.Failure(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }
}


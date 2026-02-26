using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.TableOfContents;
using Sqordia.Contracts.Responses.TableOfContents;
using Sqordia.Domain.Entities.BusinessPlan;
using Sqordia.Domain.Enums;

namespace Sqordia.Application.Services.Implementations;

/// <summary>
/// Service implementation for managing business plan table of contents settings
/// </summary>
public class TableOfContentsService : ITableOfContentsService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<TableOfContentsService> _logger;
    private readonly ILocalizationService _localizationService;

    public TableOfContentsService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<TableOfContentsService> logger,
        ILocalizationService localizationService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
        _localizationService = localizationService;
    }

    public async Task<Result<TOCSettingsResponse>> GetSettingsAsync(Guid businessPlanId, CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUserId = _currentUserService.GetUserIdAsGuid();
            if (!currentUserId.HasValue)
            {
                return Result.Failure<TOCSettingsResponse>(Error.Unauthorized("General.Unauthorized", _localizationService.GetString("General.Unauthorized")));
            }

            // Verify business plan exists and user has access
            var businessPlan = await _context.BusinessPlans
                .FirstOrDefaultAsync(bp => bp.Id == businessPlanId && !bp.IsDeleted, cancellationToken);

            if (businessPlan == null)
            {
                return Result.Failure<TOCSettingsResponse>(Error.NotFound("BusinessPlan.Error.NotFound", _localizationService.GetString("BusinessPlan.Error.NotFound")));
            }

            // Verify user has access
            var hasAccess = await VerifyUserAccessAsync(businessPlan.OrganizationId, currentUserId.Value, cancellationToken);
            if (!hasAccess)
            {
                return Result.Failure<TOCSettingsResponse>(Error.Forbidden("BusinessPlan.Error.Forbidden", _localizationService.GetString("BusinessPlan.Error.Forbidden")));
            }

            // Get existing TOC settings or return default
            var tocSettings = await _context.TableOfContentsSettings
                .FirstOrDefaultAsync(toc => toc.BusinessPlanId == businessPlanId && !toc.IsDeleted, cancellationToken);

            if (tocSettings == null)
            {
                // Return default settings
                return Result.Success(new TOCSettingsResponse
                {
                    Id = Guid.Empty,
                    BusinessPlanId = businessPlanId,
                    Style = "classic",
                    ShowPageNumbers = true,
                    ShowIcons = true,
                    ShowCategoryHeaders = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            return Result.Success(MapToResponse(tocSettings));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting TOC settings for business plan {BusinessPlanId}", businessPlanId);
            return Result.Failure<TOCSettingsResponse>(Error.Failure("TOC.Error.GetFailed", "Failed to get table of contents settings"));
        }
    }

    public async Task<Result<TOCSettingsResponse>> UpdateSettingsAsync(Guid businessPlanId, UpdateTOCSettingsRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUserId = _currentUserService.GetUserIdAsGuid();
            if (!currentUserId.HasValue)
            {
                return Result.Failure<TOCSettingsResponse>(Error.Unauthorized("General.Unauthorized", _localizationService.GetString("General.Unauthorized")));
            }

            // Verify business plan exists and user has access
            var businessPlan = await _context.BusinessPlans
                .FirstOrDefaultAsync(bp => bp.Id == businessPlanId && !bp.IsDeleted, cancellationToken);

            if (businessPlan == null)
            {
                return Result.Failure<TOCSettingsResponse>(Error.NotFound("BusinessPlan.Error.NotFound", _localizationService.GetString("BusinessPlan.Error.NotFound")));
            }

            // Verify user has edit access (Owner, Admin, or Member)
            var hasEditAccess = await VerifyUserEditAccessAsync(businessPlan.OrganizationId, currentUserId.Value, cancellationToken);
            if (!hasEditAccess)
            {
                return Result.Failure<TOCSettingsResponse>(Error.Forbidden("BusinessPlan.Error.Forbidden", _localizationService.GetString("BusinessPlan.Error.Forbidden")));
            }

            // Get existing TOC settings or create new one
            var tocSettings = await _context.TableOfContentsSettings
                .FirstOrDefaultAsync(toc => toc.BusinessPlanId == businessPlanId && !toc.IsDeleted, cancellationToken);

            if (tocSettings == null)
            {
                tocSettings = new TableOfContentsSettings(businessPlanId);
                _context.TableOfContentsSettings.Add(tocSettings);
            }

            // Update all fields
            tocSettings.UpdateStyle(request.Style);
            tocSettings.UpdateDisplayOptions(request.ShowPageNumbers, request.ShowIcons, request.ShowCategoryHeaders);
            tocSettings.UpdateStyleSettings(request.StyleSettingsJson);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("TOC settings updated for business plan {BusinessPlanId} by user {UserId}", businessPlanId, currentUserId);

            return Result.Success(MapToResponse(tocSettings));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating TOC settings for business plan {BusinessPlanId}", businessPlanId);
            return Result.Failure<TOCSettingsResponse>(Error.Failure("TOC.Error.UpdateFailed", "Failed to update table of contents settings"));
        }
    }

    private async Task<bool> VerifyUserAccessAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken)
    {
        var member = await _context.OrganizationMembers
            .FirstOrDefaultAsync(om => om.OrganizationId == organizationId &&
                                       om.UserId == userId &&
                                       om.IsActive, cancellationToken);
        return member != null;
    }

    private async Task<bool> VerifyUserEditAccessAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken)
    {
        var member = await _context.OrganizationMembers
            .FirstOrDefaultAsync(om => om.OrganizationId == organizationId &&
                                       om.UserId == userId &&
                                       om.IsActive, cancellationToken);

        if (member == null) return false;

        // Allow Owner, Admin, and Member roles to edit
        return member.Role == OrganizationRole.Owner ||
               member.Role == OrganizationRole.Admin ||
               member.Role == OrganizationRole.Member;
    }

    private static TOCSettingsResponse MapToResponse(TableOfContentsSettings settings)
    {
        return new TOCSettingsResponse
        {
            Id = settings.Id,
            BusinessPlanId = settings.BusinessPlanId,
            Style = settings.Style,
            ShowPageNumbers = settings.ShowPageNumbers,
            ShowIcons = settings.ShowIcons,
            ShowCategoryHeaders = settings.ShowCategoryHeaders,
            StyleSettingsJson = settings.StyleSettingsJson,
            CreatedAt = settings.Created,
            UpdatedAt = settings.LastModified
        };
    }
}

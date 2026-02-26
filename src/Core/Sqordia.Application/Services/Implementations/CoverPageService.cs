using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.CoverPage;
using Sqordia.Contracts.Responses.CoverPage;
using Sqordia.Domain.Entities.BusinessPlan;
using Sqordia.Domain.Enums;

namespace Sqordia.Application.Services.Implementations;

/// <summary>
/// Service implementation for managing business plan cover page settings
/// </summary>
public class CoverPageService : ICoverPageService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CoverPageService> _logger;
    private readonly ILocalizationService _localizationService;

    public CoverPageService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<CoverPageService> logger,
        ILocalizationService localizationService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
        _localizationService = localizationService;
    }

    public async Task<Result<CoverPageResponse>> GetCoverPageAsync(Guid businessPlanId, CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUserId = _currentUserService.GetUserIdAsGuid();
            if (!currentUserId.HasValue)
            {
                return Result.Failure<CoverPageResponse>(Error.Unauthorized("General.Unauthorized", _localizationService.GetString("General.Unauthorized")));
            }

            // Verify business plan exists and user has access
            var businessPlan = await _context.BusinessPlans
                .FirstOrDefaultAsync(bp => bp.Id == businessPlanId && !bp.IsDeleted, cancellationToken);

            if (businessPlan == null)
            {
                return Result.Failure<CoverPageResponse>(Error.NotFound("BusinessPlan.Error.NotFound", _localizationService.GetString("BusinessPlan.Error.NotFound")));
            }

            // Verify user has access
            var hasAccess = await VerifyUserAccessAsync(businessPlan.OrganizationId, currentUserId.Value, cancellationToken);
            if (!hasAccess)
            {
                return Result.Failure<CoverPageResponse>(Error.Forbidden("BusinessPlan.Error.Forbidden", _localizationService.GetString("BusinessPlan.Error.Forbidden")));
            }

            // Get existing cover page settings or create default
            var coverPage = await _context.CoverPageSettings
                .FirstOrDefaultAsync(cp => cp.BusinessPlanId == businessPlanId && !cp.IsDeleted, cancellationToken);

            if (coverPage == null)
            {
                // Return default settings based on business plan
                return Result.Success(new CoverPageResponse
                {
                    Id = Guid.Empty,
                    BusinessPlanId = businessPlanId,
                    CompanyName = businessPlan.Title,
                    DocumentTitle = "Business Plan",
                    PrimaryColor = "#2563EB",
                    LayoutStyle = "classic",
                    PreparedDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                });
            }

            return Result.Success(MapToResponse(coverPage));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cover page for business plan {BusinessPlanId}", businessPlanId);
            return Result.Failure<CoverPageResponse>(Error.Failure("CoverPage.Error.GetFailed", "Failed to get cover page settings"));
        }
    }

    public async Task<Result<CoverPageResponse>> UpdateCoverPageAsync(Guid businessPlanId, UpdateCoverPageRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUserId = _currentUserService.GetUserIdAsGuid();
            if (!currentUserId.HasValue)
            {
                return Result.Failure<CoverPageResponse>(Error.Unauthorized("General.Unauthorized", _localizationService.GetString("General.Unauthorized")));
            }

            // Verify business plan exists and user has access
            var businessPlan = await _context.BusinessPlans
                .FirstOrDefaultAsync(bp => bp.Id == businessPlanId && !bp.IsDeleted, cancellationToken);

            if (businessPlan == null)
            {
                return Result.Failure<CoverPageResponse>(Error.NotFound("BusinessPlan.Error.NotFound", _localizationService.GetString("BusinessPlan.Error.NotFound")));
            }

            // Verify user has edit access (Owner, Admin, or Collaborator)
            var hasEditAccess = await VerifyUserEditAccessAsync(businessPlan.OrganizationId, currentUserId.Value, cancellationToken);
            if (!hasEditAccess)
            {
                return Result.Failure<CoverPageResponse>(Error.Forbidden("BusinessPlan.Error.Forbidden", _localizationService.GetString("BusinessPlan.Error.Forbidden")));
            }

            // Get existing cover page or create new one
            var coverPage = await _context.CoverPageSettings
                .FirstOrDefaultAsync(cp => cp.BusinessPlanId == businessPlanId && !cp.IsDeleted, cancellationToken);

            if (coverPage == null)
            {
                coverPage = new CoverPageSettings(businessPlanId, request.CompanyName);
                _context.CoverPageSettings.Add(coverPage);
            }

            // Update all fields
            coverPage.UpdateBranding(
                request.CompanyName,
                request.DocumentTitle,
                request.PrimaryColor,
                request.LayoutStyle
            );

            coverPage.UpdateLogo(request.LogoUrl);

            coverPage.UpdateContactInfo(
                request.ContactName,
                request.ContactTitle,
                request.ContactPhone,
                request.ContactEmail,
                request.Website
            );

            coverPage.UpdateAddress(
                request.AddressLine1,
                request.AddressLine2,
                request.City,
                request.StateProvince,
                request.PostalCode,
                request.Country
            );

            if (request.PreparedDate.HasValue)
            {
                coverPage.UpdatePreparedDate(request.PreparedDate.Value);
            }

            coverPage.UpdateStyleSettings(request.StyleSettingsJson);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Cover page updated for business plan {BusinessPlanId} by user {UserId}", businessPlanId, currentUserId);

            return Result.Success(MapToResponse(coverPage));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cover page for business plan {BusinessPlanId}", businessPlanId);
            return Result.Failure<CoverPageResponse>(Error.Failure("CoverPage.Error.UpdateFailed", "Failed to update cover page settings"));
        }
    }

    public async Task<Result<string>> UploadLogoAsync(Guid businessPlanId, Stream logoStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUserId = _currentUserService.GetUserIdAsGuid();
            if (!currentUserId.HasValue)
            {
                return Result.Failure<string>(Error.Unauthorized("General.Unauthorized", _localizationService.GetString("General.Unauthorized")));
            }

            // Verify business plan exists and user has access
            var businessPlan = await _context.BusinessPlans
                .FirstOrDefaultAsync(bp => bp.Id == businessPlanId && !bp.IsDeleted, cancellationToken);

            if (businessPlan == null)
            {
                return Result.Failure<string>(Error.NotFound("BusinessPlan.Error.NotFound", _localizationService.GetString("BusinessPlan.Error.NotFound")));
            }

            // Verify user has edit access
            var hasEditAccess = await VerifyUserEditAccessAsync(businessPlan.OrganizationId, currentUserId.Value, cancellationToken);
            if (!hasEditAccess)
            {
                return Result.Failure<string>(Error.Forbidden("BusinessPlan.Error.Forbidden", _localizationService.GetString("BusinessPlan.Error.Forbidden")));
            }

            // Validate file type
            var allowedTypes = new[] { "image/png", "image/jpeg", "image/jpg", "image/svg+xml", "image/webp" };
            if (!allowedTypes.Contains(contentType.ToLowerInvariant()))
            {
                return Result.Failure<string>(Error.Validation("CoverPage.Error.InvalidFileType", "Logo must be PNG, JPG, SVG, or WebP"));
            }

            // Validate file size (2MB max)
            if (logoStream.Length > 2 * 1024 * 1024)
            {
                return Result.Failure<string>(Error.Validation("CoverPage.Error.FileTooLarge", "Logo file must be less than 2MB"));
            }

            // For now, we'll use a data URL approach for simplicity
            // In production, you'd want to use cloud storage (Azure Blob, S3, etc.)
            using var memoryStream = new MemoryStream();
            await logoStream.CopyToAsync(memoryStream, cancellationToken);
            var base64 = Convert.ToBase64String(memoryStream.ToArray());
            var dataUrl = $"data:{contentType};base64,{base64}";

            // Update cover page with new logo URL
            var coverPage = await _context.CoverPageSettings
                .FirstOrDefaultAsync(cp => cp.BusinessPlanId == businessPlanId && !cp.IsDeleted, cancellationToken);

            if (coverPage == null)
            {
                coverPage = new CoverPageSettings(businessPlanId, businessPlan.Title);
                _context.CoverPageSettings.Add(coverPage);
            }

            coverPage.UpdateLogo(dataUrl);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Logo uploaded for business plan {BusinessPlanId} by user {UserId}", businessPlanId, currentUserId);

            return Result.Success(dataUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading logo for business plan {BusinessPlanId}", businessPlanId);
            return Result.Failure<string>(Error.Failure("CoverPage.Error.UploadFailed", "Failed to upload logo"));
        }
    }

    public async Task<Result> DeleteLogoAsync(Guid businessPlanId, CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUserId = _currentUserService.GetUserIdAsGuid();
            if (!currentUserId.HasValue)
            {
                return Result.Failure(Error.Unauthorized("General.Unauthorized", _localizationService.GetString("General.Unauthorized")));
            }

            // Verify business plan exists and user has access
            var businessPlan = await _context.BusinessPlans
                .FirstOrDefaultAsync(bp => bp.Id == businessPlanId && !bp.IsDeleted, cancellationToken);

            if (businessPlan == null)
            {
                return Result.Failure(Error.NotFound("BusinessPlan.Error.NotFound", _localizationService.GetString("BusinessPlan.Error.NotFound")));
            }

            // Verify user has edit access
            var hasEditAccess = await VerifyUserEditAccessAsync(businessPlan.OrganizationId, currentUserId.Value, cancellationToken);
            if (!hasEditAccess)
            {
                return Result.Failure(Error.Forbidden("BusinessPlan.Error.Forbidden", _localizationService.GetString("BusinessPlan.Error.Forbidden")));
            }

            var coverPage = await _context.CoverPageSettings
                .FirstOrDefaultAsync(cp => cp.BusinessPlanId == businessPlanId && !cp.IsDeleted, cancellationToken);

            if (coverPage != null)
            {
                coverPage.UpdateLogo(null);
                await _context.SaveChangesAsync(cancellationToken);
            }

            _logger.LogInformation("Logo deleted for business plan {BusinessPlanId} by user {UserId}", businessPlanId, currentUserId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting logo for business plan {BusinessPlanId}", businessPlanId);
            return Result.Failure(Error.Failure("CoverPage.Error.DeleteFailed", "Failed to delete logo"));
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

    private static CoverPageResponse MapToResponse(CoverPageSettings coverPage)
    {
        return new CoverPageResponse
        {
            Id = coverPage.Id,
            BusinessPlanId = coverPage.BusinessPlanId,
            LogoUrl = coverPage.LogoUrl,
            CompanyName = coverPage.CompanyName,
            DocumentTitle = coverPage.DocumentTitle,
            PrimaryColor = coverPage.PrimaryColor,
            LayoutStyle = coverPage.LayoutStyle,
            ContactName = coverPage.ContactName,
            ContactTitle = coverPage.ContactTitle,
            ContactPhone = coverPage.ContactPhone,
            ContactEmail = coverPage.ContactEmail,
            Website = coverPage.Website,
            AddressLine1 = coverPage.AddressLine1,
            AddressLine2 = coverPage.AddressLine2,
            City = coverPage.City,
            StateProvince = coverPage.StateProvince,
            PostalCode = coverPage.PostalCode,
            Country = coverPage.Country,
            PreparedDate = coverPage.PreparedDate,
            CreatedAt = coverPage.Created,
            UpdatedAt = coverPage.LastModified,
            StyleSettingsJson = coverPage.StyleSettingsJson
        };
    }
}

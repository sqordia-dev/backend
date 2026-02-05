using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Services.Cms;
using Sqordia.Contracts.Requests.Cms;
using Sqordia.Contracts.Responses.Cms;
using Sqordia.Domain.Entities.Cms;

namespace Sqordia.Application.Services.Implementations.Cms;

public class CmsAssetService : ICmsAssetService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IStorageService _storageService;
    private readonly ILogger<CmsAssetService> _logger;

    public CmsAssetService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IStorageService storageService,
        ILogger<CmsAssetService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<Result<CmsAssetResponse>> UploadAssetAsync(UploadCmsAssetRequest request, CancellationToken cancellationToken = default)
    {
        if (!_currentUserService.IsAuthenticated)
        {
            return Result.Failure<CmsAssetResponse>(Error.Unauthorized("Cms.Asset.Unauthorized", "You must be authenticated to upload assets."));
        }

        try
        {
            var userId = Guid.Parse(_currentUserService.UserId!);

            var fileName = request.File.FileName;
            var contentType = request.File.ContentType;
            var fileSize = request.File.Length;

            var storageKey = $"cms-assets/{request.Category}/{Guid.NewGuid()}/{fileName}";

            string url;
            using (var stream = request.File.OpenReadStream())
            {
                url = await _storageService.UploadFileAsync(storageKey, stream, contentType, cancellationToken);
            }

            var asset = new CmsAsset(
                fileName,
                contentType,
                url,
                fileSize,
                userId,
                request.Category);

            _context.CmsAssets.Add(asset);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("CMS asset '{FileName}' uploaded by user {UserId} in category '{Category}'", fileName, userId, request.Category);

            return Result.Success(MapToAssetResponse(asset));
        }
        catch (Exception ex) when (IsCmsTableMissing(ex))
        {
            _logger.LogWarning(ex, "CMS tables not available (migration may not be applied).");
            return Result.Failure<CmsAssetResponse>(CmsTablesNotAvailableError);
        }
    }

    public async Task<Result<List<CmsAssetResponse>>> GetAssetsAsync(string? category = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.CmsAssets.AsQueryable();

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(a => a.Category == category);
            }

            var assets = await query
                .OrderByDescending(a => a.Created)
                .ToListAsync(cancellationToken);

            var responses = assets.Select(MapToAssetResponse).ToList();

            return Result.Success(responses);
        }
        catch (Exception ex) when (IsCmsTableMissing(ex))
        {
            _logger.LogWarning(ex, "CMS tables not available (migration may not be applied).");
            return Result.Failure<List<CmsAssetResponse>>(CmsTablesNotAvailableError);
        }
    }

    public async Task<Result> DeleteAssetAsync(Guid assetId, CancellationToken cancellationToken = default)
    {
        try
        {
            var asset = await _context.CmsAssets
                .FirstOrDefaultAsync(a => a.Id == assetId, cancellationToken);

            if (asset == null)
            {
                return Result.Failure(Error.NotFound("Cms.Asset.NotFound", $"CMS asset with ID '{assetId}' was not found."));
            }

            asset.IsDeleted = true;
            asset.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("CMS asset {AssetId} soft-deleted", assetId);

            return Result.Success();
        }
        catch (Exception ex) when (IsCmsTableMissing(ex))
        {
            _logger.LogWarning(ex, "CMS tables not available (migration may not be applied).");
            return Result.Failure(CmsTablesNotAvailableError);
        }
    }

    private static CmsAssetResponse MapToAssetResponse(CmsAsset asset)
    {
        return new CmsAssetResponse
        {
            Id = asset.Id,
            FileName = asset.FileName,
            ContentType = asset.ContentType,
            Url = asset.Url,
            FileSize = asset.FileSize,
            Category = asset.Category,
            CreatedAt = asset.Created
        };
    }

    private static readonly Error CmsTablesNotAvailableError = Error.Failure(
        "Cms.TablesNotAvailable",
        "CMS is not yet available. The database migration needs to be applied.");

    private static bool IsCmsTableMissing(Exception ex)
    {
        var message = ex.InnerException?.Message ?? ex.Message;
        return message.Contains("does not exist", StringComparison.OrdinalIgnoreCase)
            && message.Contains("relation", StringComparison.OrdinalIgnoreCase);
    }
}

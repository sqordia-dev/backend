using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Services.Cms;
using Sqordia.Contracts.Requests.Cms;
using Sqordia.Contracts.Responses.Cms;
using Sqordia.Domain.Entities.Cms;
using Sqordia.Domain.Enums;

namespace Sqordia.Application.Services.Implementations.Cms;

public class CmsVersionService : ICmsVersionService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICmsContentBlockService _contentBlockService;
    private readonly ILogger<CmsVersionService> _logger;

    public CmsVersionService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ICmsContentBlockService contentBlockService,
        ILogger<CmsVersionService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _contentBlockService = contentBlockService;
        _logger = logger;
    }

    public async Task<Result<CmsVersionDetailResponse?>> GetActiveVersionAsync(CancellationToken cancellationToken = default)
    {
        var draftVersion = await _context.CmsVersions
            .Include(v => v.ContentBlocks)
            .FirstOrDefaultAsync(v => v.Status == CmsVersionStatus.Draft, cancellationToken);

        if (draftVersion == null)
        {
            return Result.Success<CmsVersionDetailResponse?>(null);
        }

        var createdByUserName = await GetUserNameAsync(draftVersion.CreatedByUserId, cancellationToken);

        return Result.Success<CmsVersionDetailResponse?>(MapToVersionDetailResponse(draftVersion, createdByUserName));
    }

    public async Task<Result<CmsVersionDetailResponse>> GetVersionAsync(Guid versionId, CancellationToken cancellationToken = default)
    {
        var version = await _context.CmsVersions
            .Include(v => v.ContentBlocks)
            .FirstOrDefaultAsync(v => v.Id == versionId, cancellationToken);

        if (version == null)
        {
            return Result.Failure<CmsVersionDetailResponse>(Error.NotFound("Cms.Version.NotFound", $"CMS version with ID '{versionId}' was not found."));
        }

        var createdByUserName = await GetUserNameAsync(version.CreatedByUserId, cancellationToken);
        var publishedByUserName = version.PublishedByUserId.HasValue
            ? await GetUserNameAsync(version.PublishedByUserId.Value, cancellationToken)
            : null;

        return Result.Success(MapToVersionDetailResponse(version, createdByUserName, publishedByUserName));
    }

    public async Task<Result<List<CmsVersionResponse>>> GetAllVersionsAsync(CancellationToken cancellationToken = default)
    {
        var versions = await _context.CmsVersions
            .Include(v => v.ContentBlocks)
            .OrderByDescending(v => v.VersionNumber)
            .ToListAsync(cancellationToken);

        var userIds = versions
            .Select(v => v.CreatedByUserId)
            .Union(versions.Where(v => v.PublishedByUserId.HasValue).Select(v => v.PublishedByUserId!.Value))
            .Distinct()
            .ToList();

        var userNames = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.GetFullName(), cancellationToken);

        var responses = versions.Select(v =>
        {
            userNames.TryGetValue(v.CreatedByUserId, out var createdByName);
            string? publishedByName = null;
            if (v.PublishedByUserId.HasValue)
            {
                userNames.TryGetValue(v.PublishedByUserId.Value, out publishedByName);
            }

            return MapToVersionResponse(v, createdByName, publishedByName);
        }).ToList();

        return Result.Success(responses);
    }

    public async Task<Result<CmsVersionDetailResponse>> CreateVersionAsync(CreateCmsVersionRequest request, CancellationToken cancellationToken = default)
    {
        if (!_currentUserService.IsAuthenticated)
        {
            return Result.Failure<CmsVersionDetailResponse>(Error.Unauthorized("Cms.Version.Unauthorized", "You must be authenticated to create a CMS version."));
        }

        var userId = Guid.Parse(_currentUserService.UserId!);

        // Check for existing draft (global - only one draft at a time)
        var existingDraft = await _context.CmsVersions
            .AnyAsync(v => v.Status == CmsVersionStatus.Draft, cancellationToken);

        if (existingDraft)
        {
            return Result.Failure<CmsVersionDetailResponse>(Error.Conflict("Cms.Version.DraftAlreadyExists", "A draft version already exists. Only one draft version can exist at a time."));
        }

        // Get the next version number
        var maxVersionNumber = await _context.CmsVersions
            .MaxAsync(v => (int?)v.VersionNumber, cancellationToken) ?? 0;

        var newVersionNumber = maxVersionNumber + 1;

        var version = new CmsVersion(newVersionNumber, userId, request.Notes);

        _context.CmsVersions.Add(version);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("CMS version {VersionNumber} created by user {UserId}", newVersionNumber, userId);

        // Auto-clone blocks from the latest published version if one exists
        var cloneResult = await _contentBlockService.CloneBlocksFromPublishedAsync(version.Id, cancellationToken);

        // Reload the version with its content blocks
        var createdVersion = await _context.CmsVersions
            .Include(v => v.ContentBlocks)
            .FirstAsync(v => v.Id == version.Id, cancellationToken);

        var createdByUserName = await GetUserNameAsync(userId, cancellationToken);

        return Result.Success(MapToVersionDetailResponse(createdVersion, createdByUserName));
    }

    public async Task<Result<CmsVersionResponse>> UpdateVersionAsync(Guid versionId, UpdateCmsVersionRequest request, CancellationToken cancellationToken = default)
    {
        var version = await _context.CmsVersions
            .Include(v => v.ContentBlocks)
            .FirstOrDefaultAsync(v => v.Id == versionId, cancellationToken);

        if (version == null)
        {
            return Result.Failure<CmsVersionResponse>(Error.NotFound("Cms.Version.NotFound", $"CMS version with ID '{versionId}' was not found."));
        }

        if (version.Status != CmsVersionStatus.Draft)
        {
            return Result.Failure<CmsVersionResponse>(Error.Validation("Cms.Version.NotDraft", "Only draft versions can be updated."));
        }

        // Update notes using EF Core's DbContext.Entry to set private setter property
        var dbContext = (DbContext)_context;
        dbContext.Entry(version).Property("Notes").CurrentValue = request.Notes;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("CMS version {VersionId} notes updated", versionId);

        var createdByUserName = await GetUserNameAsync(version.CreatedByUserId, cancellationToken);

        return Result.Success(MapToVersionResponse(version, createdByUserName));
    }

    public async Task<Result<CmsVersionResponse>> PublishVersionAsync(Guid versionId, CancellationToken cancellationToken = default)
    {
        if (!_currentUserService.IsAuthenticated)
        {
            return Result.Failure<CmsVersionResponse>(Error.Unauthorized("Cms.Version.Unauthorized", "You must be authenticated to publish a CMS version."));
        }

        var userId = Guid.Parse(_currentUserService.UserId!);

        var version = await _context.CmsVersions
            .Include(v => v.ContentBlocks)
            .FirstOrDefaultAsync(v => v.Id == versionId, cancellationToken);

        if (version == null)
        {
            return Result.Failure<CmsVersionResponse>(Error.NotFound("Cms.Version.NotFound", $"CMS version with ID '{versionId}' was not found."));
        }

        if (version.Status != CmsVersionStatus.Draft)
        {
            return Result.Failure<CmsVersionResponse>(Error.Validation("Cms.Version.NotDraft", "Only draft versions can be published."));
        }

        // Archive the currently published version (if any)
        var currentlyPublished = await _context.CmsVersions
            .FirstOrDefaultAsync(v => v.Status == CmsVersionStatus.Published, cancellationToken);

        if (currentlyPublished != null)
        {
            currentlyPublished.Archive();
            _logger.LogInformation("CMS version {VersionId} archived", currentlyPublished.Id);
        }

        // Publish the new version
        version.Publish(userId);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("CMS version {VersionId} published by user {UserId}", versionId, userId);

        var createdByUserName = await GetUserNameAsync(version.CreatedByUserId, cancellationToken);
        var publishedByUserName = await GetUserNameAsync(userId, cancellationToken);

        return Result.Success(MapToVersionResponse(version, createdByUserName, publishedByUserName));
    }

    public async Task<Result> DeleteVersionAsync(Guid versionId, CancellationToken cancellationToken = default)
    {
        var version = await _context.CmsVersions
            .FirstOrDefaultAsync(v => v.Id == versionId, cancellationToken);

        if (version == null)
        {
            return Result.Failure(Error.NotFound("Cms.Version.NotFound", $"CMS version with ID '{versionId}' was not found."));
        }

        if (version.Status != CmsVersionStatus.Draft)
        {
            return Result.Failure(Error.Validation("Cms.Version.NotDraft", "Only draft versions can be deleted."));
        }

        version.IsDeleted = true;
        version.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("CMS version {VersionId} soft-deleted", versionId);

        return Result.Success();
    }

    private async Task<string?> GetUserNameAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        return user?.GetFullName();
    }

    private CmsVersionResponse MapToVersionResponse(CmsVersion version, string? createdByUserName = null, string? publishedByUserName = null)
    {
        return new CmsVersionResponse
        {
            Id = version.Id,
            VersionNumber = version.VersionNumber,
            Status = version.Status.ToString(),
            Notes = version.Notes,
            CreatedByUserId = version.CreatedByUserId,
            CreatedByUserName = createdByUserName,
            PublishedAt = version.PublishedAt,
            PublishedByUserName = publishedByUserName,
            ContentBlockCount = version.ContentBlocks?.Count ?? 0,
            CreatedAt = version.Created,
            UpdatedAt = version.LastModified
        };
    }

    private CmsVersionDetailResponse MapToVersionDetailResponse(CmsVersion version, string? createdByUserName = null, string? publishedByUserName = null)
    {
        return new CmsVersionDetailResponse
        {
            Id = version.Id,
            VersionNumber = version.VersionNumber,
            Status = version.Status.ToString(),
            Notes = version.Notes,
            CreatedByUserId = version.CreatedByUserId,
            CreatedByUserName = createdByUserName,
            PublishedAt = version.PublishedAt,
            PublishedByUserName = publishedByUserName,
            ContentBlockCount = version.ContentBlocks?.Count ?? 0,
            CreatedAt = version.Created,
            UpdatedAt = version.LastModified,
            ContentBlocks = version.ContentBlocks?.Select(MapToBlockResponse).OrderBy(b => b.SortOrder).ToList() ?? new List<CmsContentBlockResponse>()
        };
    }

    private static CmsContentBlockResponse MapToBlockResponse(CmsContentBlock block)
    {
        return new CmsContentBlockResponse
        {
            Id = block.Id,
            BlockKey = block.BlockKey,
            BlockType = block.BlockType.ToString(),
            Content = block.Content,
            Language = block.Language,
            SortOrder = block.SortOrder,
            SectionKey = block.SectionKey,
            Metadata = block.Metadata,
            CreatedAt = block.Created,
            UpdatedAt = block.LastModified
        };
    }
}

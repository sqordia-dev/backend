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

public class CmsContentBlockService : ICmsContentBlockService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CmsContentBlockService> _logger;

    public CmsContentBlockService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<CmsContentBlockService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<List<CmsContentBlockResponse>>> GetBlocksByVersionAsync(
        Guid versionId,
        string? sectionKey = null,
        string? language = null,
        CancellationToken cancellationToken = default)
    {
        var versionExists = await _context.CmsVersions
            .AnyAsync(v => v.Id == versionId, cancellationToken);

        if (!versionExists)
        {
            return Result.Failure<List<CmsContentBlockResponse>>(Error.NotFound("Cms.Version.NotFound", $"CMS version with ID '{versionId}' was not found."));
        }

        var query = _context.CmsContentBlocks
            .Where(b => b.CmsVersionId == versionId);

        if (!string.IsNullOrWhiteSpace(sectionKey))
        {
            query = query.Where(b => b.SectionKey == sectionKey);
        }

        if (!string.IsNullOrWhiteSpace(language))
        {
            query = query.Where(b => b.Language == language);
        }

        var blocks = await query
            .OrderBy(b => b.SortOrder)
            .ToListAsync(cancellationToken);

        var responses = blocks.Select(MapToBlockResponse).ToList();

        return Result.Success(responses);
    }

    public async Task<Result<CmsContentBlockResponse>> GetBlockAsync(Guid blockId, CancellationToken cancellationToken = default)
    {
        var block = await _context.CmsContentBlocks
            .FirstOrDefaultAsync(b => b.Id == blockId, cancellationToken);

        if (block == null)
        {
            return Result.Failure<CmsContentBlockResponse>(Error.NotFound("Cms.ContentBlock.NotFound", $"Content block with ID '{blockId}' was not found."));
        }

        return Result.Success(MapToBlockResponse(block));
    }

    public async Task<Result<CmsContentBlockResponse>> CreateBlockAsync(
        Guid versionId,
        CreateContentBlockRequest request,
        CancellationToken cancellationToken = default)
    {
        var version = await _context.CmsVersions
            .Include(v => v.ContentBlocks)
            .FirstOrDefaultAsync(v => v.Id == versionId, cancellationToken);

        if (version == null)
        {
            return Result.Failure<CmsContentBlockResponse>(Error.NotFound("Cms.Version.NotFound", $"CMS version with ID '{versionId}' was not found."));
        }

        if (version.Status != CmsVersionStatus.Draft)
        {
            return Result.Failure<CmsContentBlockResponse>(Error.Validation("Cms.Version.NotDraft", "Content blocks can only be added to draft versions."));
        }

        if (!Enum.TryParse<CmsBlockType>(request.BlockType, true, out var blockType))
        {
            return Result.Failure<CmsContentBlockResponse>(Error.Validation("Cms.ContentBlock.InvalidBlockType", $"Invalid block type '{request.BlockType}'. Valid types are: {string.Join(", ", Enum.GetNames<CmsBlockType>())}"));
        }

        var block = new CmsContentBlock(
            versionId,
            request.BlockKey,
            blockType,
            request.Content,
            request.SectionKey,
            request.SortOrder,
            request.Language,
            request.Metadata);

        version.AddContentBlock(block);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Content block '{BlockKey}' created for version {VersionId}", request.BlockKey, versionId);

        return Result.Success(MapToBlockResponse(block));
    }

    public async Task<Result<CmsContentBlockResponse>> UpdateBlockAsync(
        Guid blockId,
        UpdateContentBlockRequest request,
        CancellationToken cancellationToken = default)
    {
        var block = await _context.CmsContentBlocks
            .Include(b => b.Version)
            .FirstOrDefaultAsync(b => b.Id == blockId, cancellationToken);

        if (block == null)
        {
            return Result.Failure<CmsContentBlockResponse>(Error.NotFound("Cms.ContentBlock.NotFound", $"Content block with ID '{blockId}' was not found."));
        }

        if (block.Version.Status != CmsVersionStatus.Draft)
        {
            return Result.Failure<CmsContentBlockResponse>(Error.Validation("Cms.Version.NotDraft", "Content blocks can only be updated in draft versions."));
        }

        block.UpdateContent(request.Content);

        if (request.SortOrder.HasValue)
        {
            block.UpdateSortOrder(request.SortOrder.Value);
        }

        if (request.Metadata != null)
        {
            block.UpdateMetadata(request.Metadata);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Content block {BlockId} updated", blockId);

        return Result.Success(MapToBlockResponse(block));
    }

    public async Task<Result<List<CmsContentBlockResponse>>> BulkUpdateBlocksAsync(
        Guid versionId,
        BulkUpdateContentBlocksRequest request,
        CancellationToken cancellationToken = default)
    {
        var version = await _context.CmsVersions
            .FirstOrDefaultAsync(v => v.Id == versionId, cancellationToken);

        if (version == null)
        {
            return Result.Failure<List<CmsContentBlockResponse>>(Error.NotFound("Cms.Version.NotFound", $"CMS version with ID '{versionId}' was not found."));
        }

        if (version.Status != CmsVersionStatus.Draft)
        {
            return Result.Failure<List<CmsContentBlockResponse>>(Error.Validation("Cms.Version.NotDraft", "Content blocks can only be updated in draft versions."));
        }

        var blockIds = request.Blocks.Select(b => b.Id).ToList();
        var blocks = await _context.CmsContentBlocks
            .Where(b => blockIds.Contains(b.Id) && b.CmsVersionId == versionId)
            .ToListAsync(cancellationToken);

        if (blocks.Count != request.Blocks.Count)
        {
            var foundIds = blocks.Select(b => b.Id).ToHashSet();
            var missingIds = blockIds.Where(id => !foundIds.Contains(id)).ToList();
            return Result.Failure<List<CmsContentBlockResponse>>(Error.NotFound("Cms.ContentBlock.NotFound", $"Content blocks not found: {string.Join(", ", missingIds)}"));
        }

        var blockDict = blocks.ToDictionary(b => b.Id);

        foreach (var item in request.Blocks)
        {
            var block = blockDict[item.Id];
            block.UpdateContent(item.Content);

            if (item.SortOrder.HasValue)
            {
                block.UpdateSortOrder(item.SortOrder.Value);
            }

            if (item.Metadata != null)
            {
                block.UpdateMetadata(item.Metadata);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Bulk updated {Count} content blocks for version {VersionId}", request.Blocks.Count, versionId);

        var responses = blocks.Select(MapToBlockResponse).ToList();

        return Result.Success(responses);
    }

    public async Task<Result> ReorderBlocksAsync(
        Guid versionId,
        ReorderContentBlocksRequest request,
        CancellationToken cancellationToken = default)
    {
        var version = await _context.CmsVersions
            .FirstOrDefaultAsync(v => v.Id == versionId, cancellationToken);

        if (version == null)
        {
            return Result.Failure(Error.NotFound("Cms.Version.NotFound", $"CMS version with ID '{versionId}' was not found."));
        }

        if (version.Status != CmsVersionStatus.Draft)
        {
            return Result.Failure(Error.Validation("Cms.Version.NotDraft", "Content blocks can only be reordered in draft versions."));
        }

        var blockIds = request.Items.Select(i => i.BlockId).ToList();
        var blocks = await _context.CmsContentBlocks
            .Where(b => blockIds.Contains(b.Id) && b.CmsVersionId == versionId)
            .ToListAsync(cancellationToken);

        var blockDict = blocks.ToDictionary(b => b.Id);

        foreach (var item in request.Items)
        {
            if (blockDict.TryGetValue(item.BlockId, out var block))
            {
                block.UpdateSortOrder(item.NewSortOrder);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Reordered {Count} content blocks for version {VersionId}", request.Items.Count, versionId);

        return Result.Success();
    }

    public async Task<Result> DeleteBlockAsync(Guid blockId, CancellationToken cancellationToken = default)
    {
        var block = await _context.CmsContentBlocks
            .Include(b => b.Version)
            .FirstOrDefaultAsync(b => b.Id == blockId, cancellationToken);

        if (block == null)
        {
            return Result.Failure(Error.NotFound("Cms.ContentBlock.NotFound", $"Content block with ID '{blockId}' was not found."));
        }

        if (block.Version.Status != CmsVersionStatus.Draft)
        {
            return Result.Failure(Error.Validation("Cms.Version.NotDraft", "Content blocks can only be deleted from draft versions."));
        }

        block.IsDeleted = true;
        block.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Content block {BlockId} soft-deleted", blockId);

        return Result.Success();
    }

    public async Task<Result<List<CmsContentBlockResponse>>> CloneBlocksFromPublishedAsync(
        Guid targetVersionId,
        CancellationToken cancellationToken = default)
    {
        var latestPublished = await _context.CmsVersions
            .Include(v => v.ContentBlocks)
            .Where(v => v.Status == CmsVersionStatus.Published)
            .OrderByDescending(v => v.VersionNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (latestPublished == null || !latestPublished.ContentBlocks.Any())
        {
            // No published version or no blocks to clone - this is not an error
            return Result.Success(new List<CmsContentBlockResponse>());
        }

        var targetVersion = await _context.CmsVersions
            .FirstOrDefaultAsync(v => v.Id == targetVersionId, cancellationToken);

        if (targetVersion == null)
        {
            return Result.Failure<List<CmsContentBlockResponse>>(Error.NotFound("Cms.Version.NotFound", $"Target CMS version with ID '{targetVersionId}' was not found."));
        }

        var clonedBlocks = new List<CmsContentBlock>();

        foreach (var sourceBlock in latestPublished.ContentBlocks)
        {
            var clonedBlock = new CmsContentBlock(
                targetVersionId,
                sourceBlock.BlockKey,
                sourceBlock.BlockType,
                sourceBlock.Content,
                sourceBlock.SectionKey,
                sourceBlock.SortOrder,
                sourceBlock.Language,
                sourceBlock.Metadata);

            clonedBlocks.Add(clonedBlock);
            _context.CmsContentBlocks.Add(clonedBlock);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Cloned {Count} content blocks from published version {SourceVersionId} to version {TargetVersionId}",
            clonedBlocks.Count, latestPublished.Id, targetVersionId);

        var responses = clonedBlocks.Select(MapToBlockResponse).ToList();

        return Result.Success(responses);
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

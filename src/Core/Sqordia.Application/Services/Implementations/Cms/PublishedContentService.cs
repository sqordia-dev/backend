using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Services.Cms;
using Sqordia.Contracts.Responses.Cms;
using Sqordia.Domain.Entities.Cms;
using Sqordia.Domain.Enums;

namespace Sqordia.Application.Services.Implementations.Cms;

public class PublishedContentService : IPublishedContentService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<PublishedContentService> _logger;

    public PublishedContentService(
        IApplicationDbContext context,
        ILogger<PublishedContentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<PublishedContentResponse>> GetPublishedContentAsync(
        string? sectionKey = null,
        string language = "fr",
        CancellationToken cancellationToken = default)
    {
        var publishedVersion = await _context.CmsVersions
            .Include(v => v.ContentBlocks)
            .Where(v => v.Status == CmsVersionStatus.Published)
            .OrderByDescending(v => v.VersionNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (publishedVersion == null)
        {
            return Result.Failure<PublishedContentResponse>(Error.NotFound("Cms.PublishedContent.NotFound", "No published CMS version exists."));
        }

        var blocks = publishedVersion.ContentBlocks.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(sectionKey))
        {
            blocks = blocks.Where(b => b.SectionKey == sectionKey);
        }

        blocks = blocks.Where(b => b.Language == language);

        var sections = blocks
            .GroupBy(b => b.SectionKey)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(b => b.SortOrder).Select(MapToBlockResponse).ToList());

        var response = new PublishedContentResponse
        {
            Sections = sections
        };

        return Result.Success(response);
    }

    public async Task<Result<PublishedContentResponse>> GetPublishedContentByPageAsync(
        string pageKey,
        string language = "fr",
        CancellationToken cancellationToken = default)
    {
        var publishedVersion = await _context.CmsVersions
            .Include(v => v.ContentBlocks)
            .Where(v => v.Status == CmsVersionStatus.Published)
            .OrderByDescending(v => v.VersionNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (publishedVersion == null)
        {
            return Result.Failure<PublishedContentResponse>(Error.NotFound("Cms.PublishedContent.NotFound", "No published CMS version exists."));
        }

        var pagePrefix = pageKey + ".";
        var blocks = publishedVersion.ContentBlocks
            .Where(b => b.Language == language && (b.SectionKey.StartsWith(pagePrefix) || b.SectionKey == pageKey));

        var sections = blocks
            .GroupBy(b => b.SectionKey)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(b => b.SortOrder).Select(MapToBlockResponse).ToList());

        var response = new PublishedContentResponse
        {
            Sections = sections
        };

        return Result.Success(response);
    }

    public async Task<Result<CmsContentBlockResponse>> GetPublishedBlockAsync(
        string blockKey,
        string language = "fr",
        CancellationToken cancellationToken = default)
    {
        var publishedVersion = await _context.CmsVersions
            .Include(v => v.ContentBlocks)
            .Where(v => v.Status == CmsVersionStatus.Published)
            .OrderByDescending(v => v.VersionNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (publishedVersion == null)
        {
            return Result.Failure<CmsContentBlockResponse>(Error.NotFound("Cms.PublishedContent.NotFound", "No published CMS version exists."));
        }

        var block = publishedVersion.ContentBlocks
            .FirstOrDefault(b => b.BlockKey == blockKey && b.Language == language);

        if (block == null)
        {
            return Result.Failure<CmsContentBlockResponse>(Error.NotFound("Cms.ContentBlock.NotFound", $"Published content block with key '{blockKey}' and language '{language}' was not found."));
        }

        return Result.Success(MapToBlockResponse(block));
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

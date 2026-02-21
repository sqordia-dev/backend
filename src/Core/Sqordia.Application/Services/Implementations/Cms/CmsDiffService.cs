using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Services.Cms;
using Sqordia.Contracts.Responses.Cms;
using Sqordia.Domain.Enums;

namespace Sqordia.Application.Services.Implementations.Cms;

public class CmsDiffService : ICmsDiffService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CmsDiffService> _logger;

    public CmsDiffService(
        IApplicationDbContext context,
        ILogger<CmsDiffService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<CmsDiffResponse>> CompareVersionsAsync(
        Guid sourceVersionId,
        Guid targetVersionId,
        string? language = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Load source version with blocks
            var sourceVersion = await _context.CmsVersions
                .Include(v => v.ContentBlocks)
                .FirstOrDefaultAsync(v => v.Id == sourceVersionId, cancellationToken);

            if (sourceVersion == null)
            {
                return Result.Failure<CmsDiffResponse>(
                    Error.NotFound("Cms.Version.NotFound", $"Source version with ID '{sourceVersionId}' was not found."));
            }

            // Load target version with blocks
            var targetVersion = await _context.CmsVersions
                .Include(v => v.ContentBlocks)
                .FirstOrDefaultAsync(v => v.Id == targetVersionId, cancellationToken);

            if (targetVersion == null)
            {
                return Result.Failure<CmsDiffResponse>(
                    Error.NotFound("Cms.Version.NotFound", $"Target version with ID '{targetVersionId}' was not found."));
            }

            return CalculateDiff(sourceVersion, targetVersion, language);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error comparing versions {SourceId} and {TargetId}", sourceVersionId, targetVersionId);
            return Result.Failure<CmsDiffResponse>(
                Error.Failure("Cms.Diff.Error", "An error occurred while comparing versions."));
        }
    }

    public async Task<Result<CmsDiffResponse>> GetDraftVsPublishedDiffAsync(
        string? language = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get the draft version
            var draftVersion = await _context.CmsVersions
                .Include(v => v.ContentBlocks)
                .FirstOrDefaultAsync(v => v.Status == CmsVersionStatus.Draft, cancellationToken);

            if (draftVersion == null)
            {
                return Result.Failure<CmsDiffResponse>(
                    Error.NotFound("Cms.Version.NoDraft", "No draft version exists."));
            }

            // Get the published version
            var publishedVersion = await _context.CmsVersions
                .Include(v => v.ContentBlocks)
                .FirstOrDefaultAsync(v => v.Status == CmsVersionStatus.Published, cancellationToken);

            if (publishedVersion == null)
            {
                // If no published version, return empty diff (all blocks are "added")
                return CalculateDiffWithNoTarget(draftVersion, language);
            }

            return CalculateDiff(draftVersion, publishedVersion, language);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting draft vs published diff");
            return Result.Failure<CmsDiffResponse>(
                Error.Failure("Cms.Diff.Error", "An error occurred while generating the diff."));
        }
    }

    private Result<CmsDiffResponse> CalculateDiff(
        Domain.Entities.Cms.CmsVersion sourceVersion,
        Domain.Entities.Cms.CmsVersion targetVersion,
        string? language)
    {
        var sourceBlocks = sourceVersion.ContentBlocks
            .Where(b => language == null || b.Language.Equals(language, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var targetBlocks = targetVersion.ContentBlocks
            .Where(b => language == null || b.Language.Equals(language, StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Create lookup by BlockKey + Language
        var sourceMap = sourceBlocks.ToDictionary(b => $"{b.BlockKey}|{b.Language}");
        var targetMap = targetBlocks.ToDictionary(b => $"{b.BlockKey}|{b.Language}");

        var allKeys = sourceMap.Keys.Union(targetMap.Keys).ToList();
        var blockDiffs = new List<CmsBlockDiffResponse>();

        foreach (var key in allKeys)
        {
            sourceMap.TryGetValue(key, out var sourceBlock);
            targetMap.TryGetValue(key, out var targetBlock);

            CmsBlockDiffStatus status;
            if (sourceBlock != null && targetBlock == null)
            {
                status = CmsBlockDiffStatus.Added;
            }
            else if (sourceBlock == null && targetBlock != null)
            {
                status = CmsBlockDiffStatus.Removed;
            }
            else if (sourceBlock!.Content != targetBlock!.Content)
            {
                status = CmsBlockDiffStatus.Modified;
            }
            else
            {
                status = CmsBlockDiffStatus.Unchanged;
            }

            blockDiffs.Add(new CmsBlockDiffResponse
            {
                BlockKey = sourceBlock?.BlockKey ?? targetBlock!.BlockKey,
                SectionKey = sourceBlock?.SectionKey ?? targetBlock!.SectionKey,
                BlockType = (sourceBlock?.BlockType ?? targetBlock!.BlockType).ToString(),
                Language = sourceBlock?.Language ?? targetBlock!.Language,
                Status = status,
                SourceContent = sourceBlock?.Content,
                TargetContent = targetBlock?.Content,
                SourceBlockId = sourceBlock?.Id,
                TargetBlockId = targetBlock?.Id
            });
        }

        var summary = new CmsDiffSummary
        {
            TotalChanges = blockDiffs.Count(d => d.Status != CmsBlockDiffStatus.Unchanged),
            AddedCount = blockDiffs.Count(d => d.Status == CmsBlockDiffStatus.Added),
            RemovedCount = blockDiffs.Count(d => d.Status == CmsBlockDiffStatus.Removed),
            ModifiedCount = blockDiffs.Count(d => d.Status == CmsBlockDiffStatus.Modified),
            UnchangedCount = blockDiffs.Count(d => d.Status == CmsBlockDiffStatus.Unchanged)
        };

        return Result.Success(new CmsDiffResponse
        {
            SourceVersionId = sourceVersion.Id,
            SourceVersionNumber = sourceVersion.VersionNumber,
            TargetVersionId = targetVersion.Id,
            TargetVersionNumber = targetVersion.VersionNumber,
            BlockDiffs = blockDiffs.OrderBy(d => d.SectionKey).ThenBy(d => d.BlockKey).ToList(),
            Summary = summary
        });
    }

    private Result<CmsDiffResponse> CalculateDiffWithNoTarget(
        Domain.Entities.Cms.CmsVersion sourceVersion,
        string? language)
    {
        var sourceBlocks = sourceVersion.ContentBlocks
            .Where(b => language == null || b.Language.Equals(language, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var blockDiffs = sourceBlocks.Select(block => new CmsBlockDiffResponse
        {
            BlockKey = block.BlockKey,
            SectionKey = block.SectionKey,
            BlockType = block.BlockType.ToString(),
            Language = block.Language,
            Status = CmsBlockDiffStatus.Added,
            SourceContent = block.Content,
            TargetContent = null,
            SourceBlockId = block.Id,
            TargetBlockId = null
        }).ToList();

        var summary = new CmsDiffSummary
        {
            TotalChanges = blockDiffs.Count,
            AddedCount = blockDiffs.Count,
            RemovedCount = 0,
            ModifiedCount = 0,
            UnchangedCount = 0
        };

        return Result.Success(new CmsDiffResponse
        {
            SourceVersionId = sourceVersion.Id,
            SourceVersionNumber = sourceVersion.VersionNumber,
            TargetVersionId = Guid.Empty,
            TargetVersionNumber = 0,
            BlockDiffs = blockDiffs.OrderBy(d => d.SectionKey).ThenBy(d => d.BlockKey).ToList(),
            Summary = summary
        });
    }
}

using System.Text.Json;
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

/// <summary>
/// Service for managing CMS content templates.
/// </summary>
public class CmsTemplateService : ICmsTemplateService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CmsTemplateService> _logger;

    private static readonly Error TemplateNotFoundError = new("CmsTemplate.NotFound", "Template not found");
    private static readonly Error UnauthorizedError = new("CmsTemplate.Unauthorized", "User not authenticated");
    private static readonly Error AccessDeniedError = new("CmsTemplate.AccessDenied", "You do not have permission to modify this template");
    private static readonly Error VersionNotFoundError = new("CmsTemplate.VersionNotFound", "Version not found");
    private static readonly Error VersionNotDraftError = new("CmsTemplate.VersionNotDraft", "Can only apply templates to draft versions");

    public CmsTemplateService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<CmsTemplateService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<List<CmsContentTemplateSummaryResponse>>> GetTemplatesAsync(
        string? pageKey = null,
        string? sectionKey = null,
        CancellationToken cancellationToken = default)
    {
        var userIdStr = _currentUserService.UserId;
        Guid? userId = null;
        if (!string.IsNullOrEmpty(userIdStr) && Guid.TryParse(userIdStr, out var parsedUserId))
        {
            userId = parsedUserId;
        }

        var query = _context.CmsContentTemplates.AsNoTracking();

        // Filter by page/section if specified
        if (!string.IsNullOrEmpty(pageKey))
        {
            query = query.Where(t => t.PageKey == null || t.PageKey == pageKey);
        }

        if (!string.IsNullOrEmpty(sectionKey))
        {
            query = query.Where(t => t.SectionKey == null || t.SectionKey == sectionKey);
        }

        // Filter to public templates or templates created by current user
        query = query.Where(t => t.IsPublic || (userId.HasValue && t.CreatedByUserId == userId.Value));

        var templates = await query
            .OrderByDescending(t => t.Created)
            .Select(t => new CmsContentTemplateSummaryResponse
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                PageKey = t.PageKey,
                SectionKey = t.SectionKey,
                PreviewImageUrl = t.PreviewImageUrl,
                IsPublic = t.IsPublic,
                CreatedByUserId = t.CreatedByUserId,
                CreatedAt = t.Created,
                UpdatedAt = t.LastModified
            })
            .ToListAsync(cancellationToken);

        // Fetch user names
        var userIds = templates.Select(t => t.CreatedByUserId).Distinct().ToList();
        var userNames = await _context.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FirstName + " " + u.LastName, cancellationToken);

        foreach (var template in templates)
        {
            template.CreatedByUserName = userNames.GetValueOrDefault(template.CreatedByUserId);
        }

        return Result.Success(templates);
    }

    public async Task<Result<CmsContentTemplateResponse>> GetTemplateAsync(
        Guid templateId,
        CancellationToken cancellationToken = default)
    {
        var userIdStr = _currentUserService.UserId;
        Guid? userId = null;
        if (!string.IsNullOrEmpty(userIdStr) && Guid.TryParse(userIdStr, out var parsedUserId))
        {
            userId = parsedUserId;
        }

        var template = await _context.CmsContentTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == templateId, cancellationToken);

        if (template == null)
            return Result.Failure<CmsContentTemplateResponse>(TemplateNotFoundError);

        // Check access: must be public or created by current user
        if (!template.IsPublic && (!userId.HasValue || template.CreatedByUserId != userId.Value))
            return Result.Failure<CmsContentTemplateResponse>(AccessDeniedError);

        var createdByUserName = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == template.CreatedByUserId)
            .Select(u => u.FirstName + " " + u.LastName)
            .FirstOrDefaultAsync(cancellationToken);

        return Result.Success(new CmsContentTemplateResponse
        {
            Id = template.Id,
            Name = template.Name,
            Description = template.Description,
            PageKey = template.PageKey,
            SectionKey = template.SectionKey,
            TemplateData = template.TemplateData,
            PreviewImageUrl = template.PreviewImageUrl,
            IsPublic = template.IsPublic,
            CreatedByUserId = template.CreatedByUserId,
            CreatedByUserName = createdByUserName,
            CreatedAt = template.Created,
            UpdatedAt = template.LastModified
        });
    }

    public async Task<Result<CmsContentTemplateResponse>> CreateTemplateAsync(
        CreateCmsTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var userIdStr = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return Result.Failure<CmsContentTemplateResponse>(UnauthorizedError);

        var template = new CmsContentTemplate(
            request.Name,
            request.TemplateData,
            userId,
            request.Description,
            request.PageKey,
            request.SectionKey,
            request.PreviewImageUrl,
            request.IsPublic);

        _context.CmsContentTemplates.Add(template);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Template {TemplateId} created by user {UserId}", template.Id, userId);

        var createdByUserName = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.FirstName + " " + u.LastName)
            .FirstOrDefaultAsync(cancellationToken);

        return Result.Success(new CmsContentTemplateResponse
        {
            Id = template.Id,
            Name = template.Name,
            Description = template.Description,
            PageKey = template.PageKey,
            SectionKey = template.SectionKey,
            TemplateData = template.TemplateData,
            PreviewImageUrl = template.PreviewImageUrl,
            IsPublic = template.IsPublic,
            CreatedByUserId = template.CreatedByUserId,
            CreatedByUserName = createdByUserName,
            CreatedAt = template.Created,
            UpdatedAt = template.LastModified
        });
    }

    public async Task<Result<CmsContentTemplateResponse>> UpdateTemplateAsync(
        Guid templateId,
        UpdateCmsTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var userIdStr = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return Result.Failure<CmsContentTemplateResponse>(UnauthorizedError);

        var template = await _context.CmsContentTemplates
            .FirstOrDefaultAsync(t => t.Id == templateId, cancellationToken);

        if (template == null)
            return Result.Failure<CmsContentTemplateResponse>(TemplateNotFoundError);

        // Only the creator can update
        if (template.CreatedByUserId != userId)
            return Result.Failure<CmsContentTemplateResponse>(AccessDeniedError);

        template.UpdateMetadata(
            request.Name,
            request.Description,
            request.PageKey,
            request.SectionKey,
            request.PreviewImageUrl,
            request.IsPublic);

        if (!string.IsNullOrEmpty(request.TemplateData))
        {
            template.UpdateTemplateData(request.TemplateData);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Template {TemplateId} updated by user {UserId}", templateId, userId);

        var createdByUserName = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == template.CreatedByUserId)
            .Select(u => u.FirstName + " " + u.LastName)
            .FirstOrDefaultAsync(cancellationToken);

        return Result.Success(new CmsContentTemplateResponse
        {
            Id = template.Id,
            Name = template.Name,
            Description = template.Description,
            PageKey = template.PageKey,
            SectionKey = template.SectionKey,
            TemplateData = template.TemplateData,
            PreviewImageUrl = template.PreviewImageUrl,
            IsPublic = template.IsPublic,
            CreatedByUserId = template.CreatedByUserId,
            CreatedByUserName = createdByUserName,
            CreatedAt = template.Created,
            UpdatedAt = template.LastModified
        });
    }

    public async Task<Result> DeleteTemplateAsync(
        Guid templateId,
        CancellationToken cancellationToken = default)
    {
        var userIdStr = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return Result.Failure(UnauthorizedError);

        var template = await _context.CmsContentTemplates
            .FirstOrDefaultAsync(t => t.Id == templateId, cancellationToken);

        if (template == null)
            return Result.Failure(TemplateNotFoundError);

        // Only the creator can delete
        if (template.CreatedByUserId != userId)
            return Result.Failure(AccessDeniedError);

        _context.CmsContentTemplates.Remove(template);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Template {TemplateId} deleted by user {UserId}", templateId, userId);

        return Result.Success();
    }

    public async Task<Result<CmsContentTemplateResponse>> CreateTemplateFromSectionAsync(
        Guid versionId,
        string sectionKey,
        string language,
        string name,
        string? description = null,
        bool isPublic = false,
        CancellationToken cancellationToken = default)
    {
        var userIdStr = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return Result.Failure<CmsContentTemplateResponse>(UnauthorizedError);

        var version = await _context.CmsVersions
            .Include(v => v.ContentBlocks)
            .FirstOrDefaultAsync(v => v.Id == versionId, cancellationToken);

        if (version == null)
            return Result.Failure<CmsContentTemplateResponse>(VersionNotFoundError);

        // Get blocks for the specified section and language
        var blocks = version.ContentBlocks
            .Where(b => b.SectionKey == sectionKey && b.Language == language)
            .OrderBy(b => b.SortOrder)
            .Select(b => new TemplateBlockData
            {
                BlockKey = b.BlockKey,
                BlockType = b.BlockType.ToString(),
                Content = b.Content,
                SortOrder = b.SortOrder,
                Metadata = b.Metadata
            })
            .ToList();

        if (blocks.Count == 0)
            return Result.Failure<CmsContentTemplateResponse>(new Error("CmsTemplate.NoBlocks", "No blocks found in the specified section"));

        // Get the page key from the section (if available)
        var page = await _context.CmsSections
            .AsNoTracking()
            .Include(s => s.Page)
            .Where(s => s.Key == sectionKey)
            .Select(s => s.Page!.Key)
            .FirstOrDefaultAsync(cancellationToken);

        var templateData = JsonSerializer.Serialize(blocks);

        var template = new CmsContentTemplate(
            name,
            templateData,
            userId,
            description,
            page,
            sectionKey,
            null,
            isPublic);

        _context.CmsContentTemplates.Add(template);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Template {TemplateId} created from section {SectionKey} by user {UserId}", template.Id, sectionKey, userId);

        var createdByUserName = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.FirstName + " " + u.LastName)
            .FirstOrDefaultAsync(cancellationToken);

        return Result.Success(new CmsContentTemplateResponse
        {
            Id = template.Id,
            Name = template.Name,
            Description = template.Description,
            PageKey = template.PageKey,
            SectionKey = template.SectionKey,
            TemplateData = template.TemplateData,
            PreviewImageUrl = template.PreviewImageUrl,
            IsPublic = template.IsPublic,
            CreatedByUserId = template.CreatedByUserId,
            CreatedByUserName = createdByUserName,
            CreatedAt = template.Created,
            UpdatedAt = template.LastModified
        });
    }

    public async Task<Result<List<CmsContentBlockResponse>>> ApplyTemplateAsync(
        Guid versionId,
        Guid templateId,
        string sectionKey,
        string language,
        bool replaceExisting = false,
        CancellationToken cancellationToken = default)
    {
        var userIdStr = _currentUserService.UserId;
        Guid? userId = null;
        if (!string.IsNullOrEmpty(userIdStr) && Guid.TryParse(userIdStr, out var parsedUserId))
        {
            userId = parsedUserId;
        }

        var version = await _context.CmsVersions
            .Include(v => v.ContentBlocks)
            .FirstOrDefaultAsync(v => v.Id == versionId, cancellationToken);

        if (version == null)
            return Result.Failure<List<CmsContentBlockResponse>>(VersionNotFoundError);

        if (version.Status != CmsVersionStatus.Draft)
            return Result.Failure<List<CmsContentBlockResponse>>(VersionNotDraftError);

        var template = await _context.CmsContentTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == templateId, cancellationToken);

        if (template == null)
            return Result.Failure<List<CmsContentBlockResponse>>(TemplateNotFoundError);

        // Check access
        if (!template.IsPublic && (!userId.HasValue || template.CreatedByUserId != userId.Value))
            return Result.Failure<List<CmsContentBlockResponse>>(AccessDeniedError);

        // Parse template data
        var templateBlocks = JsonSerializer.Deserialize<List<TemplateBlockData>>(template.TemplateData);
        if (templateBlocks == null || templateBlocks.Count == 0)
            return Result.Failure<List<CmsContentBlockResponse>>(new Error("CmsTemplate.InvalidData", "Template has no valid block data"));

        // If replacing, remove existing blocks in the section
        if (replaceExisting)
        {
            var existingBlocks = version.ContentBlocks
                .Where(b => b.SectionKey == sectionKey && b.Language == language)
                .ToList();

            foreach (var block in existingBlocks)
            {
                version.RemoveContentBlock(block.Id);
            }
        }

        // Get current max sort order
        var maxSortOrder = version.ContentBlocks
            .Where(b => b.SectionKey == sectionKey && b.Language == language)
            .Select(b => b.SortOrder)
            .DefaultIfEmpty(-1)
            .Max();

        var createdBlocks = new List<CmsContentBlock>();

        foreach (var templateBlock in templateBlocks)
        {
            maxSortOrder++;

            // Parse the block type from string
            if (!Enum.TryParse<CmsBlockType>(templateBlock.BlockType, out var blockType))
            {
                blockType = CmsBlockType.Text; // Default to Text if parsing fails
            }

            var block = new CmsContentBlock(
                versionId,
                templateBlock.BlockKey,
                blockType,
                templateBlock.Content,
                sectionKey,
                maxSortOrder,
                language,
                templateBlock.Metadata);

            version.AddContentBlock(block);
            createdBlocks.Add(block);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Template {TemplateId} applied to version {VersionId} section {SectionKey} by user {UserId}", templateId, versionId, sectionKey, userId);

        var response = createdBlocks.Select(b => new CmsContentBlockResponse
        {
            Id = b.Id,
            BlockKey = b.BlockKey,
            BlockType = b.BlockType.ToString(),
            Content = b.Content,
            Language = b.Language,
            SortOrder = b.SortOrder,
            SectionKey = b.SectionKey,
            Metadata = b.Metadata,
            CreatedAt = b.Created,
            UpdatedAt = b.LastModified
        }).ToList();

        return Result.Success(response);
    }

    private class TemplateBlockData
    {
        public string BlockKey { get; set; } = string.Empty;
        public string BlockType { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public string? Metadata { get; set; }
    }
}

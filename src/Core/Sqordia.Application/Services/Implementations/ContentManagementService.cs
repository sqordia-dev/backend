using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Services;
using Sqordia.Contracts.Requests.Content;
using Sqordia.Contracts.Responses.Content;
using Sqordia.Domain.Entities;

namespace Sqordia.Application.Services.Implementations;

public class ContentManagementService : IContentManagementService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ContentManagementService> _logger;

    public ContentManagementService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<ContentManagementService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<ContentPageResponse?> GetContentPageAsync(
        string pageKey,
        string language = "fr",
        CancellationToken cancellationToken = default)
    {
        var page = await _context.ContentPages
            .FirstOrDefaultAsync(p => p.PageKey == pageKey && 
                                      p.Language == language &&
                                      p.IsPublished, cancellationToken);

        return page != null ? MapToResponse(page) : null;
    }

    public async Task<List<ContentPageResponse>> GetAllContentPagesAsync(
        CancellationToken cancellationToken = default)
    {
        var pages = await _context.ContentPages
            .OrderBy(p => p.PageKey)
            .ThenBy(p => p.Language)
            .ToListAsync(cancellationToken);

        return pages.Select(MapToResponse).ToList();
    }

    public async Task<ContentPageResponse> UpdateContentPageAsync(
        string pageKey,
        UpdateContentPageRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId?.ToString() ?? "System";
        
        var page = await _context.ContentPages
            .FirstOrDefaultAsync(p => p.PageKey == pageKey && p.Language == request.Language);

        if (page == null)
        {
            page = new ContentPage(pageKey, request.Title, request.Content, request.Language);
            page.CreatedBy = currentUserId;
            _context.ContentPages.Add(page);
        }
        else
        {
            page.UpdateContent(request.Title, request.Content);
            page.LastModifiedBy = currentUserId;
        }

        if (request.Publish)
        {
            page.Publish();
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Content page {PageKey} updated", pageKey);

        return MapToResponse(page);
    }

    public async Task<ContentPageResponse> PublishContentPageAsync(
        string pageKey,
        string language,
        CancellationToken cancellationToken = default)
    {
        var page = await _context.ContentPages
            .FirstOrDefaultAsync(p => p.PageKey == pageKey && p.Language == language);

        if (page == null)
        {
            throw new ArgumentException($"Content page {pageKey} ({language}) not found");
        }

        page.Publish();
        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponse(page);
    }

    private ContentPageResponse MapToResponse(ContentPage page)
    {
        return new ContentPageResponse
        {
            Id = page.Id,
            PageKey = page.PageKey,
            Title = page.Title,
            Content = page.Content,
            Language = page.Language,
            IsPublished = page.IsPublished,
            PublishedAt = page.PublishedAt,
            Version = page.Version,
            CreatedAt = page.Created,
            UpdatedAt = page.LastModified
        };
    }
}


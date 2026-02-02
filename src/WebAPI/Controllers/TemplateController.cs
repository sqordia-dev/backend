using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Templates.Commands;
using Sqordia.Application.Templates.Queries;
using Sqordia.Domain.Entities;
using Sqordia.Domain.Enums;

namespace WebAPI.Controllers;

[ApiVersion("1.0")]
[Authorize(Roles = "Admin")]
public class TemplateController : BaseApiController
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public TemplateController(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Get all templates
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<TemplateDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTemplates(CancellationToken cancellationToken)
    {
        var templates = await _context.Templates
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => MapToDto(t))
            .ToListAsync(cancellationToken);

        return Ok(templates);
    }

    /// <summary>
    /// Get a template by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TemplateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTemplate(Guid id, CancellationToken cancellationToken)
    {
        var template = await _context.Templates
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (template == null)
            return NotFound(new { message = "Template not found" });

        return Ok(MapToDto(template));
    }

    /// <summary>
    /// Create a new template
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TemplateDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTemplate(
        [FromBody] CreateTemplateCommand command,
        CancellationToken cancellationToken)
    {
        var template = Template.Create(
            name: command.Name,
            description: command.Description,
            content: command.Content,
            category: command.Category,
            type: command.Type,
            author: command.Author,
            authorEmail: command.AuthorEmail,
            createdBy: _currentUserService.UserId ?? "System",
            industry: command.Industry,
            targetAudience: command.TargetAudience,
            language: command.Language,
            country: command.Country,
            isPublic: command.IsPublic,
            tags: command.Tags,
            previewImage: command.PreviewImage,
            version: command.Version,
            changelog: command.Changelog);

        _context.Templates.Add(template);
        await _context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetTemplate), new { id = template.Id }, MapToDto(template));
    }

    /// <summary>
    /// Update a template
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TemplateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTemplate(
        Guid id,
        [FromBody] UpdateTemplateCommand command,
        CancellationToken cancellationToken)
    {
        var template = await _context.Templates
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (template == null)
            return NotFound(new { message = "Template not found" });

        template.Update(
            name: command.Name,
            description: command.Description,
            content: command.Content,
            category: command.Category,
            type: command.Type,
            industry: command.Industry,
            targetAudience: command.TargetAudience,
            language: command.Language,
            country: command.Country,
            isPublic: command.IsPublic,
            tags: command.Tags,
            previewImage: command.PreviewImage,
            author: command.Author,
            version: command.Version,
            changelog: command.Changelog,
            updatedBy: _currentUserService.UserId ?? "System");

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(MapToDto(template));
    }

    /// <summary>
    /// Delete a template
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTemplate(Guid id, CancellationToken cancellationToken)
    {
        var template = await _context.Templates
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (template == null)
            return NotFound(new { message = "Template not found" });

        _context.Templates.Remove(template);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { message = "Template deleted successfully" });
    }

    /// <summary>
    /// Search templates by name or description
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(List<TemplateDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchTemplates(
        [FromQuery] string searchTerm,
        CancellationToken cancellationToken)
    {
        var query = _context.Templates.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(t =>
                t.Name.ToLower().Contains(term) ||
                t.Description.ToLower().Contains(term) ||
                t.Industry.ToLower().Contains(term) ||
                t.Tags.ToLower().Contains(term));
        }

        var templates = await query
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => MapToDto(t))
            .ToListAsync(cancellationToken);

        return Ok(templates);
    }

    /// <summary>
    /// Get templates by category
    /// </summary>
    [HttpGet("category/{category}")]
    [ProducesResponseType(typeof(List<TemplateDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCategory(
        string category,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<TemplateCategory>(category, true, out var categoryEnum))
            return BadRequest(new { message = $"Invalid category: {category}" });

        var templates = await _context.Templates
            .AsNoTracking()
            .Where(t => t.Category == categoryEnum)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => MapToDto(t))
            .ToListAsync(cancellationToken);

        return Ok(templates);
    }

    /// <summary>
    /// Get templates by industry
    /// </summary>
    [HttpGet("industry/{industry}")]
    [ProducesResponseType(typeof(List<TemplateDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByIndustry(
        string industry,
        CancellationToken cancellationToken)
    {
        var templates = await _context.Templates
            .AsNoTracking()
            .Where(t => t.Industry.ToLower() == industry.ToLower())
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => MapToDto(t))
            .ToListAsync(cancellationToken);

        return Ok(templates);
    }

    /// <summary>
    /// Get public templates (no auth required)
    /// </summary>
    [HttpGet("public")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<TemplateDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPublicTemplates(CancellationToken cancellationToken)
    {
        var templates = await _context.Templates
            .AsNoTracking()
            .Where(t => t.IsPublic && t.Status == TemplateStatus.Published)
            .OrderByDescending(t => t.UsageCount)
            .Select(t => MapToDto(t))
            .ToListAsync(cancellationToken);

        return Ok(templates);
    }

    /// <summary>
    /// Clone a template
    /// </summary>
    [HttpPost("{id:guid}/clone")]
    [ProducesResponseType(typeof(TemplateDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CloneTemplate(
        Guid id,
        [FromBody] CloneTemplateRequest? request,
        CancellationToken cancellationToken)
    {
        var source = await _context.Templates
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (source == null)
            return NotFound(new { message = "Template not found" });

        var cloneName = request?.Name ?? $"Copy of {source.Name}";
        var clone = Template.Create(
            name: cloneName,
            description: source.Description,
            content: source.Content,
            category: source.Category,
            type: source.Type,
            author: source.Author,
            authorEmail: source.AuthorEmail,
            createdBy: _currentUserService.UserId ?? "System",
            industry: source.Industry,
            targetAudience: source.TargetAudience,
            language: source.Language,
            country: source.Country,
            isPublic: false,
            tags: source.Tags,
            previewImage: source.PreviewImage,
            version: source.Version);

        _context.Templates.Add(clone);
        await _context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetTemplate), new { id = clone.Id }, MapToDto(clone));
    }

    /// <summary>
    /// Publish a template
    /// </summary>
    [HttpPost("{id:guid}/publish")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PublishTemplate(Guid id, CancellationToken cancellationToken)
    {
        var template = await _context.Templates
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (template == null)
            return NotFound(new { message = "Template not found" });

        template.Publish();
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { message = "Template published successfully" });
    }

    /// <summary>
    /// Archive a template
    /// </summary>
    [HttpPost("{id:guid}/archive")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ArchiveTemplate(Guid id, CancellationToken cancellationToken)
    {
        var template = await _context.Templates
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (template == null)
            return NotFound(new { message = "Template not found" });

        template.Archive();
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { message = "Template archived successfully" });
    }

    private static TemplateDto MapToDto(Template t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        Description = t.Description,
        Content = t.Content,
        Category = t.Category.ToString(),
        Type = t.Type.ToString(),
        Industry = t.Industry,
        TargetAudience = t.TargetAudience,
        Language = t.Language,
        Country = t.Country,
        IsPublic = t.IsPublic,
        IsDefault = t.IsDefault,
        UsageCount = t.UsageCount,
        Rating = t.Rating,
        RatingCount = t.RatingCount,
        Tags = t.Tags,
        PreviewImage = t.PreviewImage,
        Author = t.Author,
        Version = t.Version,
        Status = t.Status.ToString(),
        CreatedAt = t.CreatedAt,
        UpdatedAt = t.UpdatedAt,
        LastUsed = t.LastUsed
    };
}

public class CloneTemplateRequest
{
    public string? Name { get; set; }
}

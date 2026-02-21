using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services.Cms;
using Sqordia.Contracts.Requests.Cms;
using Sqordia.Contracts.Responses.Cms;

namespace WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/cms/templates")]
[Authorize(Roles = "Admin")]
public class CmsTemplateController : BaseApiController
{
    private readonly ICmsTemplateService _templateService;
    private readonly ILogger<CmsTemplateController> _logger;

    public CmsTemplateController(
        ICmsTemplateService templateService,
        ILogger<CmsTemplateController> logger)
    {
        _templateService = templateService;
        _logger = logger;
    }

    /// <summary>
    /// Get all accessible templates, optionally filtered by page and section
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<CmsContentTemplateSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTemplates(
        [FromQuery] string? pageKey = null,
        [FromQuery] string? sectionKey = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _templateService.GetTemplatesAsync(pageKey, sectionKey, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get a specific template by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CmsContentTemplateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTemplate(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _templateService.GetTemplateAsync(id, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a new template
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CmsContentTemplateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateTemplate(
        [FromBody] CreateCmsTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _templateService.CreateTemplateAsync(request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Update an existing template
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CmsContentTemplateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateTemplate(
        Guid id,
        [FromBody] UpdateCmsTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _templateService.UpdateTemplateAsync(id, request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Delete a template
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteTemplate(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _templateService.DeleteTemplateAsync(id, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a template from an existing section's blocks
    /// </summary>
    [HttpPost("from-section")]
    [ProducesResponseType(typeof(CmsContentTemplateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateTemplateFromSection(
        [FromBody] CreateCmsTemplateFromSectionRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _templateService.CreateTemplateFromSectionAsync(
            request.VersionId,
            request.SectionKey,
            request.Language,
            request.Name,
            request.Description,
            request.IsPublic,
            cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Apply a template to a version's section
    /// </summary>
    [HttpPost("{id:guid}/apply")]
    [ProducesResponseType(typeof(List<CmsContentBlockResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ApplyTemplate(
        Guid id,
        [FromBody] ApplyCmsTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _templateService.ApplyTemplateAsync(
            request.VersionId,
            id,
            request.SectionKey,
            request.Language,
            request.ReplaceExisting,
            cancellationToken);
        return HandleResult(result);
    }
}

/// <summary>
/// Request to create a CMS template from an existing section
/// </summary>
public class CreateCmsTemplateFromSectionRequest
{
    public required Guid VersionId { get; set; }
    public required string SectionKey { get; set; }
    public required string Language { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsPublic { get; set; }
}

/// <summary>
/// Request to apply a CMS template to a section
/// </summary>
public class ApplyCmsTemplateRequest
{
    public required Guid VersionId { get; set; }
    public required string SectionKey { get; set; }
    public required string Language { get; set; }
    public bool ReplaceExisting { get; set; }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Templates.Commands;
using Sqordia.Application.Templates.Queries;
using Sqordia.Application.Templates.Services;
using Sqordia.Domain.Enums;

namespace WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/template")]
public class TemplateController : BaseApiController
{
    private readonly ITemplateService _templateService;

    public TemplateController(ITemplateService templateService)
    {
        _templateService = templateService;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateTemplate([FromBody] CreateTemplateCommand command, CancellationToken cancellationToken = default)
    {
        var result = await _templateService.CreateTemplateAsync(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Get all templates (admin only - returns all templates including drafts)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllTemplates(CancellationToken cancellationToken = default)
    {
        // For admin, use search with empty string to get all templates
        var result = await _templateService.SearchTemplatesAsync("");
        return HandleResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTemplate(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _templateService.GetTemplateByIdAsync(id);
        return HandleResult(result);
    }

    [HttpGet("category/{category}")]
    public async Task<IActionResult> GetTemplatesByCategory(TemplateCategory category, CancellationToken cancellationToken = default)
    {
        var result = await _templateService.GetTemplatesByCategoryAsync(category);
        return HandleResult(result);
    }

    [HttpGet("industry/{industry}")]
    public async Task<IActionResult> GetTemplatesByIndustry(string industry, CancellationToken cancellationToken = default)
    {
        var result = await _templateService.GetTemplatesByIndustryAsync(industry);
        return HandleResult(result);
    }

    [HttpGet("public")]
    public async Task<IActionResult> GetPublicTemplates(CancellationToken cancellationToken = default)
    {
        var result = await _templateService.GetPublicTemplatesAsync();
        return HandleResult(result);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchTemplates([FromQuery] string searchTerm, CancellationToken cancellationToken = default)
    {
        var result = await _templateService.SearchTemplatesAsync(searchTerm);
        return HandleResult(result);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateTemplate(Guid id, UpdateTemplateCommand command, CancellationToken cancellationToken = default)
    {
        var result = await _templateService.UpdateTemplateAsync(command);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteTemplate(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _templateService.DeleteTemplateAsync(id);
        return HandleResult(result);
    }

    [HttpPost("{id}/clone")]
    [Authorize]
    public async Task<IActionResult> CloneTemplate(Guid id, [FromBody] CloneTemplateRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _templateService.CloneTemplateAsync(id, request.Name);
        return HandleResult(result);
    }

    [HttpPost("{id}/publish")]
    [Authorize]
    public async Task<IActionResult> PublishTemplate(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _templateService.PublishTemplateAsync(id);
        return HandleResult(result);
    }

    [HttpPost("{id}/archive")]
    [Authorize]
    public async Task<IActionResult> ArchiveTemplate(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _templateService.ArchiveTemplateAsync(id);
        return HandleResult(result);
    }

    [HttpPost("{id}/usage")]
    public async Task<IActionResult> RecordTemplateUsage(Guid id, [FromBody] RecordUsageRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _templateService.RecordTemplateUsageAsync(id, request.UsageType);
        return HandleResult(result);
    }

    [HttpPost("{id}/rate")]
    [Authorize]
    public async Task<IActionResult> RateTemplate(Guid id, [FromBody] RateTemplateRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _templateService.RateTemplateAsync(id, request.Rating, request.Comment);
        return HandleResult(result);
    }

    [HttpGet("popular")]
    public async Task<IActionResult> GetPopularTemplates([FromQuery] int count = 10, CancellationToken cancellationToken = default)
    {
        var result = await _templateService.GetPopularTemplatesAsync(count);
        return HandleResult(result);
    }

    [HttpGet("recent")]
    public async Task<IActionResult> GetRecentTemplates([FromQuery] int count = 10, CancellationToken cancellationToken = default)
    {
        var result = await _templateService.GetRecentTemplatesAsync(count);
        return HandleResult(result);
    }

    [HttpGet("author/{author}")]
    public async Task<IActionResult> GetTemplatesByAuthor(string author, CancellationToken cancellationToken = default)
    {
        var result = await _templateService.GetTemplatesByAuthorAsync(author);
        return HandleResult(result);
    }

    [HttpGet("{id}/analytics")]
    public async Task<IActionResult> GetTemplateAnalytics(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _templateService.GetTemplateAnalyticsAsync(id);
        return HandleResult(result);
    }

    /// <summary>
    /// Get recommended templates based on persona and industry
    /// </summary>
    /// <param name="persona">User persona (Entrepreneur, Consultant, OBNL)</param>
    /// <param name="industry">Industry sector (optional)</param>
    /// <returns>List of recommended templates</returns>
    [HttpGet("recommended")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetRecommendedTemplates(
        [FromQuery] string? persona = null,
        [FromQuery] string? industry = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _templateService.GetRecommendedTemplatesAsync(persona, industry);
        return HandleResult(result);
    }
}

public record CloneTemplateRequest(string Name);
public record RecordUsageRequest(string UsageType);
public record RateTemplateRequest(int Rating, string Comment);

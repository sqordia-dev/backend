using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Templates.Commands;
using Sqordia.Application.Templates.Queries;
using Sqordia.Application.Templates.Services;
using Sqordia.Domain.Enums;

namespace WebAPI.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/template")]
public class TemplateController : ControllerBase
{
    private readonly ITemplateService _templateService;

    public TemplateController(ITemplateService templateService)
    {
        _templateService = templateService;
    }

    [HttpPost]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public async Task<ActionResult<Result<TemplateDto>>> CreateTemplate([FromBody] CreateTemplateCommand command)
    {
        var result = await _templateService.CreateTemplateAsync(command);
        return result.IsSuccess ? CreatedAtAction(nameof(GetTemplate), new { id = result.Value!.Id }, result) : BadRequest(result);
    }

    /// <summary>
    /// Get all templates (admin only - returns all templates including drafts)
    /// </summary>
    [HttpGet]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public async Task<ActionResult<Result<List<TemplateDto>>>> GetAllTemplates()
    {
        // For admin, use search with empty string to get all templates
        var result = await _templateService.SearchTemplatesAsync("");
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Result<TemplateDto>>> GetTemplate(Guid id)
    {
        var result = await _templateService.GetTemplateByIdAsync(id);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }

    [HttpGet("category/{category}")]
    public async Task<ActionResult<Result<List<TemplateDto>>>> GetTemplatesByCategory(TemplateCategory category)
    {
        var result = await _templateService.GetTemplatesByCategoryAsync(category);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("industry/{industry}")]
    public async Task<ActionResult<Result<List<TemplateDto>>>> GetTemplatesByIndustry(string industry)
    {
        var result = await _templateService.GetTemplatesByIndustryAsync(industry);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("public")]
    public async Task<ActionResult<Result<List<TemplateDto>>>> GetPublicTemplates()
    {
        var result = await _templateService.GetPublicTemplatesAsync();
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("search")]
    public async Task<ActionResult<Result<List<TemplateDto>>>> SearchTemplates([FromQuery] string searchTerm)
    {
        var result = await _templateService.SearchTemplatesAsync(searchTerm);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Result<TemplateDto>>> UpdateTemplate(Guid id, UpdateTemplateCommand command)
    {
        var result = await _templateService.UpdateTemplateAsync(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Result<bool>>> DeleteTemplate(Guid id)
    {
        var result = await _templateService.DeleteTemplateAsync(id);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/clone")]
    public async Task<ActionResult<Result<TemplateDto>>> CloneTemplate(Guid id, [FromBody] CloneTemplateRequest request)
    {
        var result = await _templateService.CloneTemplateAsync(id, request.Name);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/publish")]
    public async Task<ActionResult<Result<TemplateDto>>> PublishTemplate(Guid id)
    {
        var result = await _templateService.PublishTemplateAsync(id);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/archive")]
    public async Task<ActionResult<Result<TemplateDto>>> ArchiveTemplate(Guid id)
    {
        var result = await _templateService.ArchiveTemplateAsync(id);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/usage")]
    public async Task<ActionResult<Result<TemplateUsageDto>>> RecordTemplateUsage(Guid id, [FromBody] RecordUsageRequest request)
    {
        var result = await _templateService.RecordTemplateUsageAsync(id, request.UsageType);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/rate")]
    public async Task<ActionResult<Result<TemplateRatingDto>>> RateTemplate(Guid id, [FromBody] RateTemplateRequest request)
    {
        var result = await _templateService.RateTemplateAsync(id, request.Rating, request.Comment);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("popular")]
    public async Task<ActionResult<Result<List<TemplateDto>>>> GetPopularTemplates([FromQuery] int count = 10)
    {
        var result = await _templateService.GetPopularTemplatesAsync(count);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("recent")]
    public async Task<ActionResult<Result<List<TemplateDto>>>> GetRecentTemplates([FromQuery] int count = 10)
    {
        var result = await _templateService.GetRecentTemplatesAsync(count);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("author/{author}")]
    public async Task<ActionResult<Result<List<TemplateDto>>>> GetTemplatesByAuthor(string author)
    {
        var result = await _templateService.GetTemplatesByAuthorAsync(author);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id}/analytics")]
    public async Task<ActionResult<Result<TemplateAnalyticsDto>>> GetTemplateAnalytics(Guid id)
    {
        var result = await _templateService.GetTemplateAnalyticsAsync(id);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}

public record CloneTemplateRequest(string Name);
public record RecordUsageRequest(string UsageType);
public record RateTemplateRequest(int Rating, string Comment);

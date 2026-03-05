using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services;

namespace WebAPI.Controllers;

/// <summary>
/// Admin Email Template Management API
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/email-templates")]
[Authorize(Roles = "Admin")]
public class EmailTemplateController : BaseApiController
{
    private readonly IEmailTemplateService _templateService;
    private readonly ILogger<EmailTemplateController> _logger;

    public EmailTemplateController(
        IEmailTemplateService templateService,
        ILogger<EmailTemplateController> logger)
    {
        _templateService = templateService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
    {
        var result = await _templateService.GetAllAsync(cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _templateService.GetByIdAsync(id, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateEmailTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _templateService.CreateAsync(request, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateEmailTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _templateService.UpdateAsync(id, request, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _templateService.DeleteAsync(id, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateWithAi(
        [FromBody] GenerateEmailTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _templateService.GenerateWithAiAsync(request, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("{id:guid}/preview")]
    public async Task<IActionResult> Preview(
        Guid id,
        [FromBody] EmailTemplatePreviewRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _templateService.RenderAsync(id, request.Language, request.Variables, cancellationToken);
        return HandleResult(result);
    }
}

public class EmailTemplatePreviewRequest
{
    public string Language { get; set; } = "en";
    public Dictionary<string, string> Variables { get; set; } = new();
}

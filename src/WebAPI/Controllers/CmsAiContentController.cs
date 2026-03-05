using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services.Cms;
using WebAPI.Helpers;

namespace WebAPI.Controllers;

/// <summary>
/// CMS AI Content Generation API
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/cms/ai")]
[Authorize(Roles = "Admin")]
public class CmsAiContentController : BaseApiController
{
    private readonly ICmsAiContentService _contentService;
    private readonly ILogger<CmsAiContentController> _logger;

    public CmsAiContentController(
        ICmsAiContentService contentService,
        ILogger<CmsAiContentController> logger)
    {
        _contentService = contentService;
        _logger = logger;
    }

    /// <summary>
    /// Generate content (non-streaming)
    /// </summary>
    [HttpPost("generate")]
    public async Task<IActionResult> Generate(
        [FromBody] GenerateCmsContentRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _contentService.GenerateContentAsync(request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Stream content generation via SSE
    /// </summary>
    [HttpPost("stream")]
    public async Task Stream(
        [FromBody] GenerateCmsContentRequest request,
        CancellationToken cancellationToken = default)
    {
        SseHelper.ConfigureForSse(Response);

        try
        {
            await foreach (var chunk in _contentService.StreamContentAsync(request, cancellationToken))
            {
                await SseHelper.WriteJsonEventAsync(Response,
                    new { type = "token", content = chunk },
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error streaming CMS AI content");
            await SseHelper.WriteJsonEventAsync(Response,
                new { type = "error", error = "Content generation failed" },
                cancellationToken);
        }

        await SseHelper.WriteDoneAsync(Response, cancellationToken);
    }
}

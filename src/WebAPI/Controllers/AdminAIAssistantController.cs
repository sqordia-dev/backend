using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services;
using WebAPI.Helpers;

namespace WebAPI.Controllers;

/// <summary>
/// Admin AI Assistant SSE endpoint
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/ai-assistant")]
[Authorize(Roles = "Admin")]
public class AdminAIAssistantController : BaseApiController
{
    private readonly IAdminAIAssistantService _assistantService;
    private readonly ILogger<AdminAIAssistantController> _logger;

    public AdminAIAssistantController(
        IAdminAIAssistantService assistantService,
        ILogger<AdminAIAssistantController> logger)
    {
        _assistantService = assistantService;
        _logger = logger;
    }

    /// <summary>
    /// Stream admin AI assistant response via SSE
    /// </summary>
    [HttpPost("stream")]
    public async Task StreamAdminQuery(
        [FromBody] AdminAIAssistantRequest request,
        CancellationToken cancellationToken = default)
    {
        SseHelper.ConfigureForSse(Response);

        try
        {
            await foreach (var evt in _assistantService.StreamAdminQueryAsync(
                request.Messages, cancellationToken))
            {
                await SseHelper.WriteJsonEventAsync(Response, evt, cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Admin AI assistant stream cancelled by client");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in admin AI assistant stream");
            var errorMsg = ex.InnerException?.Message ?? ex.Message;
            await SseHelper.WriteJsonEventAsync(Response,
                new AdminAIStreamEvent { Type = "error", Error = $"An error occurred: {errorMsg}" },
                cancellationToken);
        }

        await SseHelper.WriteDoneAsync(Response, cancellationToken);
    }
}

public class AdminAIAssistantRequest
{
    public List<AdminAIMessage> Messages { get; set; } = new();
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services;
using Sqordia.Contracts.Requests.Questionnaire;
using WebAPI.Controllers;

namespace Sqordia.WebAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/business-plans/{businessPlanId}/adaptive-interview")]
public class AdaptiveInterviewController : BaseApiController
{
    private readonly IAdaptiveInterviewService _adaptiveInterviewService;

    public AdaptiveInterviewController(IAdaptiveInterviewService adaptiveInterviewService)
    {
        _adaptiveInterviewService = adaptiveInterviewService;
    }

    [HttpGet("questions")]
    public async Task<IActionResult> GetAdaptiveQuestions(
        Guid businessPlanId,
        [FromQuery] string language = "fr",
        CancellationToken cancellationToken = default)
    {
        var result = await _adaptiveInterviewService.GetAdaptiveQuestionsAsync(
            businessPlanId, language, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("responses")]
    public async Task<IActionResult> SubmitAdaptiveResponse(
        Guid businessPlanId,
        [FromBody] SubmitAdaptiveResponseRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _adaptiveInterviewService.SubmitAdaptiveResponseAsync(
            businessPlanId, request, cancellationToken);
        return HandleResult(result);
    }
}

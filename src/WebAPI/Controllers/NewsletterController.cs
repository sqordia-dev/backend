using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services;
using Sqordia.Contracts.Requests.Newsletter;

namespace WebAPI.Controllers;

public class NewsletterController : BaseApiController
{
    private readonly INewsletterSubscriberService _newsletterService;

    public NewsletterController(INewsletterSubscriberService newsletterService)
    {
        _newsletterService = newsletterService;
    }

    [HttpPost("subscribe")]
    [AllowAnonymous]
    public async Task<IActionResult> Subscribe(
        [FromBody] SubscribeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _newsletterService.SubscribeAsync(request, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("unsubscribe")]
    [AllowAnonymous]
    public async Task<IActionResult> Unsubscribe(
        [FromBody] UnsubscribeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _newsletterService.UnsubscribeAsync(request, cancellationToken);
        return HandleResult(result);
    }
}

using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.Newsletter;
using Sqordia.Contracts.Responses.Newsletter;

namespace Sqordia.Application.Services;

public interface INewsletterSubscriberService
{
    Task<Result<NewsletterSubscriberResponse>> SubscribeAsync(SubscribeRequest request, CancellationToken cancellationToken = default);
    Task<Result> UnsubscribeAsync(UnsubscribeRequest request, CancellationToken cancellationToken = default);
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.Newsletter;
using Sqordia.Contracts.Responses.Newsletter;
using Sqordia.Domain.Entities;

namespace Sqordia.Application.Services.Implementations;

public class NewsletterSubscriberService : INewsletterSubscriberService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<NewsletterSubscriberService> _logger;

    public NewsletterSubscriberService(
        IApplicationDbContext context,
        ILogger<NewsletterSubscriberService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<NewsletterSubscriberResponse>> SubscribeAsync(
        SubscribeRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();

            var existing = await _context.NewsletterSubscribers
                .FirstOrDefaultAsync(n => n.Email == normalizedEmail, cancellationToken);

            if (existing != null)
            {
                if (existing.IsActive)
                {
                    return Result.Failure<NewsletterSubscriberResponse>(
                        Error.Conflict("Newsletter.AlreadySubscribed", "This email is already subscribed"));
                }

                existing.Resubscribe();
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Newsletter resubscription: {Email}", normalizedEmail);
                return Result.Success(MapToResponse(existing));
            }

            var subscriber = new NewsletterSubscriber(normalizedEmail, request.Language);
            _context.NewsletterSubscribers.Add(subscriber);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("New newsletter subscription: {Email}", normalizedEmail);
            return Result.Success(MapToResponse(subscriber));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing {Email} to newsletter", request.Email);
            return Result.Failure<NewsletterSubscriberResponse>(
                Error.Failure("Newsletter.Error.SubscribeFailed", "Failed to subscribe to newsletter"));
        }
    }

    public async Task<Result> UnsubscribeAsync(
        UnsubscribeRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();

            var subscriber = await _context.NewsletterSubscribers
                .FirstOrDefaultAsync(n => n.Email == normalizedEmail, cancellationToken);

            if (subscriber == null)
            {
                return Result.Failure(
                    Error.NotFound("Newsletter.NotFound", "Email not found in subscribers"));
            }

            if (!subscriber.IsActive)
            {
                return Result.Success();
            }

            subscriber.Unsubscribe();
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Newsletter unsubscription: {Email}", normalizedEmail);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsubscribing {Email} from newsletter", request.Email);
            return Result.Failure(
                Error.Failure("Newsletter.Error.UnsubscribeFailed", "Failed to unsubscribe from newsletter"));
        }
    }

    private static NewsletterSubscriberResponse MapToResponse(NewsletterSubscriber subscriber)
    {
        return new NewsletterSubscriberResponse
        {
            Id = subscriber.Id,
            Email = subscriber.Email,
            IsActive = subscriber.IsActive,
            Language = subscriber.Language,
            SubscribedAt = subscriber.SubscribedAt,
            UnsubscribedAt = subscriber.UnsubscribedAt
        };
    }
}

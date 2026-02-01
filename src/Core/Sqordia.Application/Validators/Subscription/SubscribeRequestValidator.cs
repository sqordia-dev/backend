using FluentValidation;
using Sqordia.Application.Contracts.Requests;

namespace Sqordia.Application.Validators.Subscription;

public class SubscribeRequestValidator : AbstractValidator<SubscribeRequest>
{
    public SubscribeRequestValidator()
    {
        RuleFor(x => x.PlanId)
            .NotEmpty();

        RuleFor(x => x.OrganizationId)
            .NotEmpty();
    }
}

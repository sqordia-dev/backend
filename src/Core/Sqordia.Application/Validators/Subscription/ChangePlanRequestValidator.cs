using FluentValidation;
using Sqordia.Application.Contracts.Requests;

namespace Sqordia.Application.Validators.Subscription;

public class ChangePlanRequestValidator : AbstractValidator<ChangePlanRequest>
{
    public ChangePlanRequestValidator()
    {
        RuleFor(x => x.NewPlanId)
            .NotEmpty();
    }
}

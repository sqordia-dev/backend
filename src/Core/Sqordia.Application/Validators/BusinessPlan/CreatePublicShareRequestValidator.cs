using FluentValidation;
using Sqordia.Contracts.Requests.BusinessPlan;

namespace Sqordia.Application.Validators.BusinessPlan;

public class CreatePublicShareRequestValidator : AbstractValidator<CreatePublicShareRequest>
{
    public CreatePublicShareRequestValidator()
    {
        RuleFor(x => x.Permission)
            .IsInEnum().WithMessage("Permission must be a valid share permission value");

        RuleFor(x => x.ExpiresAt)
            .GreaterThan(DateTime.UtcNow).WithMessage("Expiration date must be in the future")
            .When(x => x.ExpiresAt.HasValue);
    }
}

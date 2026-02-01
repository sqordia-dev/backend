using FluentValidation;
using Sqordia.Contracts.Requests.BusinessPlan;

namespace Sqordia.Application.Validators.BusinessPlan;

public class ShareBusinessPlanRequestValidator : AbstractValidator<ShareBusinessPlanRequest>
{
    public ShareBusinessPlanRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required when no user ID is provided")
            .EmailAddress().WithMessage("A valid email address is required")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters")
            .When(x => x.SharedWithUserId is null);

        RuleFor(x => x)
            .Must(x => x.SharedWithUserId is not null || !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("Either a user ID or an email address must be provided");

        RuleFor(x => x.Permission)
            .IsInEnum().WithMessage("Permission must be a valid share permission value");

        RuleFor(x => x.ExpiresAt)
            .GreaterThan(DateTime.UtcNow).WithMessage("Expiration date must be in the future")
            .When(x => x.ExpiresAt.HasValue);
    }
}

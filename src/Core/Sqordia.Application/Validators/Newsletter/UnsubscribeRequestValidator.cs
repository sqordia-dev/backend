using FluentValidation;
using Sqordia.Contracts.Requests.Newsletter;

namespace Sqordia.Application.Validators.Newsletter;

public class UnsubscribeRequestValidator : AbstractValidator<UnsubscribeRequest>
{
    public UnsubscribeRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Valid email address is required")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters");
    }
}

using FluentValidation;
using Sqordia.Contracts.Requests.Auth;

namespace Sqordia.Application.Validators.Authentication;

public class SendEmailVerificationRequestValidator : AbstractValidator<SendEmailVerificationRequest>
{
    public SendEmailVerificationRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Valid email address is required");
    }
}

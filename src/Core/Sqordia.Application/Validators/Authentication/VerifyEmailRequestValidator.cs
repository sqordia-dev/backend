using FluentValidation;
using Sqordia.Contracts.Requests.Auth;

namespace Sqordia.Application.Validators.Authentication;

public class VerifyEmailRequestValidator : AbstractValidator<VerifyEmailRequest>
{
    public VerifyEmailRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token is required");
    }
}

using FluentValidation;
using Sqordia.Contracts.Requests.TwoFactor;

namespace Sqordia.Application.Validators.TwoFactor;

public class EnableTwoFactorRequestValidator : AbstractValidator<EnableTwoFactorRequest>
{
    public EnableTwoFactorRequestValidator()
    {
        RuleFor(x => x.VerificationCode)
            .NotEmpty().WithMessage("Verification code is required")
            .Length(6).WithMessage("Verification code must be exactly 6 digits")
            .Matches(@"^\d{6}$").WithMessage("Verification code must contain only digits");
    }
}

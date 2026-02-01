using FluentValidation;
using Sqordia.Contracts.Requests.Auth;

namespace Sqordia.Application.Validators.Authentication;

public class RevokeTokenRequestValidator : AbstractValidator<RevokeTokenRequest>
{
    public RevokeTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Token is required");
    }
}

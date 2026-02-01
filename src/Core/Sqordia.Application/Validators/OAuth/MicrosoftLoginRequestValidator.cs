using FluentValidation;
using Sqordia.Contracts.Requests.Auth;

namespace Sqordia.Application.Validators.OAuth;

public class MicrosoftLoginRequestValidator : AbstractValidator<MicrosoftLoginRequest>
{
    public MicrosoftLoginRequestValidator()
    {
        RuleFor(x => x.AccessToken)
            .NotEmpty().WithMessage("Microsoft access token is required")
            .MinimumLength(50).WithMessage("Microsoft access token appears to be invalid");
    }
}

using FluentValidation;
using Sqordia.Contracts.Requests.Auth;

namespace Sqordia.Application.Validators.Authentication;

public class LogoutRequestValidator : AbstractValidator<LogoutRequest>
{
    public LogoutRequestValidator()
    {
        // RefreshToken is optional — controller falls back to cookie if not provided
    }
}

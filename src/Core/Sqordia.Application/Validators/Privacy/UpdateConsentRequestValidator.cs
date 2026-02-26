using FluentValidation;
using Sqordia.Contracts.Requests.Privacy;

namespace Sqordia.Application.Validators.Privacy;

public class UpdateConsentRequestValidator : AbstractValidator<UpdateConsentRequest>
{
    public UpdateConsentRequestValidator()
    {
        RuleFor(x => x.ConsentType)
            .NotEmpty().WithMessage("Consent type is required")
            .Must(x => x == "TermsOfService" || x == "PrivacyPolicy")
            .WithMessage("Consent type must be 'TermsOfService' or 'PrivacyPolicy'");

        RuleFor(x => x.Version)
            .NotEmpty().WithMessage("Version is required")
            .MaximumLength(20).WithMessage("Version cannot exceed 20 characters");
    }
}

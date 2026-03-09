using FluentValidation;
using Sqordia.Contracts.Requests.Onboarding;

namespace Sqordia.Application.Validators.Onboarding;

public class OnboardingProfileCompleteRequestValidator : AbstractValidator<OnboardingProfileCompleteRequest>
{
    private static readonly string[] ValidPersonas = { "entrepreneur", "consultant", "obnl" };

    public OnboardingProfileCompleteRequestValidator()
    {
        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage("Company name is required")
            .MinimumLength(2).WithMessage("Company name must be at least 2 characters")
            .MaximumLength(200).WithMessage("Company name must not exceed 200 characters");

        RuleFor(x => x.Persona)
            .NotEmpty().WithMessage("Persona is required")
            .Must(p => ValidPersonas.Contains(p.ToLowerInvariant()))
            .WithMessage("Persona must be one of: entrepreneur, consultant, obnl");

        RuleFor(x => x.Industry)
            .MaximumLength(100).When(x => x.Industry != null);

        RuleFor(x => x.Sector)
            .MaximumLength(100).When(x => x.Sector != null);

        RuleFor(x => x.BusinessStage)
            .MaximumLength(50).When(x => x.BusinessStage != null);

        RuleFor(x => x.TeamSize)
            .MaximumLength(50).When(x => x.TeamSize != null);

        RuleFor(x => x.FundingStatus)
            .MaximumLength(50).When(x => x.FundingStatus != null);

        RuleFor(x => x.TargetMarket)
            .MaximumLength(100).When(x => x.TargetMarket != null);

        RuleFor(x => x.City)
            .MaximumLength(100).When(x => x.City != null);

        RuleFor(x => x.Province)
            .MaximumLength(100).When(x => x.Province != null);

        RuleFor(x => x.Country)
            .MaximumLength(100).When(x => x.Country != null);
    }
}

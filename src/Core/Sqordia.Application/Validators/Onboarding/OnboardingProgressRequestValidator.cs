using FluentValidation;
using Sqordia.Contracts.Requests.Onboarding;

namespace Sqordia.Application.Validators.Onboarding;

public class OnboardingProgressRequestValidator : AbstractValidator<OnboardingProgressRequest>
{
    public OnboardingProgressRequestValidator()
    {
        RuleFor(x => x.Step)
            .GreaterThanOrEqualTo(0).WithMessage("Step must be 0 or greater")
            .LessThanOrEqualTo(10).WithMessage("Step must be 10 or less");

        RuleFor(x => x.Persona)
            .Must(BeValidPersona).When(x => !string.IsNullOrEmpty(x.Persona))
            .WithMessage("Persona must be 'Entrepreneur', 'Consultant', or 'OBNL'");

        RuleFor(x => x.StepData)
            .MaximumLength(50000).When(x => !string.IsNullOrEmpty(x.StepData))
            .WithMessage("Step data must not exceed 50000 characters");
    }

    private static bool BeValidPersona(string? persona)
    {
        if (string.IsNullOrEmpty(persona)) return true;
        var validPersonas = new[] { "Entrepreneur", "Consultant", "OBNL" };
        return validPersonas.Contains(persona, StringComparer.OrdinalIgnoreCase);
    }
}

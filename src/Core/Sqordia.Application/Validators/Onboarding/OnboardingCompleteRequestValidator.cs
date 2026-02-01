using FluentValidation;
using Sqordia.Contracts.Requests.Onboarding;

namespace Sqordia.Application.Validators.Onboarding;

public class OnboardingCompleteRequestValidator : AbstractValidator<OnboardingCompleteRequest>
{
    public OnboardingCompleteRequestValidator()
    {
        RuleFor(x => x.PlanName)
            .NotEmpty().When(x => x.CreateInitialPlan)
            .WithMessage("Plan name is required when creating an initial plan")
            .MaximumLength(200).When(x => !string.IsNullOrEmpty(x.PlanName))
            .WithMessage("Plan name must not exceed 200 characters");

        RuleFor(x => x.Industry)
            .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Industry))
            .WithMessage("Industry must not exceed 100 characters");

        RuleFor(x => x.FinalData)
            .MaximumLength(100000).When(x => !string.IsNullOrEmpty(x.FinalData))
            .WithMessage("Final data must not exceed 100000 characters");
    }
}

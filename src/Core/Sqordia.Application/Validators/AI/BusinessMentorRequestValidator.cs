using FluentValidation;
using Sqordia.Contracts.Requests.AI;

namespace Sqordia.Application.Validators.AI;

public class BusinessMentorRequestValidator : AbstractValidator<BusinessMentorRequest>
{
    private static readonly string[] ValidLanguages = { "fr", "en" };
    private static readonly string[] ValidPlanTypes = { "BusinessPlan", "StrategicPlan", "LeanCanvas" };

    public BusinessMentorRequestValidator()
    {
        RuleFor(x => x.BusinessPlanId)
            .NotEmpty().WithMessage("Business plan ID is required");

        RuleFor(x => x.PlanType)
            .NotEmpty().WithMessage("Plan type is required")
            .Must(pt => ValidPlanTypes.Contains(pt))
            .WithMessage("Plan type must be one of: BusinessPlan, StrategicPlan, LeanCanvas");

        RuleFor(x => x.Industry)
            .MaximumLength(200).WithMessage("Industry must not exceed 200 characters")
            .When(x => x.Industry is not null);

        RuleFor(x => x.Language)
            .Must(BeValidLanguage)
            .WithMessage("Language must be 'fr' or 'en'");

        RuleForEach(x => x.AnalysisAreas)
            .NotEmpty().WithMessage("Analysis area must not be empty")
            .MaximumLength(100).WithMessage("Analysis area must not exceed 100 characters")
            .When(x => x.AnalysisAreas is not null);
    }

    private static bool BeValidLanguage(string language)
    {
        return ValidLanguages.Contains(language.ToLowerInvariant());
    }
}

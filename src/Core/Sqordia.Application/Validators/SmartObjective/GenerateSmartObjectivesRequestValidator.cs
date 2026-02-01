using FluentValidation;
using Sqordia.Contracts.Requests.SmartObjective;

namespace Sqordia.Application.Validators.SmartObjective;

public class GenerateSmartObjectivesRequestValidator : AbstractValidator<GenerateSmartObjectivesRequest>
{
    private static readonly string[] ValidLanguages = { "fr", "en" };

    public GenerateSmartObjectivesRequestValidator()
    {
        RuleFor(x => x.BusinessPlanId)
            .NotEmpty().WithMessage("Business plan ID is required");

        RuleFor(x => x.ObjectiveCount)
            .InclusiveBetween(1, 20).WithMessage("Objective count must be between 1 and 20");

        RuleForEach(x => x.Categories)
            .NotEmpty().WithMessage("Category must not be empty")
            .MaximumLength(100).WithMessage("Category must not exceed 100 characters")
            .When(x => x.Categories is not null);

        RuleFor(x => x.TimeHorizonMonths)
            .InclusiveBetween(1, 60).WithMessage("Time horizon must be between 1 and 60 months");

        RuleFor(x => x.Language)
            .Must(BeValidLanguage)
            .WithMessage("Language must be 'fr' or 'en'");
    }

    private static bool BeValidLanguage(string language)
    {
        return ValidLanguages.Contains(language.ToLowerInvariant());
    }
}

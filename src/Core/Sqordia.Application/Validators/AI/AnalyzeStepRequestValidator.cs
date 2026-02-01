using FluentValidation;
using Sqordia.Contracts.Requests.AI;

namespace Sqordia.Application.Validators.AI;

public class AnalyzeStepRequestValidator : AbstractValidator<AnalyzeStepRequest>
{
    private static readonly string[] ValidLanguages = { "fr", "en" };
    private static readonly string[] ValidPersonas = { "Entrepreneur", "Consultant", "OBNL" };

    public AnalyzeStepRequestValidator()
    {
        RuleFor(x => x.StepNumber)
            .InclusiveBetween(1, 10).WithMessage("Step number must be between 1 and 10");

        RuleFor(x => x.Answers)
            .NotEmpty().WithMessage("Answers are required");

        RuleForEach(x => x.Answers)
            .SetValidator(new StepAnswerValidator());

        RuleFor(x => x.Persona)
            .Must(p => ValidPersonas.Contains(p))
            .WithMessage("Persona must be one of: Entrepreneur, Consultant, OBNL");

        RuleFor(x => x.Language)
            .Must(BeValidLanguage)
            .WithMessage("Language must be 'fr' or 'en'");
    }

    private static bool BeValidLanguage(string language)
    {
        return ValidLanguages.Contains(language.ToLowerInvariant());
    }
}

public class StepAnswerValidator : AbstractValidator<StepAnswer>
{
    public StepAnswerValidator()
    {
        RuleFor(x => x.QuestionId)
            .NotEmpty().WithMessage("Question ID is required");

        RuleFor(x => x.Answer)
            .NotEmpty().WithMessage("Answer is required")
            .MaximumLength(50000).WithMessage("Answer must not exceed 50000 characters");
    }
}

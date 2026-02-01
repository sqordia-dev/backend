using FluentValidation;
using Sqordia.Contracts.Requests.AI;

namespace Sqordia.Application.Validators.AI;

public class PolishAnswerRequestValidator : AbstractValidator<PolishAnswerRequest>
{
    private static readonly string[] ValidLanguages = { "fr", "en" };
    private static readonly string[] ValidPersonas = { "Entrepreneur", "Consultant", "OBNL" };

    public PolishAnswerRequestValidator()
    {
        RuleFor(x => x.QuestionId)
            .NotEmpty().WithMessage("Question ID is required");

        RuleFor(x => x.Answer)
            .NotEmpty().WithMessage("Answer is required")
            .MinimumLength(10).WithMessage("Answer must be at least 10 characters")
            .MaximumLength(50000).WithMessage("Answer must not exceed 50000 characters");

        RuleFor(x => x.Context)
            .MaximumLength(2000).WithMessage("Context must not exceed 2000 characters")
            .When(x => x.Context is not null);

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

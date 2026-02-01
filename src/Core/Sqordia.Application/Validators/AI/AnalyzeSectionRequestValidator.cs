using FluentValidation;
using Sqordia.Contracts.Requests.AI;

namespace Sqordia.Application.Validators.AI;

public class AnalyzeSectionRequestValidator : AbstractValidator<AnalyzeSectionRequest>
{
    private static readonly string[] ValidLanguages = { "fr", "en" };
    private static readonly string[] ValidPersonas = { "Entrepreneur", "Consultant", "OBNL" };

    public AnalyzeSectionRequestValidator()
    {
        RuleFor(x => x.SectionName)
            .NotEmpty().WithMessage("Section name is required")
            .MaximumLength(200).WithMessage("Section name must not exceed 200 characters");

        RuleFor(x => x.Persona)
            .Must(p => ValidPersonas.Contains(p))
            .WithMessage("Persona must be one of: Entrepreneur, Consultant, OBNL")
            .When(x => x.Persona is not null);

        RuleFor(x => x.Language)
            .Must(BeValidLanguage)
            .WithMessage("Language must be 'fr' or 'en'");
    }

    private static bool BeValidLanguage(string language)
    {
        return ValidLanguages.Contains(language.ToLowerInvariant());
    }
}

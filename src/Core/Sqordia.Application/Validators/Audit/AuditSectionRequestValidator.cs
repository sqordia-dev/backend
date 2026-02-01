using FluentValidation;
using Sqordia.Contracts.Requests.V2.Audit;

namespace Sqordia.Application.Validators.Audit;

public class AuditSectionRequestValidator : AbstractValidator<AuditSectionRequest>
{
    private static readonly string[] ValidLanguages = { "fr", "en" };

    public AuditSectionRequestValidator()
    {
        RuleFor(x => x.Language)
            .NotEmpty().WithMessage("Language is required")
            .Must(BeValidLanguage).WithMessage("Language must be 'fr' or 'en'");

        RuleFor(x => x.SectionName)
            .MaximumLength(100).WithMessage("Section name must not exceed 100 characters")
            .When(x => x.SectionName is not null);
    }

    private static bool BeValidLanguage(string language)
    {
        return ValidLanguages.Contains(language.ToLowerInvariant());
    }
}

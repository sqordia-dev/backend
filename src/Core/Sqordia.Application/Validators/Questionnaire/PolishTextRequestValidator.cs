using FluentValidation;
using Sqordia.Contracts.Requests.V2.Questionnaire;

namespace Sqordia.Application.Validators.Questionnaire;

public class PolishTextRequestValidator : AbstractValidator<PolishTextRequest>
{
    private static readonly string[] ValidLanguages = { "fr", "en" };
    private static readonly string[] ValidTones = { "professional", "casual", "formal", "persuasive" };

    public PolishTextRequestValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Text is required")
            .MinimumLength(10).WithMessage("Text must be at least 10 characters");

        RuleFor(x => x.Language)
            .NotEmpty().WithMessage("Language is required")
            .Must(BeValidLanguage)
            .WithMessage("Language must be 'fr' or 'en'");

        RuleFor(x => x.Tone)
            .NotEmpty().WithMessage("Tone is required")
            .Must(BeValidTone)
            .WithMessage("Tone must be one of: professional, casual, formal, persuasive");

        RuleFor(x => x.Context)
            .MaximumLength(500).WithMessage("Context must not exceed 500 characters")
            .When(x => x.Context is not null);

        RuleFor(x => x.TargetAudience)
            .MaximumLength(200).WithMessage("Target audience must not exceed 200 characters")
            .When(x => x.TargetAudience is not null);
    }

    private static bool BeValidLanguage(string language)
    {
        return ValidLanguages.Contains(language.ToLowerInvariant());
    }

    private static bool BeValidTone(string tone)
    {
        return ValidTones.Contains(tone.ToLowerInvariant());
    }
}

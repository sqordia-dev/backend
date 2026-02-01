using FluentValidation;
using Sqordia.Contracts.Requests.Sections;

namespace Sqordia.Application.Validators.Sections;

public class AiAssistRequestValidator : AbstractValidator<AiAssistRequest>
{
    private static readonly string[] ValidActions = { "improve", "expand", "shorten" };
    private static readonly string[] ValidLanguages = { "fr", "en" };

    public AiAssistRequestValidator()
    {
        RuleFor(x => x.Action)
            .NotEmpty().WithMessage("Action is required")
            .Must(BeValidAction)
            .WithMessage("Action must be 'improve', 'expand', or 'shorten'");

        RuleFor(x => x.CurrentContent)
            .NotEmpty().WithMessage("Current content is required")
            .MinimumLength(1).WithMessage("Current content must not be empty")
            .MaximumLength(50000).WithMessage("Current content must not exceed 50000 characters");

        RuleFor(x => x.Instructions)
            .MaximumLength(1000).When(x => !string.IsNullOrEmpty(x.Instructions))
            .WithMessage("Instructions must not exceed 1000 characters");

        RuleFor(x => x.Language)
            .Must(BeValidLanguage)
            .WithMessage("Language must be 'fr' or 'en'");
    }

    private static bool BeValidAction(string action)
    {
        return ValidActions.Contains(action.ToLowerInvariant());
    }

    private static bool BeValidLanguage(string language)
    {
        return ValidLanguages.Contains(language.ToLowerInvariant());
    }
}

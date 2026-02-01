using FluentValidation;
using Sqordia.Contracts.Requests.AI;

namespace Sqordia.Application.Validators.AI;

public class AIConfigurationRequestValidator : AbstractValidator<AIConfigurationRequest>
{
    private static readonly string[] ValidProviders = { "OpenAI", "Claude", "Gemini" };

    public AIConfigurationRequestValidator()
    {
        RuleFor(x => x.ActiveProvider)
            .NotEmpty().WithMessage("Active provider is required")
            .Must(BeValidProvider)
            .WithMessage("Active provider must be one of: OpenAI, Claude, Gemini");

        RuleFor(x => x.FallbackProviders)
            .NotNull().WithMessage("Fallback providers list is required");

        RuleFor(x => x.Providers)
            .NotNull().WithMessage("Providers configuration is required")
            .NotEmpty().WithMessage("At least one provider must be configured");

        RuleForEach(x => x.Providers)
            .ChildRules(entry =>
            {
                entry.RuleFor(x => x.Value)
                    .SetValidator(new ProviderSettingsRequestValidator());
            })
            .When(x => x.Providers is not null);
    }

    private static bool BeValidProvider(string provider)
    {
        return ValidProviders.Contains(provider);
    }
}

public class ProviderSettingsRequestValidator : AbstractValidator<ProviderSettingsRequest>
{
    public ProviderSettingsRequestValidator()
    {
        RuleFor(x => x.Model)
            .NotEmpty().WithMessage("Model is required");
    }
}

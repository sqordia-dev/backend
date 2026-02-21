using FluentValidation;
using Sqordia.Contracts.Requests.Admin.PromptRegistry;

namespace Sqordia.Application.Validators.Admin.PromptRegistry;

public class TestDraftPromptRequestValidator : AbstractValidator<TestDraftPromptRequest>
{
    private static readonly string[] ValidProviders = { "OpenAI", "Claude", "Gemini", "openai", "claude", "gemini" };

    public TestDraftPromptRequestValidator()
    {
        RuleFor(x => x.SectionType)
            .IsInEnum().WithMessage("Invalid section type");

        RuleFor(x => x.PlanType)
            .IsInEnum().WithMessage("Invalid plan type");

        RuleFor(x => x.SystemPrompt)
            .NotEmpty().WithMessage("System prompt is required")
            .MinimumLength(50).WithMessage("System prompt must be at least 50 characters")
            .MaximumLength(10000).WithMessage("System prompt must not exceed 10000 characters");

        RuleFor(x => x.UserPromptTemplate)
            .NotEmpty().WithMessage("User prompt template is required")
            .MinimumLength(20).WithMessage("User prompt template must be at least 20 characters")
            .MaximumLength(10000).WithMessage("User prompt template must not exceed 10000 characters");

        RuleFor(x => x.SampleVariables)
            .NotEmpty().WithMessage("Sample variables are required")
            .Must(BeValidJson).WithMessage("Sample variables must be valid JSON");

        RuleFor(x => x.Provider)
            .Must(p => p == null || ValidProviders.Contains(p))
            .WithMessage("Provider must be OpenAI, Claude, or Gemini")
            .When(x => x.Provider is not null);

        RuleFor(x => x.MaxTokens)
            .InclusiveBetween(100, 4000).WithMessage("Max tokens must be between 100 and 4000");

        RuleFor(x => x.Temperature)
            .InclusiveBetween(0.0, 1.0).WithMessage("Temperature must be between 0.0 and 1.0");
    }

    private static bool BeValidJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            System.Text.Json.JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

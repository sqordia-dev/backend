using FluentValidation;
using Sqordia.Contracts.Requests.Admin;

namespace Sqordia.Application.Validators.Admin;

public class TestAIPromptRequestValidator : AbstractValidator<TestAIPromptRequest>
{
    public TestAIPromptRequestValidator()
    {
        RuleFor(x => x.PromptId)
            .NotEmpty().WithMessage("Prompt ID is required");

        RuleFor(x => x.SampleVariables)
            .NotEmpty().WithMessage("Sample variables is required")
            .MaximumLength(2000).WithMessage("Sample variables must not exceed 2000 characters");

        RuleFor(x => x.TestContext)
            .MaximumLength(1000).WithMessage("Test context must not exceed 1000 characters")
            .When(x => x.TestContext is not null);

        RuleFor(x => x.MaxTokens)
            .InclusiveBetween(100, 2000).WithMessage("Max tokens must be between 100 and 2000");

        RuleFor(x => x.Temperature)
            .InclusiveBetween(0.0, 1.0).WithMessage("Temperature must be between 0.0 and 1.0");
    }
}

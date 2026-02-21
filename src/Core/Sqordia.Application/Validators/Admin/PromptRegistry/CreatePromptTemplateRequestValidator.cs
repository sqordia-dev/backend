using FluentValidation;
using Sqordia.Contracts.Requests.Admin.PromptRegistry;

namespace Sqordia.Application.Validators.Admin.PromptRegistry;

public class CreatePromptTemplateRequestValidator : AbstractValidator<CreatePromptTemplateRequest>
{
    public CreatePromptTemplateRequestValidator()
    {
        RuleFor(x => x.SectionType)
            .IsInEnum().WithMessage("Invalid section type");

        RuleFor(x => x.PlanType)
            .IsInEnum().WithMessage("Invalid plan type");

        RuleFor(x => x.IndustryCategory)
            .MaximumLength(50).WithMessage("Industry category must not exceed 50 characters")
            .When(x => x.IndustryCategory is not null);

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MinimumLength(3).WithMessage("Name must be at least 3 characters")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(x => x.Description is not null);

        RuleFor(x => x.SystemPrompt)
            .NotEmpty().WithMessage("System prompt is required")
            .MinimumLength(50).WithMessage("System prompt must be at least 50 characters")
            .MaximumLength(10000).WithMessage("System prompt must not exceed 10000 characters");

        RuleFor(x => x.UserPromptTemplate)
            .NotEmpty().WithMessage("User prompt template is required")
            .MinimumLength(20).WithMessage("User prompt template must be at least 20 characters")
            .MaximumLength(10000).WithMessage("User prompt template must not exceed 10000 characters");

        RuleFor(x => x.OutputFormat)
            .IsInEnum().WithMessage("Invalid output format");
    }
}

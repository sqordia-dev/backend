using FluentValidation;
using Sqordia.Contracts.Requests.Admin.PromptRegistry;

namespace Sqordia.Application.Validators.Admin.PromptRegistry;

public class UpdatePromptTemplateRequestValidator : AbstractValidator<UpdatePromptTemplateRequest>
{
    public UpdatePromptTemplateRequestValidator()
    {
        RuleFor(x => x.Name)
            .MinimumLength(3).WithMessage("Name must be at least 3 characters")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters")
            .When(x => x.Name is not null);

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(x => x.Description is not null);

        RuleFor(x => x.SystemPrompt)
            .MinimumLength(50).WithMessage("System prompt must be at least 50 characters")
            .MaximumLength(10000).WithMessage("System prompt must not exceed 10000 characters")
            .When(x => x.SystemPrompt is not null);

        RuleFor(x => x.UserPromptTemplate)
            .MinimumLength(20).WithMessage("User prompt template must be at least 20 characters")
            .MaximumLength(10000).WithMessage("User prompt template must not exceed 10000 characters")
            .When(x => x.UserPromptTemplate is not null);

        RuleFor(x => x.OutputFormat)
            .IsInEnum().WithMessage("Invalid output format")
            .When(x => x.OutputFormat.HasValue);

        RuleFor(x => x.IndustryCategory)
            .MaximumLength(50).WithMessage("Industry category must not exceed 50 characters")
            .When(x => x.IndustryCategory is not null);
    }
}

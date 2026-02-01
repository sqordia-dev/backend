using FluentValidation;
using Sqordia.Contracts.Requests.Admin;

namespace Sqordia.Application.Validators.Admin;

public class UpdateAIPromptRequestValidator : AbstractValidator<UpdateAIPromptRequest>
{
    public UpdateAIPromptRequestValidator()
    {
        RuleFor(x => x.Name)
            .MinimumLength(3).WithMessage("Name must be at least 3 characters")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters")
            .When(x => x.Name is not null);

        RuleFor(x => x.Description)
            .MinimumLength(10).WithMessage("Description must be at least 10 characters")
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(x => x.Description is not null);

        RuleFor(x => x.SystemPrompt)
            .MinimumLength(50).WithMessage("System prompt must be at least 50 characters")
            .MaximumLength(5000).WithMessage("System prompt must not exceed 5000 characters")
            .When(x => x.SystemPrompt is not null);

        RuleFor(x => x.UserPromptTemplate)
            .MinimumLength(20).WithMessage("User prompt template must be at least 20 characters")
            .MaximumLength(2000).WithMessage("User prompt template must not exceed 2000 characters")
            .When(x => x.UserPromptTemplate is not null);

        RuleFor(x => x.Variables)
            .MaximumLength(2000).WithMessage("Variables must not exceed 2000 characters")
            .When(x => x.Variables is not null);

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters")
            .When(x => x.Notes is not null);

        RuleFor(x => x.SectionName)
            .MaximumLength(100).WithMessage("Section name must not exceed 100 characters")
            .When(x => x.SectionName is not null);
    }
}

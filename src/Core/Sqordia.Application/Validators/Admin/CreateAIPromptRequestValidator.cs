using FluentValidation;
using Sqordia.Contracts.Requests.Admin;

namespace Sqordia.Application.Validators.Admin;

public class CreateAIPromptRequestValidator : AbstractValidator<CreateAIPromptRequest>
{
    public CreateAIPromptRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MinimumLength(3).WithMessage("Name must be at least 3 characters")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MinimumLength(10).WithMessage("Description must be at least 10 characters")
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required")
            .MaximumLength(100).WithMessage("Category must not exceed 100 characters");

        RuleFor(x => x.PlanType)
            .NotEmpty().WithMessage("Plan type is required")
            .MaximumLength(50).WithMessage("Plan type must not exceed 50 characters");

        RuleFor(x => x.Language)
            .NotEmpty().WithMessage("Language is required")
            .Length(2, 2).WithMessage("Language must be exactly 2 characters");

        RuleFor(x => x.SectionName)
            .MaximumLength(100).WithMessage("Section name must not exceed 100 characters")
            .When(x => x.SectionName is not null);

        RuleFor(x => x.SystemPrompt)
            .NotEmpty().WithMessage("System prompt is required")
            .MinimumLength(50).WithMessage("System prompt must be at least 50 characters")
            .MaximumLength(5000).WithMessage("System prompt must not exceed 5000 characters");

        RuleFor(x => x.UserPromptTemplate)
            .NotEmpty().WithMessage("User prompt template is required")
            .MinimumLength(20).WithMessage("User prompt template must be at least 20 characters")
            .MaximumLength(2000).WithMessage("User prompt template must not exceed 2000 characters");

        RuleFor(x => x.Variables)
            .MaximumLength(2000).WithMessage("Variables must not exceed 2000 characters")
            .When(x => x.Variables is not null);

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters")
            .When(x => x.Notes is not null);
    }
}

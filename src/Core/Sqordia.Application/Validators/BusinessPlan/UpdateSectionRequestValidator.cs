using FluentValidation;
using Sqordia.Contracts.Requests.BusinessPlan;

namespace Sqordia.Application.Validators.BusinessPlan;

public class UpdateSectionRequestValidator : AbstractValidator<UpdateSectionRequest>
{
    private static readonly string[] ValidStatuses = { "draft", "review", "final" };

    public UpdateSectionRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required")
            .MaximumLength(50000).WithMessage("Content must not exceed 50000 characters");

        RuleFor(x => x.Status)
            .Must(s => ValidStatuses.Contains(s))
            .WithMessage("Status must be one of: draft, review, final")
            .When(x => x.Status is not null);

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes must not exceed 500 characters")
            .When(x => x.Notes is not null);

        RuleForEach(x => x.Tags)
            .NotEmpty().WithMessage("Tag value cannot be empty")
            .MaximumLength(50).WithMessage("Each tag must not exceed 50 characters");
    }
}

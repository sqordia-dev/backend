using FluentValidation;
using Sqordia.Contracts.Requests.BusinessPlan;

namespace Sqordia.Application.Validators.BusinessPlan;

public class DuplicateBusinessPlanRequestValidator : AbstractValidator<DuplicateBusinessPlanRequest>
{
    public DuplicateBusinessPlanRequestValidator()
    {
        RuleFor(x => x.NewTitle)
            .MinimumLength(3).WithMessage("New title must be at least 3 characters")
            .MaximumLength(200).WithMessage("New title must not exceed 200 characters")
            .When(x => x.NewTitle is not null);
    }
}

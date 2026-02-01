using FluentValidation;
using Sqordia.Contracts.Requests.BusinessPlan;

namespace Sqordia.Application.Validators.BusinessPlan;

public class UpdateBusinessPlanRequestValidator : AbstractValidator<UpdateBusinessPlanRequest>
{
    public UpdateBusinessPlanRequestValidator()
    {
        RuleFor(x => x.Title)
            .MinimumLength(3).WithMessage("Title must be at least 3 characters")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters")
            .When(x => x.Title is not null);

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(x => x.Description is not null);
    }
}

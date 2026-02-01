using FluentValidation;
using Sqordia.Contracts.Requests.BusinessPlan;

namespace Sqordia.Application.Validators.BusinessPlan;

public class CreateBusinessPlanRequestValidator : AbstractValidator<CreateBusinessPlanRequest>
{
    private static readonly string[] ValidPlanTypes = { "BusinessPlan", "StrategicPlan", "LeanCanvas" };
    private static readonly string[] ValidPersonas = { "Entrepreneur", "Consultant", "OBNL" };

    public CreateBusinessPlanRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MinimumLength(3).WithMessage("Title must be at least 3 characters")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(x => x.Description is not null);

        RuleFor(x => x.PlanType)
            .NotEmpty().WithMessage("Plan type is required")
            .Must(pt => ValidPlanTypes.Contains(pt))
            .WithMessage("Plan type must be one of: BusinessPlan, StrategicPlan, LeanCanvas");

        RuleFor(x => x.OrganizationId)
            .NotEmpty().WithMessage("Organization ID is required");

        RuleFor(x => x.Persona)
            .Must(p => ValidPersonas.Contains(p))
            .WithMessage("Persona must be one of: Entrepreneur, Consultant, OBNL")
            .When(x => x.Persona is not null);
    }
}

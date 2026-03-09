using FluentValidation;
using Sqordia.Contracts.Requests.Organization;

namespace Sqordia.Application.Validators.Organization;

public class UpdateOrganizationRequestValidator : AbstractValidator<UpdateOrganizationRequest>
{
    public UpdateOrganizationRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Organization name is required")
            .MinimumLength(2).WithMessage("Organization name must be at least 2 characters")
            .MaximumLength(200).WithMessage("Organization name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(x => x.Description is not null);

        RuleFor(x => x.Website)
            .MaximumLength(500).WithMessage("Website URL must not exceed 500 characters")
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Website must be a valid URL")
            .When(x => !string.IsNullOrEmpty(x.Website));

        RuleFor(x => x.Industry).MaximumLength(100).When(x => x.Industry != null);
        RuleFor(x => x.Sector).MaximumLength(100).When(x => x.Sector != null);
        RuleFor(x => x.TeamSize).MaximumLength(50).When(x => x.TeamSize != null);
        RuleFor(x => x.FundingStatus).MaximumLength(50).When(x => x.FundingStatus != null);
        RuleFor(x => x.TargetMarket).MaximumLength(100).When(x => x.TargetMarket != null);
        RuleFor(x => x.BusinessStage).MaximumLength(50).When(x => x.BusinessStage != null);
        RuleFor(x => x.City).MaximumLength(100).When(x => x.City != null);
        RuleFor(x => x.Province).MaximumLength(100).When(x => x.Province != null);
        RuleFor(x => x.Country).MaximumLength(100).When(x => x.Country != null);
    }
}

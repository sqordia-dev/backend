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
    }
}

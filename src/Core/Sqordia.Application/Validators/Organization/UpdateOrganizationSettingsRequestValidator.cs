using FluentValidation;
using Sqordia.Contracts.Requests.Organization;

namespace Sqordia.Application.Validators.Organization;

public class UpdateOrganizationSettingsRequestValidator : AbstractValidator<UpdateOrganizationSettingsRequest>
{
    public UpdateOrganizationSettingsRequestValidator()
    {
        RuleFor(x => x.MaxMembers)
            .InclusiveBetween(1, 1000).WithMessage("Maximum members must be between 1 and 1000");
    }
}

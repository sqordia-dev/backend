using FluentValidation;
using Sqordia.Contracts.Requests.BusinessPlan;

namespace Sqordia.Application.Validators.BusinessPlan;

public class UpdateSharePermissionRequestValidator : AbstractValidator<UpdateSharePermissionRequest>
{
    public UpdateSharePermissionRequestValidator()
    {
        RuleFor(x => x.Permission)
            .IsInEnum().WithMessage("Permission must be a valid share permission value");
    }
}

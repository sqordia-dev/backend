using FluentValidation;
using Sqordia.Contracts.Requests.Role;

namespace Sqordia.Application.Validators.Role;

public class UpdateRoleRequestValidator : AbstractValidator<UpdateRoleRequest>
{
    public UpdateRoleRequestValidator()
    {
        RuleFor(x => x.Name)
            .MinimumLength(2).WithMessage("Role name must be at least 2 characters")
                .When(x => !string.IsNullOrEmpty(x.Name))
            .MaximumLength(100).WithMessage("Role name must not exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Description)
            .MaximumLength(200).WithMessage("Description must not exceed 200 characters")
                .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.PermissionIds)
            .NotNull().WithMessage("Permissions list must not be null");

        RuleForEach(x => x.PermissionIds)
            .NotEmpty().WithMessage("Permission ID must not be empty");
    }
}

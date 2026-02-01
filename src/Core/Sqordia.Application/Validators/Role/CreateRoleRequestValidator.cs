using FluentValidation;
using Sqordia.Contracts.Requests.Role;

namespace Sqordia.Application.Validators.Role;

public class CreateRoleRequestValidator : AbstractValidator<CreateRoleRequest>
{
    public CreateRoleRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Role name is required")
            .MinimumLength(2).WithMessage("Role name must be at least 2 characters")
            .MaximumLength(100).WithMessage("Role name must not exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(200).WithMessage("Description must not exceed 200 characters");

        RuleFor(x => x.PermissionIds)
            .NotNull().WithMessage("Permissions list must not be null");

        RuleForEach(x => x.PermissionIds)
            .NotEmpty().WithMessage("Permission ID must not be empty");
    }
}

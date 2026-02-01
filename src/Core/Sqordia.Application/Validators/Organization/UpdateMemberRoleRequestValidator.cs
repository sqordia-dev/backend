using FluentValidation;
using Sqordia.Contracts.Requests.Organization;

namespace Sqordia.Application.Validators.Organization;

public class UpdateMemberRoleRequestValidator : AbstractValidator<UpdateMemberRoleRequest>
{
    private static readonly string[] ValidRoles = { "Owner", "Admin", "Member", "Viewer" };

    public UpdateMemberRoleRequestValidator()
    {
        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required")
            .Must(role => ValidRoles.Contains(role))
            .WithMessage("Role must be one of: Owner, Admin, Member, Viewer");
    }
}

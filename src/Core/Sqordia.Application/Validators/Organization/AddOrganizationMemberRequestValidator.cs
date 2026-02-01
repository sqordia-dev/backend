using FluentValidation;
using Sqordia.Contracts.Requests.Organization;

namespace Sqordia.Application.Validators.Organization;

public class AddOrganizationMemberRequestValidator : AbstractValidator<AddOrganizationMemberRequest>
{
    private static readonly string[] ValidRoles = { "Owner", "Admin", "Member", "Viewer" };

    public AddOrganizationMemberRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required")
            .Must(role => ValidRoles.Contains(role))
            .WithMessage("Role must be one of: Owner, Admin, Member, Viewer");
    }
}

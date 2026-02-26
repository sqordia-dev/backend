using FluentValidation;
using Sqordia.Contracts.Requests.Privacy;

namespace Sqordia.Application.Validators.Privacy;

public class AccountDeletionRequestValidator : AbstractValidator<AccountDeletionRequest>
{
    public AccountDeletionRequestValidator()
    {
        RuleFor(x => x.DeletionType)
            .NotEmpty().WithMessage("Deletion type is required")
            .Must(x => x == "Deactivate" || x == "Permanent")
            .WithMessage("Deletion type must be 'Deactivate' or 'Permanent'");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required for account deletion");

        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters");
    }
}

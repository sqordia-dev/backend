using FluentValidation;
using Sqordia.Contracts.Requests.V2.Share;

namespace Sqordia.Application.Validators.Share;

public class CreateVaultShareRequestValidator : AbstractValidator<CreateVaultShareRequest>
{
    public CreateVaultShareRequestValidator()
    {
        RuleFor(x => x.ExpiresAt)
            .GreaterThan(DateTime.UtcNow).WithMessage("Expiration date must be in the future")
            .When(x => x.ExpiresAt.HasValue);

        RuleFor(x => x.WatermarkText)
            .MaximumLength(200).WithMessage("Watermark text must not exceed 200 characters")
            .When(x => x.WatermarkText is not null);

        RuleFor(x => x.Password)
            .MinimumLength(6).WithMessage("Password must be at least 6 characters")
            .When(x => x.Password is not null);

        RuleFor(x => x.MaxViews)
            .GreaterThan(0).WithMessage("Max views must be greater than 0")
            .When(x => x.MaxViews.HasValue);
    }
}

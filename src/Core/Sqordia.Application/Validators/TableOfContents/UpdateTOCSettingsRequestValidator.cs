using FluentValidation;
using Sqordia.Contracts.Requests.TableOfContents;

namespace Sqordia.Application.Validators.TableOfContents;

public class UpdateTOCSettingsRequestValidator : AbstractValidator<UpdateTOCSettingsRequest>
{
    private static readonly string[] ValidStyles = { "classic", "modern", "minimal", "magazine", "corporate" };

    public UpdateTOCSettingsRequestValidator()
    {
        RuleFor(x => x.Style)
            .NotEmpty().WithMessage("Style is required")
            .Must(s => ValidStyles.Contains(s))
            .WithMessage("Style must be one of: classic, modern, minimal, magazine, corporate");
    }
}

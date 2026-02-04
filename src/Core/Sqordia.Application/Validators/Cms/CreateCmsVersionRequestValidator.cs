using FluentValidation;
using Sqordia.Contracts.Requests.Cms;

namespace Sqordia.Application.Validators.Cms;

public class CreateCmsVersionRequestValidator : AbstractValidator<CreateCmsVersionRequest>
{
    public CreateCmsVersionRequestValidator()
    {
        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes must not exceed 500 characters")
            .When(x => x.Notes is not null);
    }
}

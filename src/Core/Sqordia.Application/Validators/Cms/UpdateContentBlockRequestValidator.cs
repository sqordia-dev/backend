using FluentValidation;
using Sqordia.Contracts.Requests.Cms;

namespace Sqordia.Application.Validators.Cms;

public class UpdateContentBlockRequestValidator : AbstractValidator<UpdateContentBlockRequest>
{
    public UpdateContentBlockRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required");
    }
}

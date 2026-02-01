using FluentValidation;
using Sqordia.Contracts.Requests.User;

namespace Sqordia.Application.Validators.User;

public class SetPersonaRequestValidator : AbstractValidator<SetPersonaRequest>
{
    public SetPersonaRequestValidator()
    {
        RuleFor(x => x.Persona)
            .NotEmpty().WithMessage("Persona is required");
    }
}

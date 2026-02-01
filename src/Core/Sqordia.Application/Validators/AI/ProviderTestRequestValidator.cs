using FluentValidation;
using Sqordia.Contracts.Requests.AI;

namespace Sqordia.Application.Validators.AI;

public class ProviderTestRequestValidator : AbstractValidator<ProviderTestRequest>
{
    public ProviderTestRequestValidator()
    {
        RuleFor(x => x.ApiKey)
            .NotEmpty().WithMessage("API key is required");

        RuleFor(x => x.Model)
            .NotEmpty().WithMessage("Model is required");
    }
}

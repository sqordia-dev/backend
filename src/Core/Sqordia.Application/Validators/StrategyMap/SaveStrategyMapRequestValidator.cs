using FluentValidation;
using Sqordia.Contracts.Requests.V2.StrategyMap;

namespace Sqordia.Application.Validators.StrategyMap;

public class SaveStrategyMapRequestValidator : AbstractValidator<SaveStrategyMapRequest>
{
    public SaveStrategyMapRequestValidator()
    {
        RuleFor(x => x.StrategyMapJson)
            .NotEmpty().WithMessage("Strategy map JSON is required");
    }
}

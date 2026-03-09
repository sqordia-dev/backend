using FluentValidation;
using Sqordia.Contracts.Requests.Questionnaire;

namespace Sqordia.Application.Validators.Questionnaire;

public class SubmitAdaptiveResponseRequestValidator : AbstractValidator<SubmitAdaptiveResponseRequest>
{
    public SubmitAdaptiveResponseRequestValidator()
    {
        RuleFor(x => x.QuestionId)
            .NotEmpty().WithMessage("Question ID is required");

        RuleFor(x => x.ResponseText)
            .NotEmpty().WithMessage("Response text is required")
            .MaximumLength(10000).WithMessage("Response text must not exceed 10000 characters");
    }
}

using FluentValidation;
using Sqordia.Contracts.Requests.BusinessPlan;

namespace Sqordia.Application.Validators.BusinessPlan;

public class SubmitQuestionnaireResponseRequestValidator : AbstractValidator<SubmitQuestionnaireResponseRequest>
{
    public SubmitQuestionnaireResponseRequestValidator()
    {
        RuleFor(x => x.QuestionTemplateId)
            .NotEmpty().WithMessage("Question template ID is required");

        RuleFor(x => x.ResponseText)
            .NotEmpty().WithMessage("Response text is required");

        RuleForEach(x => x.SelectedOptions)
            .NotEmpty().WithMessage("Selected option value cannot be empty")
            .When(x => x.SelectedOptions is not null);
    }
}

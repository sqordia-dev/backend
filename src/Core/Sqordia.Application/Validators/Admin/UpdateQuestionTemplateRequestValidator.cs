using FluentValidation;
using Sqordia.Contracts.Requests.Admin;

namespace Sqordia.Application.Validators.Admin;

public class UpdateQuestionTemplateRequestValidator : AbstractValidator<UpdateQuestionTemplateRequest>
{
    private static readonly string[] ValidQuestionTypes =
    {
        "ShortText", "LongText", "SingleChoice", "MultipleChoice",
        "Number", "Currency", "Percentage", "Date", "YesNo", "Scale"
    };

    private static readonly string[] ValidPersonaTypes =
    {
        "Entrepreneur", "Consultant", "OBNL"
    };

    public UpdateQuestionTemplateRequestValidator()
    {
        RuleFor(x => x.QuestionText)
            .MinimumLength(5).WithMessage("Question text must be at least 5 characters")
            .MaximumLength(1000).WithMessage("Question text must not exceed 1000 characters")
            .When(x => x.QuestionText != null);

        RuleFor(x => x.QuestionType)
            .Must(t => t == null || ValidQuestionTypes.Contains(t))
            .WithMessage($"Question type must be one of: {string.Join(", ", ValidQuestionTypes)}");

        RuleFor(x => x.StepNumber)
            .InclusiveBetween(1, 5).WithMessage("Step number must be between 1 and 5")
            .When(x => x.StepNumber.HasValue);

        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(0).WithMessage("Order must be non-negative")
            .When(x => x.Order.HasValue);

        RuleFor(x => x.PersonaType)
            .Must(p => p == null || p == "" || ValidPersonaTypes.Contains(p))
            .WithMessage($"Persona type must be empty (all) or one of: {string.Join(", ", ValidPersonaTypes)}");
    }
}

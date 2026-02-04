using FluentValidation;
using Sqordia.Contracts.Requests.Admin;

namespace Sqordia.Application.Validators.Admin;

public class CreateQuestionTemplateRequestValidator : AbstractValidator<CreateQuestionTemplateRequest>
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

    public CreateQuestionTemplateRequestValidator()
    {
        RuleFor(x => x.QuestionText)
            .NotEmpty().WithMessage("Question text is required")
            .MinimumLength(5).WithMessage("Question text must be at least 5 characters")
            .MaximumLength(1000).WithMessage("Question text must not exceed 1000 characters");

        RuleFor(x => x.QuestionType)
            .NotEmpty().WithMessage("Question type is required")
            .Must(t => ValidQuestionTypes.Contains(t))
            .WithMessage($"Question type must be one of: {string.Join(", ", ValidQuestionTypes)}");

        RuleFor(x => x.StepNumber)
            .InclusiveBetween(1, 5).WithMessage("Step number must be between 1 and 5");

        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(0).WithMessage("Order must be non-negative");

        RuleFor(x => x.PersonaType)
            .Must(p => p == null || ValidPersonaTypes.Contains(p))
            .WithMessage($"Persona type must be null or one of: {string.Join(", ", ValidPersonaTypes)}");

        RuleFor(x => x.Options)
            .NotEmpty().WithMessage("Options are required for choice-type questions")
            .When(x => x.QuestionType is "SingleChoice" or "MultipleChoice");
    }
}

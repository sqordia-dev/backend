using FluentValidation;
using Sqordia.Contracts.Requests.Comment;

namespace Sqordia.Application.Validators.Comment;

public class CreateCommentRequestValidator : AbstractValidator<CreateCommentRequest>
{
    public CreateCommentRequestValidator()
    {
        RuleFor(x => x.BusinessPlanId)
            .NotEmpty().WithMessage("Business plan ID is required");

        RuleFor(x => x.SectionName)
            .NotEmpty().WithMessage("Section name is required")
            .MaximumLength(100).WithMessage("Section name must not exceed 100 characters");

        RuleFor(x => x.CommentText)
            .NotEmpty().WithMessage("Comment text is required")
            .MaximumLength(2000).WithMessage("Comment text must not exceed 2000 characters");
    }
}

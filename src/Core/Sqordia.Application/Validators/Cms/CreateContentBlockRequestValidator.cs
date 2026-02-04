using FluentValidation;
using Sqordia.Contracts.Requests.Cms;

namespace Sqordia.Application.Validators.Cms;

public class CreateContentBlockRequestValidator : AbstractValidator<CreateContentBlockRequest>
{
    private static readonly string[] ValidBlockTypes =
        { "Text", "RichText", "Image", "Link", "Json", "Number", "Boolean" };

    private static readonly string[] ValidLanguages = { "en", "fr" };

    public CreateContentBlockRequestValidator()
    {
        RuleFor(x => x.BlockKey)
            .NotEmpty().WithMessage("Block key is required")
            .MaximumLength(200).WithMessage("Block key must not exceed 200 characters");

        RuleFor(x => x.BlockType)
            .NotEmpty().WithMessage("Block type is required")
            .Must(bt => ValidBlockTypes.Contains(bt))
            .WithMessage("Block type must be one of: Text, RichText, Image, Link, Json, Number, Boolean");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required");

        RuleFor(x => x.Language)
            .NotEmpty().WithMessage("Language is required")
            .MaximumLength(5).WithMessage("Language must not exceed 5 characters")
            .Must(lang => ValidLanguages.Contains(lang))
            .WithMessage("Language must be one of: en, fr");

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Sort order must be greater than or equal to 0");

        RuleFor(x => x.SectionKey)
            .NotEmpty().WithMessage("Section key is required")
            .MaximumLength(200).WithMessage("Section key must not exceed 200 characters");

        RuleFor(x => x.Metadata)
            .MaximumLength(2000).WithMessage("Metadata must not exceed 2000 characters")
            .When(x => x.Metadata is not null);
    }
}

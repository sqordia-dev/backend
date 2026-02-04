using FluentValidation;
using Sqordia.Contracts.Requests.Cms;

namespace Sqordia.Application.Validators.Cms;

public class BulkUpdateContentBlocksRequestValidator : AbstractValidator<BulkUpdateContentBlocksRequest>
{
    public BulkUpdateContentBlocksRequestValidator()
    {
        RuleFor(x => x.Blocks)
            .NotEmpty().WithMessage("At least one block is required");

        RuleForEach(x => x.Blocks).ChildRules(block =>
        {
            block.RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Block ID is required");

            block.RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Content is required");

            block.RuleFor(x => x.Metadata)
                .MaximumLength(2000).WithMessage("Metadata must not exceed 2000 characters")
                .When(x => x.Metadata is not null);
        });
    }
}

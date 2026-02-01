using FluentValidation;
using Sqordia.Contracts.Requests.Financial;

namespace Sqordia.Application.Validators.Financial;

public class UpdateFinancialCellRequestValidator : AbstractValidator<UpdateFinancialCellRequest>
{
    private static readonly string[] ValidCellTypes = ["number", "percentage", "currency", "text", "formula", "date"];

    public UpdateFinancialCellRequestValidator()
    {
        RuleFor(x => x.RowId)
            .NotEmpty().WithMessage("Row ID is required");

        RuleFor(x => x.CellId)
            .NotEmpty().WithMessage("Cell ID is required");

        RuleFor(x => x.Value)
            .NotNull().WithMessage("Value is required");

        RuleFor(x => x.Formula)
            .MaximumLength(500).WithMessage("Formula must not exceed 500 characters")
            .When(x => x.Formula is not null);

        RuleFor(x => x.SheetName)
            .MaximumLength(100).WithMessage("Sheet name must not exceed 100 characters")
            .When(x => x.SheetName is not null);

        RuleFor(x => x.CellType)
            .Must(type => ValidCellTypes.Contains(type!.ToLowerInvariant()))
            .WithMessage("Cell type must be one of: number, percentage, currency, text, formula, date")
            .When(x => x.CellType is not null);
    }
}

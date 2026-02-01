using FluentValidation;
using Sqordia.Contracts.Requests.Export;

namespace Sqordia.Application.Validators.Export;

public class ExportWithVisualsRequestValidator : AbstractValidator<ExportWithVisualsRequest>
{
    private static readonly string[] ValidFormats = { "pdf", "docx", "html" };
    private static readonly string[] ValidLanguages = { "fr", "en" };

    public ExportWithVisualsRequestValidator()
    {
        RuleFor(x => x.Format)
            .NotEmpty().WithMessage("Format is required")
            .Must(f => ValidFormats.Contains(f))
            .WithMessage("Format must be one of: pdf, docx, html");

        RuleFor(x => x.Language)
            .NotEmpty().WithMessage("Language is required")
            .Must(l => ValidLanguages.Contains(l))
            .WithMessage("Language must be one of: fr, en");
    }
}

using FluentValidation;
using Sqordia.Contracts.Requests.CoverPage;

namespace Sqordia.Application.Validators.CoverPage;

public class UpdateCoverPageRequestValidator : AbstractValidator<UpdateCoverPageRequest>
{
    private static readonly string[] ValidLayoutStyles = { "classic", "modern", "minimal", "bold", "creative", "elegant" };

    public UpdateCoverPageRequestValidator()
    {
        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage("Company name is required")
            .MaximumLength(200).WithMessage("Company name must not exceed 200 characters");

        RuleFor(x => x.DocumentTitle)
            .NotEmpty().WithMessage("Document title is required")
            .MaximumLength(100).WithMessage("Document title must not exceed 100 characters");

        RuleFor(x => x.PrimaryColor)
            .NotEmpty().WithMessage("Primary color is required")
            .Length(7).WithMessage("Primary color must be exactly 7 characters (e.g. #FF0000)")
            .Matches(@"^#[0-9A-Fa-f]{6}$").WithMessage("Primary color must be a valid hex color (e.g. #FF0000)");

        RuleFor(x => x.LayoutStyle)
            .NotEmpty().WithMessage("Layout style is required")
            .Must(ls => ValidLayoutStyles.Contains(ls))
            .WithMessage("Layout style must be one of: classic, modern, minimal, bold, creative, elegant");

        RuleFor(x => x.ContactName)
            .MaximumLength(100).WithMessage("Contact name must not exceed 100 characters")
            .When(x => x.ContactName is not null);

        RuleFor(x => x.ContactTitle)
            .MaximumLength(100).WithMessage("Contact title must not exceed 100 characters")
            .When(x => x.ContactTitle is not null);

        RuleFor(x => x.ContactPhone)
            .MaximumLength(30).WithMessage("Contact phone must not exceed 30 characters")
            .When(x => x.ContactPhone is not null);

        RuleFor(x => x.ContactEmail)
            .MaximumLength(200).WithMessage("Contact email must not exceed 200 characters")
            .EmailAddress().WithMessage("Contact email must be a valid email address")
            .When(x => x.ContactEmail is not null);

        RuleFor(x => x.Website)
            .MaximumLength(200).WithMessage("Website must not exceed 200 characters")
            .When(x => x.Website is not null);

        RuleFor(x => x.AddressLine1)
            .MaximumLength(200).WithMessage("Address line 1 must not exceed 200 characters")
            .When(x => x.AddressLine1 is not null);

        RuleFor(x => x.AddressLine2)
            .MaximumLength(200).WithMessage("Address line 2 must not exceed 200 characters")
            .When(x => x.AddressLine2 is not null);

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("City must not exceed 100 characters")
            .When(x => x.City is not null);

        RuleFor(x => x.StateProvince)
            .MaximumLength(100).WithMessage("State/Province must not exceed 100 characters")
            .When(x => x.StateProvince is not null);

        RuleFor(x => x.PostalCode)
            .MaximumLength(20).WithMessage("Postal code must not exceed 20 characters")
            .When(x => x.PostalCode is not null);

        RuleFor(x => x.Country)
            .MaximumLength(100).WithMessage("Country must not exceed 100 characters")
            .When(x => x.Country is not null);
    }
}

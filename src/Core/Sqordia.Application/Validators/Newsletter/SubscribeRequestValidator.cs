using FluentValidation;
using Sqordia.Contracts.Requests.Newsletter;

namespace Sqordia.Application.Validators.Newsletter;

public class SubscribeRequestValidator : AbstractValidator<SubscribeRequest>
{
    public SubscribeRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Valid email address is required")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters");

        RuleFor(x => x.Language)
            .Must(lang => lang is "fr" or "en")
            .WithMessage("Language must be 'fr' or 'en'");
    }
}

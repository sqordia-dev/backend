using FluentValidation;
using Sqordia.Contracts.Requests.Pricing;

namespace Sqordia.Application.Validators.Pricing;

public class PricingAnalysisRequestValidator : AbstractValidator<PricingAnalysisRequest>
{
    private static readonly string[] ValidLanguages = { "fr", "en" };

    public PricingAnalysisRequestValidator()
    {
        RuleFor(x => x.BusinessPlanId)
            .NotEmpty().WithMessage("Business plan ID is required");

        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200).WithMessage("Product name must not exceed 200 characters");

        RuleFor(x => x.ProductDescription)
            .MaximumLength(1000).WithMessage("Product description must not exceed 1000 characters")
            .When(x => x.ProductDescription is not null);

        RuleFor(x => x.TargetMarket)
            .MaximumLength(200).WithMessage("Target market must not exceed 200 characters")
            .When(x => x.TargetMarket is not null);

        RuleFor(x => x.CostPerUnit)
            .GreaterThanOrEqualTo(0).WithMessage("Cost per unit must be a positive value")
            .When(x => x.CostPerUnit.HasValue);

        RuleFor(x => x.MarketSize)
            .GreaterThanOrEqualTo(0).WithMessage("Market size must be a positive value")
            .When(x => x.MarketSize.HasValue);

        RuleFor(x => x.Industry)
            .MaximumLength(200).WithMessage("Industry must not exceed 200 characters")
            .When(x => x.Industry is not null);

        RuleFor(x => x.Language)
            .Must(BeValidLanguage)
            .WithMessage("Language must be 'fr' or 'en'");

        RuleForEach(x => x.Competitors)
            .SetValidator(new CompetitorInfoValidator())
            .When(x => x.Competitors is not null);
    }

    private static bool BeValidLanguage(string language)
    {
        return ValidLanguages.Contains(language.ToLowerInvariant());
    }
}

public class CompetitorInfoValidator : AbstractValidator<CompetitorInfo>
{
    public CompetitorInfoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Competitor name is required")
            .MaximumLength(200).WithMessage("Competitor name must not exceed 200 characters");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Competitor price must be a positive value")
            .When(x => x.Price.HasValue);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Competitor description must not exceed 500 characters")
            .When(x => x.Description is not null);

        RuleFor(x => x.Strengths)
            .MaximumLength(500).WithMessage("Competitor strengths must not exceed 500 characters")
            .When(x => x.Strengths is not null);

        RuleFor(x => x.Weaknesses)
            .MaximumLength(500).WithMessage("Competitor weaknesses must not exceed 500 characters")
            .When(x => x.Weaknesses is not null);
    }
}

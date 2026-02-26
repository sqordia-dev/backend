using FluentValidation;
using Sqordia.Contracts.Requests.GitHub;

namespace Sqordia.Application.Validators.GitHub;

public class CreateGitHubIssueRequestValidator : AbstractValidator<CreateGitHubIssueRequest>
{
    private static readonly string[] ValidSeverities = { "Low", "Medium", "High", "Critical" };
    private static readonly string[] ValidCategories = { "Bug", "Feature", "Enhancement", "Documentation", "Performance" };
    private static readonly string[] ValidRepositories = { "frontend", "backend" };

    public CreateGitHubIssueRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MinimumLength(5).WithMessage("Title must be at least 5 characters")
            .MaximumLength(256).WithMessage("Title must not exceed 256 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MinimumLength(20).WithMessage("Description must be at least 20 characters")
            .MaximumLength(10000).WithMessage("Description must not exceed 10000 characters");

        RuleFor(x => x.Severity)
            .NotEmpty().WithMessage("Severity is required")
            .Must(s => ValidSeverities.Contains(s, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Severity must be one of: {string.Join(", ", ValidSeverities)}");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required")
            .Must(c => ValidCategories.Contains(c, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Category must be one of: {string.Join(", ", ValidCategories)}");

        RuleFor(x => x.Repository)
            .NotEmpty().WithMessage("Repository is required")
            .Must(r => ValidRepositories.Contains(r, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Repository must be one of: {string.Join(", ", ValidRepositories)}");

        RuleFor(x => x.ReproductionSteps)
            .MaximumLength(5000).WithMessage("Reproduction steps must not exceed 5000 characters")
            .When(x => !string.IsNullOrEmpty(x.ReproductionSteps));
    }
}

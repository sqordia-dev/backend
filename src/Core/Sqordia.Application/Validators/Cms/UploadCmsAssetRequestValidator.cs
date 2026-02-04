using FluentValidation;
using Sqordia.Contracts.Requests.Cms;

namespace Sqordia.Application.Validators.Cms;

public class UploadCmsAssetRequestValidator : AbstractValidator<UploadCmsAssetRequest>
{
    private const long MaxFileSize = 5_242_880; // 5 MB

    private static readonly string[] AllowedContentTypes =
        { "image/png", "image/jpeg", "image/svg+xml", "image/webp" };

    public UploadCmsAssetRequestValidator()
    {
        RuleFor(x => x.File)
            .NotNull().WithMessage("File is required");

        RuleFor(x => x.File.Length)
            .GreaterThan(0).WithMessage("File must not be empty")
            .LessThanOrEqualTo(MaxFileSize).WithMessage("File size must not exceed 5 MB")
            .When(x => x.File is not null);

        RuleFor(x => x.File.ContentType)
            .Must(ct => AllowedContentTypes.Contains(ct))
            .WithMessage("File type must be one of: image/png, image/jpeg, image/svg+xml, image/webp")
            .When(x => x.File is not null);

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required")
            .MaximumLength(100).WithMessage("Category must not exceed 100 characters");
    }
}

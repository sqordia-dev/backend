using FluentValidation;
using Sqordia.Contracts.Requests.Financial;

namespace Sqordia.Application.Validators.Financial;

public class CalculateConsultantFinancialsRequestValidator : AbstractValidator<CalculateConsultantFinancialsRequest>
{
    public CalculateConsultantFinancialsRequestValidator()
    {
        RuleFor(x => x.HourlyRate)
            .GreaterThanOrEqualTo(0).WithMessage("Hourly rate must be greater than or equal to 0");

        RuleFor(x => x.UtilizationPercent)
            .GreaterThanOrEqualTo(0).WithMessage("Utilization percentage must be greater than or equal to 0")
            .LessThanOrEqualTo(100).WithMessage("Utilization percentage must not exceed 100");

        RuleFor(x => x.ClientAcquisitionCost)
            .GreaterThanOrEqualTo(0).WithMessage("Client acquisition cost must be greater than or equal to 0");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required")
            .MaximumLength(100).WithMessage("City must not exceed 100 characters");

        RuleFor(x => x.Province)
            .NotEmpty().WithMessage("Province is required")
            .MaximumLength(100).WithMessage("Province must not exceed 100 characters");
    }
}

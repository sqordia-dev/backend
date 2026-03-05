using FluentValidation;
using Sqordia.Contracts.Requests.Financial.Previsio;

namespace Sqordia.Application.Validators.Financial.Previsio;

public class CreateFinancialPlanRequestValidator : AbstractValidator<CreateFinancialPlanRequest>
{
    public CreateFinancialPlanRequestValidator()
    {
        RuleFor(x => x.StartYear)
            .InclusiveBetween(2020, 2050).WithMessage("Start year must be between 2020 and 2050");

        RuleFor(x => x.ProjectionYears)
            .InclusiveBetween(1, 5).WithMessage("Projection years must be between 1 and 5");
    }
}

public class UpdateFinancialPlanSettingsRequestValidator : AbstractValidator<UpdateFinancialPlanSettingsRequest>
{
    public UpdateFinancialPlanSettingsRequestValidator()
    {
        RuleFor(x => x.ProjectionYears)
            .InclusiveBetween(1, 5).WithMessage("Projection years must be between 1 and 5");

        RuleFor(x => x.DefaultVolumeGrowthRate)
            .InclusiveBetween(-100, 100).WithMessage("Volume growth rate must be between -100% and 100%");

        RuleFor(x => x.DefaultPriceIndexationRate)
            .InclusiveBetween(-100, 100).WithMessage("Price indexation rate must be between -100% and 100%");

        RuleFor(x => x.DefaultExpenseIndexationRate)
            .InclusiveBetween(-100, 100).WithMessage("Expense indexation rate must be between -100% and 100%");

        RuleFor(x => x.DefaultSocialChargeRate)
            .InclusiveBetween(0, 100).WithMessage("Social charge rate must be between 0% and 100%");

        RuleFor(x => x.DefaultSalesTaxRate)
            .InclusiveBetween(0, 100).WithMessage("Sales tax rate must be between 0% and 100%");
    }
}

public class CreateSalesProductRequestValidator : AbstractValidator<CreateSalesProductRequest>
{
    public CreateSalesProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200).WithMessage("Product name must not exceed 200 characters");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("Unit price must be greater than 0");

        RuleFor(x => x.PaymentDelay)
            .NotEmpty().WithMessage("Payment delay is required");

        RuleFor(x => x.TaxRate)
            .InclusiveBetween(0, 100).WithMessage("Tax rate must be between 0% and 100%");
    }
}

public class UpdateSalesProductRequestValidator : AbstractValidator<UpdateSalesProductRequest>
{
    public UpdateSalesProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200).WithMessage("Product name must not exceed 200 characters");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("Unit price must be greater than 0");

        RuleFor(x => x.TaxRate)
            .InclusiveBetween(0, 100).WithMessage("Tax rate must be between 0% and 100%");
    }
}

public class UpdateSalesVolumeGridRequestValidator : AbstractValidator<UpdateSalesVolumeGridRequest>
{
    public UpdateSalesVolumeGridRequestValidator()
    {
        RuleFor(x => x.SalesProductId)
            .NotEmpty().WithMessage("Sales product ID is required");

        RuleFor(x => x.Year)
            .InclusiveBetween(0, 5).WithMessage("Year must be between 0 (pre-opening) and 5");

        RuleForEach(x => x.MonthlyValues)
            .ChildRules(mv =>
            {
                mv.RuleFor(m => m.Month).InclusiveBetween(0, 12).WithMessage("Month must be between 0 and 12");
                mv.RuleFor(m => m.Value).GreaterThanOrEqualTo(0).WithMessage("Quantity must be non-negative");
            });
    }
}

public class CreatePayrollItemRequestValidator : AbstractValidator<CreatePayrollItemRequest>
{
    public CreatePayrollItemRequestValidator()
    {
        RuleFor(x => x.JobTitle)
            .NotEmpty().WithMessage("Job title is required")
            .MaximumLength(200).WithMessage("Job title must not exceed 200 characters");

        RuleFor(x => x.SalaryAmount)
            .GreaterThan(0).WithMessage("Salary amount must be greater than 0");

        RuleFor(x => x.SocialChargeRate)
            .InclusiveBetween(0, 100).WithMessage("Social charge rate must be between 0% and 100%");

        RuleFor(x => x.HeadCount)
            .GreaterThan(0).WithMessage("Head count must be at least 1");
    }
}

public class CreateFinancingSourceRequestValidator : AbstractValidator<CreateFinancingSourceRequest>
{
    public CreateFinancingSourceRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Source name is required")
            .MaximumLength(200).WithMessage("Source name must not exceed 200 characters");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0");

        RuleFor(x => x.InterestRate)
            .InclusiveBetween(0, 100).WithMessage("Interest rate must be between 0% and 100%");

        RuleFor(x => x.TermMonths)
            .GreaterThanOrEqualTo(0).WithMessage("Term must be non-negative");

        RuleFor(x => x.MoratoireMonths)
            .GreaterThanOrEqualTo(0).WithMessage("Moratoire must be non-negative")
            .LessThanOrEqualTo(x => x.TermMonths).WithMessage("Moratoire cannot exceed term")
            .When(x => x.TermMonths > 0);
    }
}

public class CreateCapexAssetRequestValidator : AbstractValidator<CreateCapexAssetRequest>
{
    public CreateCapexAssetRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Asset name is required")
            .MaximumLength(200).WithMessage("Asset name must not exceed 200 characters");

        RuleFor(x => x.PurchaseValue)
            .GreaterThan(0).WithMessage("Purchase value must be greater than 0");

        RuleFor(x => x.PurchaseMonth)
            .InclusiveBetween(0, 12).WithMessage("Purchase month must be between 0 and 12");

        RuleFor(x => x.PurchaseYear)
            .InclusiveBetween(0, 5).WithMessage("Purchase year must be between 0 (pre-opening) and 5");
    }
}

public class CreateSalesExpenseRequestValidator : AbstractValidator<CreateSalesExpenseRequest>
{
    public CreateSalesExpenseRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Expense name is required")
            .MaximumLength(200).WithMessage("Expense name must not exceed 200 characters");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0");
    }
}

public class CreateAdminExpenseRequestValidator : AbstractValidator<CreateAdminExpenseRequest>
{
    public CreateAdminExpenseRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Expense name is required")
            .MaximumLength(200).WithMessage("Expense name must not exceed 200 characters");

        RuleFor(x => x.MonthlyAmount)
            .GreaterThan(0).WithMessage("Monthly amount must be greater than 0");
    }
}

public class UpdateProjectCostSettingsRequestValidator : AbstractValidator<UpdateProjectCostSettingsRequest>
{
    public UpdateProjectCostSettingsRequestValidator()
    {
        RuleFor(x => x.WorkingCapitalMonthsCOGS)
            .InclusiveBetween(0, 24).WithMessage("Working capital months must be between 0 and 24");

        RuleFor(x => x.WorkingCapitalMonthsPayroll)
            .InclusiveBetween(0, 24).WithMessage("Working capital months must be between 0 and 24");

        RuleFor(x => x.WorkingCapitalMonthsSalesExpenses)
            .InclusiveBetween(0, 24).WithMessage("Working capital months must be between 0 and 24");

        RuleFor(x => x.WorkingCapitalMonthsAdminExpenses)
            .InclusiveBetween(0, 24).WithMessage("Working capital months must be between 0 and 24");

        RuleFor(x => x.CapexInclusionMonths)
            .InclusiveBetween(0, 36).WithMessage("CAPEX inclusion months must be between 0 and 36");
    }
}

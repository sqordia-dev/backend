namespace Sqordia.Application.Contracts.Responses;

/// <summary>
/// Preview of what a plan change will cost, including proration details.
/// </summary>
public class PlanChangePreviewDto
{
    /// <summary>Current plan name</summary>
    public string CurrentPlanName { get; set; } = null!;

    /// <summary>New plan name</summary>
    public string NewPlanName { get; set; } = null!;

    /// <summary>Current plan type (Free, Starter, Professional, Enterprise)</summary>
    public string CurrentPlanType { get; set; } = null!;

    /// <summary>New plan type</summary>
    public string NewPlanType { get; set; } = null!;

    /// <summary>True if upgrading (higher tier), false if downgrading</summary>
    public bool IsUpgrade { get; set; }

    /// <summary>Days remaining in current billing period</summary>
    public int RemainingDays { get; set; }

    /// <summary>Total days in current billing period</summary>
    public int TotalDays { get; set; }

    /// <summary>Current subscription end date (renewal date)</summary>
    public DateTime CurrentPeriodEnd { get; set; }

    /// <summary>Credit for unused portion of current plan (positive value)</summary>
    public decimal CreditAmount { get; set; }

    /// <summary>Charge for remaining days on new plan (positive value)</summary>
    public decimal ChargeAmount { get; set; }

    /// <summary>
    /// Net amount due now. Positive = user pays, negative = credit applied.
    /// </summary>
    public decimal NetAmount { get; set; }

    /// <summary>New recurring amount starting next billing period</summary>
    public decimal NewRecurringAmount { get; set; }

    /// <summary>Currency code (e.g. CAD)</summary>
    public string Currency { get; set; } = "CAD";

    /// <summary>Whether the new plan uses yearly billing</summary>
    public bool IsYearly { get; set; }

    /// <summary>Effective date of the plan change (now)</summary>
    public DateTime EffectiveDate { get; set; }

    /// <summary>New billing period end date after the switch</summary>
    public DateTime NewPeriodEnd { get; set; }

    /// <summary>Tax amount (HST 13%)</summary>
    public decimal TaxAmount { get; set; }

    /// <summary>Total with tax</summary>
    public decimal TotalWithTax { get; set; }
}

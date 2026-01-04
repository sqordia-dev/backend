using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities.BusinessPlan;

/// <summary>
/// Financial projection data for a business plan
/// Supports monthly/quarterly/yearly projections
/// </summary>
public class FinancialProjection : BaseEntity
{
    public Guid BusinessPlanId { get; private set; }
    public int Year { get; private set; }
    public int? Month { get; private set; } // Null for yearly, 1-12 for monthly
    public int? Quarter { get; private set; } // Null or 1-4 for quarterly
    
    // Revenue projections
    public decimal? Revenue { get; private set; }
    public decimal? RevenueGrowthRate { get; private set; } // Percentage
    
    // Cost projections
    public decimal? CostOfGoodsSold { get; private set; }
    public decimal? OperatingExpenses { get; private set; }
    public decimal? MarketingExpenses { get; private set; }
    public decimal? RAndDExpenses { get; private set; }
    public decimal? AdministrativeExpenses { get; private set; }
    public decimal? OtherExpenses { get; private set; }
    
    // Calculated fields
    public decimal? GrossProfit { get; private set; }
    public decimal? NetIncome { get; private set; }
    public decimal? EBITDA { get; private set; }
    
    // Cash flow
    public decimal? CashFlow { get; private set; }
    public decimal? CashBalance { get; private set; }
    
    // Headcount
    public int? Employees { get; private set; }
    
    // Customers/Units
    public int? Customers { get; private set; }
    public int? UnitsSold { get; private set; }
    public decimal? AverageRevenuePerCustomer { get; private set; }
    
    // Notes and assumptions
    public string? Notes { get; private set; }
    public string? Assumptions { get; private set; }
    
    // Navigation properties
    public BusinessPlan BusinessPlan { get; private set; } = null!;
    
    private FinancialProjection() { } // EF Core constructor
    
    public FinancialProjection(Guid businessPlanId, int year, int? month = null, int? quarter = null)
    {
        BusinessPlanId = businessPlanId;
        Year = year;
        Month = month;
        Quarter = quarter;
        
        // Validate month and quarter
        if (month.HasValue && (month < 1 || month > 12))
            throw new ArgumentException("Month must be between 1 and 12", nameof(month));
            
        if (quarter.HasValue && (quarter < 1 || quarter > 4))
            throw new ArgumentException("Quarter must be between 1 and 4", nameof(quarter));
    }
    
    public void SetRevenue(decimal revenue, decimal? growthRate = null)
    {
        Revenue = revenue;
        RevenueGrowthRate = growthRate;
        RecalculateProfitability();
    }
    
    public void SetCosts(
        decimal? cogs = null,
        decimal? opex = null,
        decimal? marketing = null,
        decimal? rnd = null,
        decimal? admin = null,
        decimal? other = null)
    {
        CostOfGoodsSold = cogs;
        OperatingExpenses = opex;
        MarketingExpenses = marketing;
        RAndDExpenses = rnd;
        AdministrativeExpenses = admin;
        OtherExpenses = other;
        RecalculateProfitability();
    }
    
    public void SetCashFlow(decimal cashFlow, decimal? cashBalance = null)
    {
        CashFlow = cashFlow;
        CashBalance = cashBalance;
    }
    
    public void SetMetrics(int? employees = null, int? customers = null, int? unitsSold = null)
    {
        Employees = employees;
        Customers = customers;
        UnitsSold = unitsSold;
        
        if (customers.HasValue && customers > 0 && Revenue.HasValue)
        {
            AverageRevenuePerCustomer = Revenue.Value / customers.Value;
        }
    }
    
    public void SetNotes(string? notes, string? assumptions = null)
    {
        Notes = notes;
        Assumptions = assumptions;
    }
    
    private void RecalculateProfitability()
    {
        if (Revenue.HasValue && CostOfGoodsSold.HasValue)
        {
            GrossProfit = Revenue.Value - CostOfGoodsSold.Value;
        }
        
        var totalExpenses = (OperatingExpenses ?? 0) + (MarketingExpenses ?? 0) +
                           (RAndDExpenses ?? 0) + (AdministrativeExpenses ?? 0) +
                           (OtherExpenses ?? 0);
        
        if (GrossProfit.HasValue)
        {
            NetIncome = GrossProfit.Value - totalExpenses;
            EBITDA = NetIncome; // Simplified - would need depreciation/amortization for accuracy
        }
    }
}


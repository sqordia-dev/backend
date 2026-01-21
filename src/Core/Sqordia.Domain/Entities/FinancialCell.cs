using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities;

/// <summary>
/// Represents a cell in a financial spreadsheet with optional formula support
/// </summary>
public class FinancialCell : BaseEntity
{
    public Guid BusinessPlanId { get; set; }
    public string SheetName { get; set; } = "Main";      // e.g., "Revenue", "Expenses", "Main"
    public string RowId { get; set; } = string.Empty;    // e.g., "revenue_product_a", "expenses_marketing"
    public string ColumnId { get; set; } = string.Empty; // e.g., "2024_Q1", "2024_M01", "year_1"
    public decimal Value { get; set; }
    public string? Formula { get; set; }                 // e.g., "=SUM(A1:A5)", "=B1*1.15"
    public bool IsCalculated { get; set; }               // true if cell has formula
    public string CellType { get; set; } = "number";     // "number", "percentage", "currency"
    public string? DisplayFormat { get; set; }           // e.g., "#,##0.00", "0%"
    public bool IsLocked { get; set; }                   // true if cell cannot be edited
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public virtual Domain.Entities.BusinessPlan.BusinessPlan? BusinessPlan { get; set; }

    /// <summary>
    /// Gets the unique cell reference for this cell (e.g., "Main!revenue_product_a_2024_Q1")
    /// </summary>
    public string GetCellReference() => $"{SheetName}!{RowId}_{ColumnId}";
}

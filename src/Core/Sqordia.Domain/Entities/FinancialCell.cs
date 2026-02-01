using System.Text.RegularExpressions;
using Sqordia.Domain.Common;
using Sqordia.Domain.Enums;

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
    public CellType CellType { get; set; } = CellType.Number;
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

    /// <summary>
    /// Validates that the formula has basic valid syntax when CellType is Formula or IsCalculated is true.
    /// Returns true if valid or no formula is present.
    /// </summary>
    public bool ValidateFormula()
    {
        if (string.IsNullOrWhiteSpace(Formula))
            return CellType != CellType.Formula; // Formula type must have a formula

        // Must start with =
        if (!Formula.StartsWith('='))
            return false;

        // Must have content after =
        if (Formula.Length < 2)
            return false;

        // Check for balanced parentheses
        var depth = 0;
        foreach (var c in Formula)
        {
            if (c == '(') depth++;
            if (c == ')') depth--;
            if (depth < 0) return false;
        }

        return depth == 0;
    }

    /// <summary>
    /// Validates that the DisplayFormat is appropriate for the CellType.
    /// Returns true if valid or no display format is set.
    /// </summary>
    public bool ValidateDisplayFormat()
    {
        if (string.IsNullOrWhiteSpace(DisplayFormat))
            return true;

        return CellType switch
        {
            CellType.Number => Regex.IsMatch(DisplayFormat, @"^[#0,.\s]+$"),
            CellType.Percentage => DisplayFormat.Contains('%'),
            CellType.Currency => Regex.IsMatch(DisplayFormat, @"^[$€£¥]?[#0,.\s]+$"),
            CellType.Text => true,
            CellType.Formula => true,
            CellType.Date => true,
            _ => true
        };
    }

    /// <summary>
    /// Parses a string cell type to the CellType enum. Returns Number as default for unknown values.
    /// </summary>
    public static CellType ParseCellType(string? cellType)
    {
        if (string.IsNullOrWhiteSpace(cellType))
            return CellType.Number;

        return cellType.ToLowerInvariant() switch
        {
            "number" => CellType.Number,
            "percentage" => CellType.Percentage,
            "currency" => CellType.Currency,
            "text" => CellType.Text,
            "formula" => CellType.Formula,
            "date" => CellType.Date,
            _ => CellType.Number
        };
    }
}

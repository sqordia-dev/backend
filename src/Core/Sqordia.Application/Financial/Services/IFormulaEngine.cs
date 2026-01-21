using Sqordia.Domain.Entities;

namespace Sqordia.Application.Financial.Services;

/// <summary>
/// Interface for a spreadsheet-like formula engine that handles cell calculations
/// </summary>
public interface IFormulaEngine
{
    /// <summary>
    /// Evaluates a formula and returns the calculated value
    /// </summary>
    /// <param name="formula">The formula to evaluate (e.g., "=SUM(A1:A5)", "=B1*1.15")</param>
    /// <param name="cellValues">Dictionary mapping cell references to their values</param>
    /// <returns>The calculated result</returns>
    decimal Evaluate(string formula, Dictionary<string, decimal> cellValues);

    /// <summary>
    /// Parses a formula and extracts all cell references it depends on
    /// </summary>
    /// <param name="formula">The formula to parse</param>
    /// <returns>List of cell references (e.g., ["A1", "B1:B5"])</returns>
    List<string> ParseDependencies(string formula);

    /// <summary>
    /// Expands range references into individual cell references
    /// </summary>
    /// <param name="range">A range like "A1:A5"</param>
    /// <returns>List of individual cells ["A1", "A2", "A3", "A4", "A5"]</returns>
    List<string> ExpandRange(string range);

    /// <summary>
    /// Validates that a formula is syntactically correct
    /// </summary>
    /// <param name="formula">The formula to validate</param>
    /// <returns>Tuple of (isValid, errorMessage)</returns>
    (bool IsValid, string? ErrorMessage) ValidateFormula(string formula);

    /// <summary>
    /// Detects if adding a formula would create a circular dependency
    /// </summary>
    /// <param name="targetCell">The cell that would contain the formula</param>
    /// <param name="formula">The formula to check</param>
    /// <param name="existingCells">Existing cells with their formulas</param>
    /// <returns>True if a circular dependency would be created</returns>
    bool WouldCreateCircularDependency(string targetCell, string formula, IEnumerable<FinancialCell> existingCells);

    /// <summary>
    /// Calculates all cells that need to be recalculated when a cell value changes.
    /// Returns cells in the correct calculation order (topological sort).
    /// </summary>
    /// <param name="changedCellRef">The reference of the cell that changed</param>
    /// <param name="allCells">All cells in the spreadsheet</param>
    /// <returns>Ordered list of cells that need recalculation</returns>
    List<FinancialCell> GetCellsToRecalculate(string changedCellRef, IEnumerable<FinancialCell> allCells);

    /// <summary>
    /// Recalculates all dependent cells after a value change
    /// </summary>
    /// <param name="changedCell">The cell whose value changed</param>
    /// <param name="newValue">The new value of the cell</param>
    /// <param name="allCells">All cells in the spreadsheet</param>
    /// <returns>Dictionary of updated cell references and their new values</returns>
    Dictionary<string, decimal> RecalculateDependents(FinancialCell changedCell, decimal newValue, IEnumerable<FinancialCell> allCells);
}

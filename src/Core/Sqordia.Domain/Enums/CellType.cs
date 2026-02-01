namespace Sqordia.Domain.Enums;

/// <summary>
/// Type of financial cell determining how its value is interpreted and displayed
/// </summary>
public enum CellType
{
    /// <summary>
    /// Plain numeric value
    /// </summary>
    Number = 0,

    /// <summary>
    /// Percentage value (e.g., 15.5%)
    /// </summary>
    Percentage = 1,

    /// <summary>
    /// Currency/monetary value
    /// </summary>
    Currency = 2,

    /// <summary>
    /// Text/label cell
    /// </summary>
    Text = 3,

    /// <summary>
    /// Formula-based calculated cell
    /// </summary>
    Formula = 4,

    /// <summary>
    /// Date value
    /// </summary>
    Date = 5
}

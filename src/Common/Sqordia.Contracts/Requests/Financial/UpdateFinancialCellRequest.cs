using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.Financial;

public class UpdateFinancialCellRequest
{
    [Required]
    public string RowId { get; set; } = null!;

    [Required]
    public string CellId { get; set; } = null!;

    [Required]
    public decimal Value { get; set; }

    /// <summary>
    /// Optional formula for calculated cells (e.g., "=SUM(A1:A5)", "=B1*1.15")
    /// </summary>
    public string? Formula { get; set; }

    /// <summary>
    /// Sheet name for organizing cells (defaults to "Main")
    /// </summary>
    public string? SheetName { get; set; }

    /// <summary>
    /// Cell type: "number", "percentage", "currency" (defaults to "number")
    /// </summary>
    public string? CellType { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.Privacy;

/// <summary>
/// Request to delete user account (Quebec Bill 25 compliance)
/// </summary>
public class AccountDeletionRequest
{
    /// <summary>
    /// Type of deletion: "Deactivate" (soft delete, recoverable) or "Permanent" (hard delete, irreversible)
    /// </summary>
    [Required]
    public required string DeletionType { get; set; }

    /// <summary>
    /// User's password for confirmation
    /// </summary>
    [Required]
    public required string Password { get; set; }

    /// <summary>
    /// Optional reason for leaving (feedback)
    /// </summary>
    [MaxLength(500)]
    public string? Reason { get; set; }
}

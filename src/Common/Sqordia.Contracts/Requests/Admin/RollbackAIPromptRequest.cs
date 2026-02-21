using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.Admin;

/// <summary>
/// Request to rollback an AI prompt to a previous version
/// </summary>
public class RollbackAIPromptRequest
{
    /// <summary>
    /// The version number to rollback to
    /// </summary>
    [Required]
    [Range(1, int.MaxValue)]
    public required int TargetVersion { get; set; }

    /// <summary>
    /// Optional note explaining the reason for rollback
    /// </summary>
    [StringLength(500)]
    public string? Notes { get; set; }
}

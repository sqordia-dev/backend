using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.Cms;

/// <summary>
/// Request to update an existing CMS version
/// </summary>
public class UpdateCmsVersionRequest
{
    /// <summary>
    /// Optional notes describing the version changes
    /// </summary>
    [StringLength(500)]
    public string? Notes { get; set; }
}

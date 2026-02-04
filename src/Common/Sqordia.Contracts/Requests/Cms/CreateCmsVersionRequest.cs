using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.Cms;

/// <summary>
/// Request to create a new CMS version (draft)
/// </summary>
public class CreateCmsVersionRequest
{
    /// <summary>
    /// Optional notes describing the version changes
    /// </summary>
    [StringLength(500)]
    public string? Notes { get; set; }
}

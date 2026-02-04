using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Sqordia.Contracts.Requests.Cms;

/// <summary>
/// Request to upload an asset (image) for CMS content
/// </summary>
public class UploadCmsAssetRequest
{
    /// <summary>
    /// The file to upload
    /// </summary>
    [Required]
    public required IFormFile File { get; set; }

    /// <summary>
    /// Asset category for organization (e.g., "hero", "logo", "banner")
    /// </summary>
    [Required]
    [StringLength(100)]
    public required string Category { get; set; }
}

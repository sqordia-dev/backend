using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.Privacy;

/// <summary>
/// Request to update user consent (Quebec Bill 25 compliance)
/// </summary>
public class UpdateConsentRequest
{
    /// <summary>
    /// Type of consent: "TermsOfService" or "PrivacyPolicy"
    /// </summary>
    [Required]
    public required string ConsentType { get; set; }

    /// <summary>
    /// Whether the user accepts this consent
    /// </summary>
    [Required]
    public bool Accepted { get; set; }

    /// <summary>
    /// Version of the document being accepted
    /// </summary>
    [Required]
    [MaxLength(20)]
    public required string Version { get; set; }
}

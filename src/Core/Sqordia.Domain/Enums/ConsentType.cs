namespace Sqordia.Domain.Enums;

/// <summary>
/// Types of consent that users can provide (Quebec Bill 25 compliance)
/// </summary>
public enum ConsentType
{
    /// <summary>
    /// Terms of Service acceptance
    /// </summary>
    TermsOfService = 0,

    /// <summary>
    /// Privacy Policy acceptance
    /// </summary>
    PrivacyPolicy = 1
}

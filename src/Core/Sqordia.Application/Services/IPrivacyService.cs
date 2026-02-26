using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.Privacy;
using Sqordia.Contracts.Responses.Privacy;

namespace Sqordia.Application.Services;

/// <summary>
/// Service interface for privacy-related operations (Quebec Bill 25 compliance)
/// </summary>
public interface IPrivacyService
{
    /// <summary>
    /// Export user's personal data in a machine-readable format (data portability)
    /// </summary>
    Task<Result<UserDataExportResponse>> ExportUserDataAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete user account (soft deactivation or permanent hard delete)
    /// </summary>
    Task<Result<AccountDeletionResponse>> DeleteAccountAsync(AccountDeletionRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get current consent status for all consent types
    /// </summary>
    Task<Result<ConsentStatusResponse>> GetConsentsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Update consent (accept or withdraw consent for ToS/Privacy Policy)
    /// </summary>
    Task<Result<ConsentStatusResponse>> UpdateConsentAsync(UpdateConsentRequest request, CancellationToken cancellationToken = default);
}

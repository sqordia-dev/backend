using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.V2.Share;
using Sqordia.Contracts.Responses.V2.Share;

namespace Sqordia.Application.Services.V2;

/// <summary>
/// Secure vault share service for business plans
/// Provides enhanced security features like watermarking, view tracking, and password protection
/// </summary>
public interface IVaultShareService
{
    /// <summary>
    /// Creates a secure vault share for a business plan
    /// </summary>
    Task<Result<VaultShareResponse>> CreateVaultShareAsync(
        Guid businessPlanId,
        CreateVaultShareRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets vault share details by share ID
    /// </summary>
    Task<Result<VaultShareResponse>> GetVaultShareAsync(
        Guid shareId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all vault shares for a business plan
    /// </summary>
    Task<Result<IEnumerable<VaultShareResponse>>> GetVaultSharesAsync(
        Guid businessPlanId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets analytics for a vault share
    /// </summary>
    Task<Result<VaultShareAnalyticsResponse>> GetVaultShareAnalyticsAsync(
        Guid shareId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a view of the shared document
    /// </summary>
    Task<Result<bool>> RecordViewAsync(
        string token,
        string? viewerEmail = null,
        string? viewerName = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates password for a password-protected share
    /// </summary>
    Task<Result<bool>> ValidatePasswordAsync(
        string token,
        string password,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes a vault share
    /// </summary>
    Task<Result<bool>> RevokeVaultShareAsync(
        Guid shareId,
        CancellationToken cancellationToken = default);
}

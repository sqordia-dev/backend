using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.TwoFactor;
using Sqordia.Contracts.Responses.TwoFactor;

namespace Sqordia.Application.Services;

public interface ITwoFactorService
{
    // Setup and Configuration
    Task<Result<TwoFactorSetupResponse>> SetupTwoFactorAsync(CancellationToken cancellationToken = default);
    Task<Result<BackupCodesResponse>> EnableTwoFactorAsync(EnableTwoFactorRequest request, CancellationToken cancellationToken = default);
    Task<Result> DisableTwoFactorAsync(CancellationToken cancellationToken = default);
    
    // Status and Info
    Task<Result<TwoFactorStatusResponse>> GetTwoFactorStatusAsync(CancellationToken cancellationToken = default);
    
    // Backup Codes
    Task<Result<BackupCodesResponse>> RegenerateBackupCodesAsync(CancellationToken cancellationToken = default);
    
    // Verification
    Task<Result> VerifyTwoFactorCodeAsync(VerifyTwoFactorRequest request, CancellationToken cancellationToken = default);
}


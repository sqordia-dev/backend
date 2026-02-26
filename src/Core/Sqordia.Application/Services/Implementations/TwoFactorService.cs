using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.TwoFactor;
using Sqordia.Contracts.Responses.TwoFactor;
using Sqordia.Domain.Entities.Identity;

namespace Sqordia.Application.Services.Implementations;

public class TwoFactorService : ITwoFactorService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITotpService _totpService;
    private readonly ILogger<TwoFactorService> _logger;
    private readonly ILocalizationService _localizationService;

    public TwoFactorService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ITotpService totpService,
        ILogger<TwoFactorService> logger,
        ILocalizationService localizationService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _totpService = totpService;
        _logger = logger;
        _localizationService = localizationService;
    }

    public async Task<Result<TwoFactorSetupResponse>> SetupTwoFactorAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _currentUserService.GetUserIdAsGuid();
            if (userId == null)
            {
                return Result.Failure<TwoFactorSetupResponse>(Error.Unauthorized("General.Unauthorized", _localizationService.GetString("General.Unauthorized")));
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId.Value && !u.IsDeleted, cancellationToken);

            if (user == null)
            {
                return Result.Failure<TwoFactorSetupResponse>(Error.NotFound("Auth.Error.UserNotFound", _localizationService.GetString("Auth.Error.UserNotFound")));
            }

            // Check if 2FA already exists
            var existingTwoFactor = await _context.TwoFactorAuths
                .FirstOrDefaultAsync(t => t.UserId == userId.Value, cancellationToken);

            string secretKey;
            if (existingTwoFactor != null)
            {
                // If already setup but not enabled, return existing secret
                if (!existingTwoFactor.IsEnabled)
                {
                    secretKey = existingTwoFactor.SecretKey;
                }
                else
                {
                    // Generate new secret for re-setup
                    secretKey = _totpService.GenerateSecretKey();
                    existingTwoFactor.UpdateSecretKey(secretKey);
                }
            }
            else
            {
                // Create new 2FA setup
                secretKey = _totpService.GenerateSecretKey();
                var twoFactor = new TwoFactorAuth(userId.Value, secretKey);
                _context.TwoFactorAuths.Add(twoFactor);
            }

            await _context.SaveChangesAsync(cancellationToken);

            var response = new TwoFactorSetupResponse
            {
                SecretKey = secretKey,
                QrCodeUrl = _totpService.GenerateQrCodeUrl(user.Email.Value, secretKey),
                ManualEntryKey = _totpService.FormatSecretKeyForManualEntry(secretKey)
            };

            _logger.LogInformation("2FA setup initiated for user {UserId}", userId);

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting up 2FA");
            return Result.Failure<TwoFactorSetupResponse>(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    public async Task<Result<BackupCodesResponse>> EnableTwoFactorAsync(EnableTwoFactorRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _currentUserService.GetUserIdAsGuid();
            if (userId == null)
            {
                return Result.Failure<BackupCodesResponse>(Error.Unauthorized("General.Unauthorized", _localizationService.GetString("General.Unauthorized")));
            }

            var twoFactor = await _context.TwoFactorAuths
                .FirstOrDefaultAsync(t => t.UserId == userId.Value, cancellationToken);

            if (twoFactor == null)
            {
                return Result.Failure<BackupCodesResponse>(Error.NotFound("TwoFactor.Error.NotSetup", _localizationService.GetString("TwoFactor.Error.NotSetup")));
            }

            if (twoFactor.IsEnabled)
            {
                return Result.Failure<BackupCodesResponse>(Error.Conflict("TwoFactor.Error.AlreadyEnabled", _localizationService.GetString("TwoFactor.Error.AlreadyEnabled")));
            }

            // Verify the code
            if (!_totpService.VerifyCode(twoFactor.SecretKey, request.VerificationCode))
            {
                twoFactor.RecordFailedAttempt();
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogWarning("Failed 2FA enablement attempt for user {UserId}", userId);
                return Result.Failure<BackupCodesResponse>(Error.Validation("TwoFactor.Error.InvalidCode", _localizationService.GetString("TwoFactor.Error.InvalidCode")));
            }

            // Generate backup codes
            var backupCodes = _totpService.GenerateBackupCodes();
            var backupCodesJson = JsonConvert.SerializeObject(backupCodes);

            // Enable 2FA
            twoFactor.Enable(backupCodesJson);
            twoFactor.ResetFailedAttempts();
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("2FA enabled successfully for user {UserId}", userId);

            return Result.Success(new BackupCodesResponse { BackupCodes = backupCodes });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling 2FA");
            return Result.Failure<BackupCodesResponse>(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    public async Task<Result> DisableTwoFactorAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _currentUserService.GetUserIdAsGuid();
            if (userId == null)
            {
                return Result.Failure(Error.Unauthorized("General.Unauthorized", _localizationService.GetString("General.Unauthorized")));
            }

            var twoFactor = await _context.TwoFactorAuths
                .FirstOrDefaultAsync(t => t.UserId == userId.Value, cancellationToken);

            if (twoFactor == null)
            {
                return Result.Failure(Error.NotFound("TwoFactor.Error.NotSetup", _localizationService.GetString("TwoFactor.Error.NotSetup")));
            }

            if (!twoFactor.IsEnabled)
            {
                return Result.Failure(Error.Conflict("TwoFactor.Error.NotEnabled", _localizationService.GetString("TwoFactor.Error.NotEnabled")));
            }

            twoFactor.Disable();
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("2FA disabled for user {UserId}", userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling 2FA");
            return Result.Failure(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    public async Task<Result<TwoFactorStatusResponse>> GetTwoFactorStatusAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _currentUserService.GetUserIdAsGuid();
            if (userId == null)
            {
                return Result.Failure<TwoFactorStatusResponse>(Error.Unauthorized("General.Unauthorized", _localizationService.GetString("General.Unauthorized")));
            }

            var twoFactor = await _context.TwoFactorAuths
                .FirstOrDefaultAsync(t => t.UserId == userId.Value, cancellationToken);

            if (twoFactor == null)
            {
                return Result.Success(new TwoFactorStatusResponse
                {
                    IsEnabled = false,
                    EnabledAt = null,
                    RemainingBackupCodes = 0
                });
            }

            var remainingBackupCodes = 0;
            if (!string.IsNullOrEmpty(twoFactor.BackupCodes))
            {
                try
                {
                    var backupCodes = JsonConvert.DeserializeObject<List<string>>(twoFactor.BackupCodes);
                    remainingBackupCodes = backupCodes?.Count ?? 0;
                }
                catch
                {
                    remainingBackupCodes = 0;
                }
            }

            return Result.Success(new TwoFactorStatusResponse
            {
                IsEnabled = twoFactor.IsEnabled,
                EnabledAt = twoFactor.EnabledAt,
                RemainingBackupCodes = remainingBackupCodes
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting 2FA status");
            return Result.Failure<TwoFactorStatusResponse>(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    public async Task<Result<BackupCodesResponse>> RegenerateBackupCodesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _currentUserService.GetUserIdAsGuid();
            if (userId == null)
            {
                return Result.Failure<BackupCodesResponse>(Error.Unauthorized("General.Unauthorized", _localizationService.GetString("General.Unauthorized")));
            }

            var twoFactor = await _context.TwoFactorAuths
                .FirstOrDefaultAsync(t => t.UserId == userId.Value, cancellationToken);

            if (twoFactor == null || !twoFactor.IsEnabled)
            {
                return Result.Failure<BackupCodesResponse>(Error.NotFound("TwoFactor.Error.NotEnabled", _localizationService.GetString("TwoFactor.Error.NotEnabled")));
            }

            // Generate new backup codes
            var backupCodes = _totpService.GenerateBackupCodes();
            var backupCodesJson = JsonConvert.SerializeObject(backupCodes);

            twoFactor.RegenerateBackupCodes(backupCodesJson);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Backup codes regenerated for user {UserId}", userId);

            return Result.Success(new BackupCodesResponse { BackupCodes = backupCodes });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error regenerating backup codes");
            return Result.Failure<BackupCodesResponse>(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    public async Task<Result> VerifyTwoFactorCodeAsync(VerifyTwoFactorRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _currentUserService.GetUserIdAsGuid();
            if (userId == null)
            {
                return Result.Failure(Error.Unauthorized("General.Unauthorized", _localizationService.GetString("General.Unauthorized")));
            }

            var twoFactor = await _context.TwoFactorAuths
                .FirstOrDefaultAsync(t => t.UserId == userId.Value, cancellationToken);

            if (twoFactor == null || !twoFactor.IsEnabled)
            {
                return Result.Failure(Error.NotFound("TwoFactor.Error.NotEnabled", _localizationService.GetString("TwoFactor.Error.NotEnabled")));
            }

            // Check if account is locked due to failed attempts
            if (twoFactor.FailedAttempts >= 5)
            {
                var lockoutTime = twoFactor.LastAttemptAt?.AddMinutes(15);
                if (lockoutTime > DateTime.UtcNow)
                {
                    return Result.Failure(Error.Forbidden("TwoFactor.Error.LockedOut", _localizationService.GetString("TwoFactor.Error.LockedOut")));
                }
                else
                {
                    twoFactor.ResetFailedAttempts();
                }
            }

            // Try to verify with TOTP code
            if (_totpService.VerifyCode(twoFactor.SecretKey, request.Code))
            {
                twoFactor.ResetFailedAttempts();
                await _context.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }

            // Try to verify with backup code
            if (!string.IsNullOrEmpty(twoFactor.BackupCodes))
            {
                try
                {
                    var backupCodes = JsonConvert.DeserializeObject<List<string>>(twoFactor.BackupCodes);
                    if (backupCodes != null && backupCodes.Contains(request.Code))
                    {
                        // Remove used backup code
                        backupCodes.Remove(request.Code);
                        twoFactor.RegenerateBackupCodes(JsonConvert.SerializeObject(backupCodes));
                        twoFactor.ResetFailedAttempts();
                        await _context.SaveChangesAsync(cancellationToken);

                        _logger.LogInformation("Backup code used for user {UserId}. Remaining codes: {RemainingCodes}", userId, backupCodes.Count);
                        return Result.Success();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error verifying backup code");
                }
            }

            // Invalid code
            twoFactor.RecordFailedAttempt();
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogWarning("Failed 2FA verification attempt for user {UserId}. Failed attempts: {FailedAttempts}", userId, twoFactor.FailedAttempts);

            return Result.Failure(Error.Validation("TwoFactor.Error.InvalidCode", _localizationService.GetString("TwoFactor.Error.InvalidCode")));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying 2FA code");
            return Result.Failure(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

}


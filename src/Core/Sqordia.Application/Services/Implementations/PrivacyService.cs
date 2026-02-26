using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Common.Security;
using Sqordia.Contracts.Requests.Privacy;
using Sqordia.Contracts.Responses.Privacy;
using Sqordia.Domain.Entities.Identity;
using Sqordia.Domain.Enums;

namespace Sqordia.Application.Services.Implementations;

/// <summary>
/// Service for privacy-related operations (Quebec Bill 25 compliance)
/// </summary>
public class PrivacyService : IPrivacyService
{
    private readonly IApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ISecurityService _securityService;
    private readonly ILogger<PrivacyService> _logger;
    private readonly ILocalizationService _localizationService;

    // Current policy versions - should be configured in appsettings or database
    private const string CurrentTosVersion = "1.0";
    private const string CurrentPrivacyVersion = "1.0";

    public PrivacyService(
        IApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        ISecurityService securityService,
        ILogger<PrivacyService> logger,
        ILocalizationService localizationService)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _securityService = securityService;
        _logger = logger;
        _localizationService = localizationService;
    }

    public async Task<Result<UserDataExportResponse>> ExportUserDataAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Result.Failure<UserDataExportResponse>(
                    Error.Unauthorized("General.Unauthorized", _localizationService.GetString("General.Unauthorized")));
            }

            var user = await _context.Users
                .Include(u => u.Consents)
                .FirstOrDefaultAsync(u => u.Id == userId.Value && !u.IsDeleted, cancellationToken);

            if (user == null)
            {
                return Result.Failure<UserDataExportResponse>(
                    Error.NotFound("Auth.Error.UserNotFound", _localizationService.GetString("Auth.Error.UserNotFound")));
            }

            var response = new UserDataExportResponse
            {
                Metadata = new ExportMetadata
                {
                    ExportedAt = DateTime.UtcNow,
                    ExportVersion = "1.0",
                    RequestedBy = user.Email.Value
                },
                Profile = new ProfileData
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email.Value,
                    PhoneNumber = user.PhoneNumber,
                    Persona = user.Persona?.ToString(),
                    CreatedAt = user.Created,
                    LastModifiedAt = user.LastModified
                },
                Consents = user.Consents
                    .Where(c => !c.IsDeleted)
                    .Select(c => new ConsentRecord
                    {
                        Type = c.Type.ToString(),
                        Version = c.Version,
                        IsAccepted = c.IsAccepted,
                        AcceptedAt = c.AcceptedAt
                    }).ToList()
            };

            _logger.LogInformation("User {UserId} exported their personal data", userId);
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting user data");
            return Result.Failure<UserDataExportResponse>(
                Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    public async Task<Result<AccountDeletionResponse>> DeleteAccountAsync(AccountDeletionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Result.Failure<AccountDeletionResponse>(
                    Error.Unauthorized("General.Unauthorized", _localizationService.GetString("General.Unauthorized")));
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId.Value && !u.IsDeleted, cancellationToken);

            if (user == null)
            {
                return Result.Failure<AccountDeletionResponse>(
                    Error.NotFound("Auth.Error.UserNotFound", _localizationService.GetString("Auth.Error.UserNotFound")));
            }

            // Verify password
            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                // OAuth users - check if they have a password set
                return Result.Failure<AccountDeletionResponse>(
                    Error.Validation("Privacy.NoPassword", "OAuth users must set a password before deleting their account"));
            }

            if (!_securityService.VerifyPassword(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Failed account deletion attempt for user {UserId} - incorrect password", userId);
                return Result.Failure<AccountDeletionResponse>(
                    Error.Validation("Privacy.InvalidPassword", _localizationService.GetString("Profile.Error.InvalidCurrentPassword")));
            }

            // Parse deletion type
            if (!Enum.TryParse<AccountDeletionType>(request.DeletionType, true, out var deletionType))
            {
                return Result.Failure<AccountDeletionResponse>(
                    Error.Validation("Privacy.InvalidDeletionType", "Deletion type must be 'Deactivate' or 'Permanent'"));
            }

            // Revoke all refresh tokens
            var refreshTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == user.Id && rt.RevokedAt == null)
                .ToListAsync(cancellationToken);

            foreach (var token in refreshTokens)
            {
                token.Revoke("AccountDeletion");
            }

            var response = new AccountDeletionResponse
            {
                Success = true,
                DeletionType = request.DeletionType
            };

            if (deletionType == AccountDeletionType.Deactivate)
            {
                // Soft delete - account can be recovered within 30 days
                user.Deactivate();
                response.Message = "Your account has been deactivated. You can reactivate within 30 days by contacting support.";
                response.ReactivationDeadline = DateTime.UtcNow.AddDays(30);
                _logger.LogInformation("User {UserId} deactivated their account", userId);
            }
            else // Permanent
            {
                // Hard delete - remove all user data permanently
                await HardDeleteUserAsync(user, cancellationToken);
                response.Message = "Your account and all associated data have been permanently deleted.";
                _logger.LogInformation("User {UserId} permanently deleted their account", userId);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user account");
            return Result.Failure<AccountDeletionResponse>(
                Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    private async Task HardDeleteUserAsync(User user, CancellationToken cancellationToken)
    {
        // Remove consents
        var consents = await _context.UserConsents
            .Where(c => c.UserId == user.Id)
            .ToListAsync(cancellationToken);
        _context.UserConsents.RemoveRange(consents);

        // Remove refresh tokens
        var tokens = await _context.RefreshTokens
            .Where(t => t.UserId == user.Id)
            .ToListAsync(cancellationToken);
        _context.RefreshTokens.RemoveRange(tokens);

        // Remove active sessions
        var sessions = await _context.ActiveSessions
            .Where(s => s.UserId == user.Id)
            .ToListAsync(cancellationToken);
        _context.ActiveSessions.RemoveRange(sessions);

        // Remove login history
        var loginHistory = await _context.LoginHistories
            .Where(l => l.UserId == user.Id)
            .ToListAsync(cancellationToken);
        _context.LoginHistories.RemoveRange(loginHistory);

        // Remove 2FA settings
        var twoFactorAuth = await _context.TwoFactorAuths
            .Where(t => t.UserId == user.Id)
            .ToListAsync(cancellationToken);
        _context.TwoFactorAuths.RemoveRange(twoFactorAuth);

        // Remove email verification tokens
        var emailTokens = await _context.EmailVerificationTokens
            .Where(e => e.UserId == user.Id)
            .ToListAsync(cancellationToken);
        _context.EmailVerificationTokens.RemoveRange(emailTokens);

        // Remove password reset tokens
        var passwordTokens = await _context.PasswordResetTokens
            .Where(p => p.UserId == user.Id)
            .ToListAsync(cancellationToken);
        _context.PasswordResetTokens.RemoveRange(passwordTokens);

        // Remove user roles
        var userRoles = await _context.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .ToListAsync(cancellationToken);
        _context.UserRoles.RemoveRange(userRoles);

        // Finally, remove the user
        _context.Users.Remove(user);
    }

    public async Task<Result<ConsentStatusResponse>> GetConsentsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Result.Failure<ConsentStatusResponse>(
                    Error.Unauthorized("General.Unauthorized", _localizationService.GetString("General.Unauthorized")));
            }

            var consents = await _context.UserConsents
                .Where(c => c.UserId == userId.Value && !c.IsDeleted)
                .ToListAsync(cancellationToken);

            var response = new ConsentStatusResponse
            {
                Consents = new List<ConsentItem>
                {
                    BuildConsentItem(ConsentType.TermsOfService, consents, CurrentTosVersion),
                    BuildConsentItem(ConsentType.PrivacyPolicy, consents, CurrentPrivacyVersion)
                }
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user consents");
            return Result.Failure<ConsentStatusResponse>(
                Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    public async Task<Result<ConsentStatusResponse>> UpdateConsentAsync(UpdateConsentRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Result.Failure<ConsentStatusResponse>(
                    Error.Unauthorized("General.Unauthorized", _localizationService.GetString("General.Unauthorized")));
            }

            if (!Enum.TryParse<ConsentType>(request.ConsentType, true, out var consentType))
            {
                return Result.Failure<ConsentStatusResponse>(
                    Error.Validation("Privacy.InvalidConsentType", "Consent type must be 'TermsOfService' or 'PrivacyPolicy'"));
            }

            var httpContext = _httpContextAccessor.HttpContext;
            var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString();
            var userAgent = httpContext?.Request.Headers["User-Agent"].ToString();

            // Create new consent record (we keep history of all acceptances)
            var consent = new UserConsent(userId.Value, consentType, request.Version, ipAddress, userAgent);
            _context.UserConsents.Add(consent);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User {UserId} accepted consent for {ConsentType} version {Version}",
                userId, consentType, request.Version);

            // Return updated consent status
            return await GetConsentsAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user consent");
            return Result.Failure<ConsentStatusResponse>(
                Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    private static ConsentItem BuildConsentItem(ConsentType type, List<UserConsent> consents, string currentVersion)
    {
        var latest = consents
            .Where(c => c.Type == type && c.IsAccepted)
            .OrderByDescending(c => c.AcceptedAt)
            .FirstOrDefault();

        return new ConsentItem
        {
            Type = type.ToString(),
            Version = latest?.Version ?? string.Empty,
            IsAccepted = latest?.IsAccepted ?? false,
            AcceptedAt = latest?.AcceptedAt,
            RequiresUpdate = latest == null || latest.Version != currentVersion,
            LatestVersion = currentVersion
        };
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?
            .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}

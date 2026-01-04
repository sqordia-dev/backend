using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Common.Security;
using Sqordia.Contracts.Requests.User;
using Sqordia.Contracts.Responses.User;

namespace Sqordia.Application.Services.Implementations;

public class UserProfileService : IUserProfileService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ISecurityService _securityService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<UserProfileService> _logger;
    private readonly ILocalizationService _localizationService;

    public UserProfileService(
        IApplicationDbContext context,
        IMapper mapper,
        ISecurityService securityService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<UserProfileService> logger,
        ILocalizationService localizationService)
    {
        _context = context;
        _mapper = mapper;
        _securityService = securityService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _localizationService = localizationService;
    }

    public async Task<Result<UserProfileResponse>> GetProfileAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Result.Failure<UserProfileResponse>(Error.Unauthorized("General.Unauthorized", _localizationService.GetString("General.Unauthorized")));
            }

            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userGuid && !u.IsDeleted, cancellationToken);

            if (user == null)
            {
                return Result.Failure<UserProfileResponse>(Error.NotFound("Auth.Error.UserNotFound", _localizationService.GetString("Auth.Error.UserNotFound")));
            }

            var response = new UserProfileResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email.Value,
                UserName = user.UserName,
                UserType = user.UserType.ToString(),
                PhoneNumber = user.PhoneNumber,
                ProfilePictureUrl = user.ProfilePictureUrl,
                EmailVerified = user.IsEmailConfirmed,
                PhoneNumberVerified = user.PhoneNumberVerified,
                CreatedAt = user.Created,
                LastModifiedAt = user.LastModified,
                Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList()
            };

            return Result<UserProfileResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user profile");
            return Result.Failure<UserProfileResponse>(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    public async Task<Result<UserProfileResponse>> UpdateProfileAsync(UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Result.Failure<UserProfileResponse>(Error.Unauthorized("General.Unauthorized", _localizationService.GetString("General.Unauthorized")));
            }

            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userGuid && !u.IsDeleted, cancellationToken);

            if (user == null)
            {
                return Result.Failure<UserProfileResponse>(Error.NotFound("Auth.Error.UserNotFound", _localizationService.GetString("Auth.Error.UserNotFound")));
            }

            // Check if username is being changed and if it's already taken
            if (!string.IsNullOrEmpty(request.UserName) && request.UserName != user.UserName)
            {
                var usernameExists = await _context.Users
                    .AnyAsync(u => u.UserName == request.UserName && u.Id != user.Id && !u.IsDeleted, cancellationToken);

                if (usernameExists)
                {
                    return Result.Failure<UserProfileResponse>(Error.Conflict("Profile.Error.UserNameTaken", _localizationService.GetString("Profile.Error.UserNameTaken")));
                }
            }

            // Update user properties using domain methods
            user.UpdateProfile(request.FirstName, request.LastName, request.UserName);
            user.UpdatePhoneNumber(request.PhoneNumber);
            user.UpdateProfilePicture(request.ProfilePictureUrl);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User profile updated successfully for user {UserId}", userId);

            var response = new UserProfileResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email.Value,
                UserName = user.UserName,
                PhoneNumber = user.PhoneNumber,
                ProfilePictureUrl = user.ProfilePictureUrl,
                EmailVerified = user.IsEmailConfirmed,
                PhoneNumberVerified = user.PhoneNumberVerified,
                CreatedAt = user.Created,
                LastModifiedAt = user.LastModified,
                Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList()
            };

            return Result<UserProfileResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile");
            return Result.Failure<UserProfileResponse>(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    public async Task<Result> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Result.Failure(Error.Unauthorized("General.Unauthorized", _localizationService.GetString("General.Unauthorized")));
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userGuid && !u.IsDeleted, cancellationToken);

            if (user == null)
            {
                return Result.Failure(Error.NotFound("Auth.Error.UserNotFound", _localizationService.GetString("Auth.Error.UserNotFound")));
            }

            // Verify current password
            if (!_securityService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            {
                _logger.LogWarning("Failed password change attempt for user {UserId} - incorrect current password", userId);
                return Result.Failure(Error.Validation("Profile.Error.InvalidCurrentPassword", _localizationService.GetString("Profile.Error.InvalidCurrentPassword")));
            }

            // Hash new password and update using domain method
            var newPasswordHash = _securityService.HashPassword(request.NewPassword);
            user.UpdatePassword(newPasswordHash);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Password changed successfully for user {UserId}", userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password");
            return Result.Failure(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    public async Task<Result> DeleteAccountAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Result.Failure(Error.Unauthorized("General.Unauthorized", _localizationService.GetString("General.Unauthorized")));
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userGuid && !u.IsDeleted, cancellationToken);

            if (user == null)
            {
                return Result.Failure(Error.NotFound("Auth.Error.UserNotFound", _localizationService.GetString("Auth.Error.UserNotFound")));
            }

            // Soft delete the user using domain method
            user.SoftDelete();

            // Revoke all refresh tokens
            var refreshTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == user.Id && rt.RevokedAt == null)
                .ToListAsync(cancellationToken);

            foreach (var token in refreshTokens)
            {
                token.Revoke("System");
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User account deleted successfully for user {UserId}", userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user account");
            return Result.Failure(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }
}


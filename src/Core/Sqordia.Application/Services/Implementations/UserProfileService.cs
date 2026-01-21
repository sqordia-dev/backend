using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Common.Security;
using Sqordia.Contracts.Requests.User;
using Sqordia.Contracts.Responses.User;
using Sqordia.Domain.Enums;

namespace Sqordia.Application.Services.Implementations;

public class UserProfileService : IUserProfileService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ISecurityService _securityService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<UserProfileService> _logger;
    private readonly ILocalizationService _localizationService;
    private readonly IStorageService _storageService;

    public UserProfileService(
        IApplicationDbContext context,
        IMapper mapper,
        ISecurityService securityService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<UserProfileService> logger,
        ILocalizationService localizationService,
        IStorageService storageService)
    {
        _context = context;
        _mapper = mapper;
        _securityService = securityService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _localizationService = localizationService;
        _storageService = storageService;
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
                Persona = user.Persona?.ToString(),
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
                UserType = user.UserType.ToString(),
                Persona = user.Persona?.ToString(),
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

    public async Task<Result<string>> UploadProfilePictureAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Result.Failure<string>(Error.Unauthorized("General.Unauthorized", _localizationService.GetString("General.Unauthorized")));
            }

            // Validate file
            if (file == null || file.Length == 0)
            {
                return Result.Failure<string>(Error.Validation("Profile.Error.InvalidFile", _localizationService.GetString("Profile.Error.InvalidFile")));
            }

            // Validate file size (max 5MB)
            const long maxFileSize = 5 * 1024 * 1024; // 5MB
            if (file.Length > maxFileSize)
            {
                return Result.Failure<string>(Error.Validation("Profile.Error.FileTooLarge", _localizationService.GetString("Profile.Error.FileTooLarge")));
            }

            // Validate file type (images only)
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return Result.Failure<string>(Error.Validation("Profile.Error.InvalidFileType", _localizationService.GetString("Profile.Error.InvalidFileType")));
            }

            // Generate unique file key
            var fileKey = $"profile-pictures/{userGuid}/{Guid.NewGuid()}{fileExtension}";

            // Upload file to storage
            using var fileStream = file.OpenReadStream();
            var contentType = file.ContentType ?? "image/jpeg";
            var fileUrl = await _storageService.UploadFileAsync(fileKey, fileStream, contentType, cancellationToken);

            _logger.LogInformation("Profile picture uploaded successfully for user {UserId}: {FileUrl}", userId, fileUrl);

            return Result<string>.Success(fileUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading profile picture");
            return Result.Failure<string>(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
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

    public async Task<Result<UserProfileResponse>> SetPersonaAsync(SetPersonaRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Result.Failure<UserProfileResponse>(Error.Unauthorized("General.Unauthorized", _localizationService.GetString("General.Unauthorized")));
            }

            if (!Enum.TryParse<PersonaType>(request.Persona, true, out var personaType))
            {
                return Result.Failure<UserProfileResponse>(Error.Validation("User.InvalidPersona", $"Invalid persona type: {request.Persona}. Valid values: Entrepreneur, Consultant, OBNL"));
            }

            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userGuid && !u.IsDeleted, cancellationToken);

            if (user == null)
            {
                return Result.Failure<UserProfileResponse>(Error.NotFound("Auth.Error.UserNotFound", _localizationService.GetString("Auth.Error.UserNotFound")));
            }

            user.SetPersona(personaType);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User persona set to {Persona} for user {UserId}", personaType, userId);

            var response = new UserProfileResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email.Value,
                UserName = user.UserName,
                UserType = user.UserType.ToString(),
                Persona = user.Persona?.ToString(),
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
            _logger.LogError(ex, "Error setting user persona");
            return Result.Failure<UserProfileResponse>(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }
}


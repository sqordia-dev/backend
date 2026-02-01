using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Services;
using Sqordia.Contracts.Requests.Auth;
using Sqordia.Contracts.Responses.Auth;
using UserDto = Sqordia.Contracts.Responses.Auth.UserDto;
using Sqordia.Domain.Entities.Identity;
using Sqordia.Domain.ValueObjects;
using Sqordia.Application.Common.Security;

namespace Sqordia.Application.Services.Implementations;

public class AuthenticationService : IAuthenticationService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IEmailService _emailService;
    private readonly ISecurityService _securityService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly ILocalizationService _localizationService;

    public AuthenticationService(
        IApplicationDbContext context,
        IMapper mapper,
        IJwtTokenService jwtTokenService,
        IEmailService emailService,
        ISecurityService securityService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuthenticationService> logger,
        ILocalizationService localizationService)
    {
        _context = context;
        _mapper = mapper;
        _jwtTokenService = jwtTokenService;
        _emailService = emailService;
        _securityService = securityService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _localizationService = localizationService;
    }

    private string GetClientIpAddress()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Connection?.RemoteIpAddress != null)
        {
            return httpContext.Connection.RemoteIpAddress.ToString();
        }
        return "Unknown";
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if user already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.Value == request.Email, cancellationToken);

            if (existingUser != null)
            {
                return Result.Failure<AuthResponse>(Error.Conflict("Auth.Error.EmailAlreadyExists", _localizationService.GetString("Auth.Error.EmailAlreadyExists")));
            }

            // Parse user type
            if (!Enum.TryParse<Domain.Enums.UserType>(request.UserType, out var userType))
            {
                return Result.Failure<AuthResponse>(Error.Validation("Validation.Required", _localizationService.GetString("Validation.Required")));
            }

            // Create user with proper password hashing
            var email = new EmailAddress(request.Email);
            var user = new User(request.FirstName, request.LastName, email, request.Email, userType);
            
            // Hash password using BCrypt
            var passwordHash = _securityService.HashPassword(request.Password);
            user.SetPasswordHash(passwordHash);

            _context.Users.Add(user);

            // Generate JWT token and refresh token
            var accessToken = await _jwtTokenService.GenerateAccessTokenAsync(user);
            var refreshToken = await _jwtTokenService.GenerateRefreshTokenAsync(user.Id, GetClientIpAddress());

            // Generate email verification token
            var verificationToken = _securityService.GenerateSecureToken();
            var emailVerificationToken = new EmailVerificationToken(user.Id, verificationToken, DateTime.UtcNow.AddHours(24));
            _context.EmailVerificationTokens.Add(emailVerificationToken);

            // Single save: User + RefreshToken + EmailVerificationToken
            await _context.SaveChangesAsync(cancellationToken);

            // Send combined welcome and verification email
            try
            {
                _logger.LogInformation("Sending welcome and verification email to {Email}", request.Email);
                
                // Send single email with welcome message and verification link
                await _emailService.SendWelcomeWithVerificationAsync(request.Email, request.FirstName, request.LastName, user.UserName, verificationToken);
                _logger.LogInformation("Welcome and verification email sent successfully to {Email}", request.Email);
            }
            catch (Exception emailEx)
            {
                // Log email failure but don't fail registration
                _logger.LogError(emailEx, "Failed to send email to {Email}. User registration completed but email not sent.", request.Email);
                // In production, you might want to queue this for retry
            }

            var response = new AuthResponse
            {
                Token = accessToken,
                RefreshToken = refreshToken.Token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60), // JWT expiration
                User = _mapper.Map<UserDto>(user)
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            return Result.Failure<AuthResponse>(Error.Failure("Authentication.Register.Failed", $"Registration failed: {ex.Message}"));
        }
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var ipAddress = GetClientIpAddress();
        var userAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();
        
        try
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email.Value == request.Email, cancellationToken);

            if (user == null)
            {
                // Log failed attempt for non-existent user (for security monitoring)
                _logger.LogWarning("Login attempt for non-existent email: {Email} from IP: {IpAddress}", request.Email, ipAddress);
                return Result.Failure<AuthResponse>(Error.NotFound("Auth.Error.InvalidCredentials", _localizationService.GetString("Auth.Error.InvalidCredentials")));
            }

            // Check if account is locked
            if (user.IsLockedOut)
            {
                _logger.LogWarning("Login attempt for locked account: {UserId} from IP: {IpAddress}", user.Id, ipAddress);
                
                // Record failed login attempt
                await RecordLoginAttempt(user.Id, false, ipAddress, userAgent, "Account is locked", cancellationToken);
                
                var lockoutMinutesRemaining = (int)Math.Ceiling((user.LockoutEnd!.Value - DateTime.UtcNow).TotalMinutes);
                return Result.Failure<AuthResponse>(Error.Unauthorized(
                    "Auth.Error.AccountLocked", 
                    _localizationService.GetString("Auth.Error.AccountLocked")));
            }

            // Verify password using BCrypt
            if (!_securityService.VerifyPassword(request.Password, user.PasswordHash))
            {
                // Record failed login attempt
                user.RecordFailedLogin();
                
                // Check if we need to lock the account (5 failed attempts)
                const int maxFailedAttempts = 5;
                if (user.AccessFailedCount >= maxFailedAttempts)
                {
                    var lockoutDuration = TimeSpan.FromMinutes(30);
                    user.LockAccount(DateTime.UtcNow.Add(lockoutDuration));
                    
                    _logger.LogWarning("Account locked due to failed login attempts: {UserId}", user.Id);
                    
                    // Send account lockout email
                    try
                    {
                        await _emailService.SendAccountLockoutNotificationAsync(user.Email.Value, user.FirstName, lockoutDuration, DateTime.UtcNow);
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogError(emailEx, "Failed to send account lockout email to {Email}", user.Email.Value);
                    }
                }
                
                await _context.SaveChangesAsync(cancellationToken);
                await RecordLoginAttempt(user.Id, false, ipAddress, userAgent, "Invalid password", cancellationToken);
                
                return Result.Failure<AuthResponse>(Error.Unauthorized("Auth.Error.InvalidCredentials", _localizationService.GetString("Auth.Error.InvalidCredentials")));
            }

            // Check if user is active
            if (!user.IsActive)
            {
                await RecordLoginAttempt(user.Id, false, ipAddress, userAgent, "Account is disabled", cancellationToken);
                return Result.Failure<AuthResponse>(Error.Unauthorized("Auth.Error.EmailNotVerified", _localizationService.GetString("Auth.Error.EmailNotVerified")));
            }

            // Check if password change is required
            if (user.RequirePasswordChange)
            {
                await RecordLoginAttempt(user.Id, false, ipAddress, userAgent, "Password change required", cancellationToken);
                return Result.Failure<AuthResponse>(Error.Unauthorized(
                    "Authentication.PasswordChangeRequired", 
                    "You must change your password before logging in. Please use the password reset functionality."));
            }

            // Check if password has expired (90 days policy)
            const int passwordExpiryDays = 90;
            if (user.IsPasswordExpired(passwordExpiryDays))
            {
                user.ForcePasswordChange();
                await _context.SaveChangesAsync(cancellationToken);
                await RecordLoginAttempt(user.Id, false, ipAddress, userAgent, "Password expired", cancellationToken);
                
                return Result.Failure<AuthResponse>(Error.Unauthorized(
                    "Authentication.PasswordExpired", 
                    $"Your password has expired. Passwords must be changed every {passwordExpiryDays} days. Please use the password reset functionality."));
            }

            // Successful login - reset failed attempts
            user.ResetAccessFailedCount();
            user.UpdateLastLogin();
            await _context.SaveChangesAsync(cancellationToken);
            
            // Record successful login
            await RecordLoginAttempt(user.Id, true, ipAddress, userAgent, null, cancellationToken);

            // Revoke all active refresh tokens for this user to prevent duplicate key conflicts
            var activeTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == user.Id && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
                .ToListAsync(cancellationToken);

            foreach (var oldToken in activeTokens)
            {
                oldToken.Revoke(ipAddress, "Revoked due to new login");
            }

            if (activeTokens.Count > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Revoked {Count} active refresh tokens for user {UserId}", activeTokens.Count, user.Id);
            }

            // Generate JWT token and refresh token
            var accessToken = await _jwtTokenService.GenerateAccessTokenAsync(user);
            var refreshToken = await _jwtTokenService.GenerateRefreshTokenAsync(user.Id, ipAddress);

            // Save refresh token (will be saved along with active session)
            // Create active session (this also saves changes, so refresh token will be saved)
            await CreateActiveSession(user.Id, refreshToken.Token, refreshToken.ExpiresAt, ipAddress, userAgent, cancellationToken);

            // Send login alert email (optional)
            try
            {
                await _emailService.SendLoginAlertAsync(user.Email.Value, user.UserName, ipAddress, DateTime.UtcNow);
            }
            catch (Exception emailEx)
            {
                _logger.LogError(emailEx, "Failed to send login alert email to {Email}", user.Email.Value);
            }

            var response = new AuthResponse
            {
                Token = accessToken,
                RefreshToken = refreshToken.Token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60), // JWT expiration
                User = _mapper.Map<UserDto>(user)
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            return Result.Failure<AuthResponse>(Error.Failure("Authentication.Login.Failed", $"Login failed: {ex.Message}"));
        }
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get refresh token from database
            var refreshToken = await _jwtTokenService.GetRefreshTokenAsync(request.RefreshToken);
            
            if (refreshToken == null || !refreshToken.IsActive)
            {
                return Result.Failure<AuthResponse>(Error.Unauthorized("Auth.Error.InvalidToken", _localizationService.GetString("Auth.Error.InvalidToken")));
            }

            // Check if token is expired
            if (refreshToken.ExpiresAt < DateTime.UtcNow)
            {
                return Result.Failure<AuthResponse>(Error.Unauthorized("Auth.Error.InvalidToken", _localizationService.GetString("Auth.Error.InvalidToken")));
            }

            // Get user
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == refreshToken.UserId, cancellationToken);

            if (user == null || !user.IsActive)
            {
                return Result.Failure<AuthResponse>(Error.Unauthorized("Auth.Error.UserNotFound", _localizationService.GetString("Auth.Error.UserNotFound")));
            }

            // Revoke old refresh token
            await _jwtTokenService.RevokeRefreshTokenAsync(refreshToken, GetClientIpAddress());

            // Generate new tokens
            var accessToken = await _jwtTokenService.GenerateAccessTokenAsync(user);
            var newRefreshToken = await _jwtTokenService.GenerateRefreshTokenAsync(user.Id, GetClientIpAddress());
            
            // Save refresh token (no need to add, it's already added by GenerateRefreshTokenAsync)
            await _context.SaveChangesAsync(cancellationToken);

            var response = new AuthResponse
            {
                Token = accessToken,
                RefreshToken = newRefreshToken.Token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60), // JWT expiration
                User = _mapper.Map<UserDto>(user)
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            return Result.Failure<AuthResponse>(Error.Failure("Authentication.RefreshToken.Failed", $"Token refresh failed: {ex.Message}"));
        }
    }

    public async Task<Result> RevokeTokenAsync(RevokeTokenRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var refreshToken = await _jwtTokenService.GetRefreshTokenAsync(request.RefreshToken);
            
            if (refreshToken != null && refreshToken.IsActive)
            {
                await _jwtTokenService.RevokeRefreshTokenAsync(refreshToken, GetClientIpAddress());
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Failure("Authentication.RevokeToken.Failed", $"Token revocation failed: {ex.Message}"));
        }
    }

    public async Task<Result> LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Revoke the refresh token if provided
            if (!string.IsNullOrEmpty(request.RefreshToken))
            {
                var refreshToken = await _jwtTokenService.GetRefreshTokenAsync(request.RefreshToken);
                
                if (refreshToken != null && refreshToken.IsActive)
                {
                    await _jwtTokenService.RevokeRefreshTokenAsync(refreshToken, GetClientIpAddress());
                }
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Failure("Authentication.Logout.Failed", $"Logout failed: {ex.Message}"));
        }
    }

    public async Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.Value == request.Email, cancellationToken);

            if (user == null)
            {
                // Don't reveal if user exists for security
                return Result.Success();
            }

            // Generate password reset token
            var resetToken = _securityService.GenerateSecureToken();
            var passwordResetToken = new PasswordResetToken(user.Id, resetToken, DateTime.UtcNow.AddHours(1)); // 1 hour expiration
            
            _context.PasswordResetTokens.Add(passwordResetToken);
            await _context.SaveChangesAsync(cancellationToken);

            // Send password reset email
            try
            {
                await _emailService.SendPasswordResetAsync(user.Email.Value, user.UserName, resetToken);
            }
            catch (Exception)
            {
                // Log email failure but don't fail the request
                // In production, you might want to queue this for retry
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Failure("Authentication.PasswordReset.Failed", $"Password reset request failed: {ex.Message}"));
        }
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Find the password reset token
            var resetToken = await _context.PasswordResetTokens
                .Include(prt => prt.User)
                .FirstOrDefaultAsync(prt => prt.Token == request.Token && !prt.IsUsed && prt.ExpiresAt > DateTime.UtcNow, cancellationToken);

            if (resetToken == null || resetToken.ExpiresAt < DateTime.UtcNow)
            {
                return Result.Failure(Error.Unauthorized("Auth.Error.InvalidToken", _localizationService.GetString("Auth.Error.InvalidToken")));
            }

            var user = resetToken.User;
            if (user == null)
            {
                return Result.Failure(Error.NotFound("Auth.Error.UserNotFound", _localizationService.GetString("Auth.Error.UserNotFound")));
            }

            // Hash new password
            var newPasswordHash = _securityService.HashPassword(request.NewPassword);
            user.SetPasswordHash(newPasswordHash);

            // Deactivate the reset token
            resetToken.Deactivate();

            _context.Users.Update(user);
            _context.PasswordResetTokens.Update(resetToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Failure("Authentication.PasswordReset.Failed", $"Password reset failed: {ex.Message}"));
        }
    }

    public async Task<Result> SendEmailVerificationAsync(SendEmailVerificationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.Value == request.Email, cancellationToken);

            if (user == null || user.IsEmailConfirmed)
            {
                return Result.Success(); // Don't reveal user existence or verification status
            }

            // Generate new verification token
            var verificationToken = _securityService.GenerateSecureToken();
            var emailVerificationToken = new EmailVerificationToken(user.Id, verificationToken, DateTime.UtcNow.AddHours(24));
            
            _context.EmailVerificationTokens.Add(emailVerificationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // Send verification email
            try
            {
                await _emailService.SendEmailVerificationAsync(user.Email.Value, user.UserName, verificationToken);
            }
            catch (Exception)
            {
                // Log email failure but don't fail the request
                // In production, you might want to queue this for retry
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Failure("Authentication.EmailVerification.Failed", $"Email verification failed: {ex.Message}"));
        }
    }

    public async Task<Result> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Find the email verification token
            var verificationToken = await _context.EmailVerificationTokens
                .Include(evt => evt.User)
                .FirstOrDefaultAsync(evt => evt.Token == request.Token && !evt.IsUsed && evt.ExpiresAt > DateTime.UtcNow, cancellationToken);

            if (verificationToken == null || verificationToken.ExpiresAt < DateTime.UtcNow)
            {
                return Result.Failure(Error.Unauthorized("Auth.Error.InvalidToken", _localizationService.GetString("Auth.Error.InvalidToken")));
            }

            var user = verificationToken.User;
            if (user == null)
            {
                return Result.Failure(Error.NotFound("Auth.Error.UserNotFound", _localizationService.GetString("Auth.Error.UserNotFound")));
            }

            // Mark email as confirmed
            user.ConfirmEmail();

            // Deactivate the verification token
            verificationToken.Deactivate();

            _context.Users.Update(user);
            _context.EmailVerificationTokens.Update(verificationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Failure("Authentication.EmailVerification.Failed", $"Email verification failed: {ex.Message}"));
        }
    }

    public async Task<Result<UserResponse>> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null)
            {
                return Result.Failure<UserResponse>(Error.NotFound("Auth.Error.UserNotFound", _localizationService.GetString("Auth.Error.UserNotFound")));
            }

            var userResponse = _mapper.Map<UserResponse>(user);
            return Result.Success(userResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get current user {UserId}", userId);
            return Result.Failure<UserResponse>(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    // Helper methods for security enhancements
    private async Task RecordLoginAttempt(
        Guid userId,
        bool isSuccessful,
        string ipAddress,
        string? userAgent,
        string? failureReason,
        CancellationToken cancellationToken)
    {
        try
        {
            var loginHistory = new LoginHistory(userId, isSuccessful, ipAddress, userAgent, failureReason);
            _context.LoginHistories.Add(loginHistory);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record login attempt for user {UserId}", userId);
        }
    }

    private async Task CreateActiveSession(
        Guid userId,
        string sessionToken,
        DateTime expiresAt,
        string ipAddress,
        string? userAgent,
        CancellationToken cancellationToken)
    {
        try
        {
            var activeSession = new ActiveSession(userId, sessionToken, expiresAt, ipAddress, userAgent);
            _context.ActiveSessions.Add(activeSession);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create active session for user {UserId}", userId);
        }
    }

    // Google OAuth methods
    public async Task<Result<AuthResponse>> AuthenticateWithGoogleAsync(string googleId, string email, string firstName, string lastName, string? profilePictureUrl = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Authenticating user with Google ID: {GoogleId}", googleId);

            // Check if user already exists with this Google ID
            var existingUser = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.GoogleId == googleId, cancellationToken);

            if (existingUser != null)
            {
                _logger.LogInformation("Found existing Google user: {UserId}", existingUser.Id);

                // Update last login
                existingUser.UpdateLastLogin();

                // Revoke all active refresh tokens for this user to prevent duplicate key conflicts
                var existingUserActiveTokens = await _context.RefreshTokens
                    .Where(rt => rt.UserId == existingUser.Id && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
                    .ToListAsync(cancellationToken);

                foreach (var oldToken in existingUserActiveTokens)
                {
                    oldToken.Revoke(GetClientIpAddress(), "Revoked due to new login");
                }

                _logger.LogInformation("Revoked {Count} active refresh tokens for user {UserId}", existingUserActiveTokens.Count, existingUser.Id);

                // Generate refresh token (adds to context but doesn't save)
                var refreshTokenEntity = await _jwtTokenService.GenerateRefreshTokenAsync(existingUser.Id, GetClientIpAddress());

                // Save both user update and refresh token in one call
                await _context.SaveChangesAsync(cancellationToken);

                // Generate JWT token
                var token = await _jwtTokenService.GenerateAccessTokenAsync(existingUser);

                var userResponse = _mapper.Map<UserDto>(existingUser);
                var authResponse = new AuthResponse
                {
                    Token = token,
                    RefreshToken = refreshTokenEntity.Token,
                    User = userResponse
                };

                return Result.Success(authResponse);
            }

            // Check if user exists with this email but different provider
            var emailUser = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email.Value == email, cancellationToken);

            if (emailUser != null)
            {
                // If user is already a Google user with a different Google ID, return error
                if (emailUser.IsGoogleUser && emailUser.GoogleId != googleId)
                {
                    _logger.LogWarning("Email {Email} is already associated with a different Google account", email);
                    return Result.Failure<AuthResponse>(Error.Conflict(
                        "Auth.Error.EmailAlreadyLinkedToDifferentGoogleAccount", 
                        _localizationService.GetString("Auth.Error.EmailAlreadyLinkedToDifferentGoogleAccount")));
                }

                // If user is already a Google user with the same Google ID, just log them in
                if (emailUser.IsGoogleUser && emailUser.GoogleId == googleId)
                {
                    _logger.LogInformation("Found existing Google user: {UserId}", emailUser.Id);
                    emailUser.UpdateLastLogin();

                    // Revoke all active refresh tokens for this user to prevent duplicate key conflicts
                    var emailUserActiveTokens = await _context.RefreshTokens
                        .Where(rt => rt.UserId == emailUser.Id && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
                        .ToListAsync(cancellationToken);

                    foreach (var oldToken in emailUserActiveTokens)
                    {
                        oldToken.Revoke(GetClientIpAddress(), "Revoked due to new login");
                    }

                    _logger.LogInformation("Revoked {Count} active refresh tokens for user {UserId}", emailUserActiveTokens.Count, emailUser.Id);

                    // Generate refresh token (adds to context but doesn't save)
                    var emailUserRefreshToken = await _jwtTokenService.GenerateRefreshTokenAsync(emailUser.Id, GetClientIpAddress());

                    // Save both user update and refresh token in one call
                    await _context.SaveChangesAsync(cancellationToken);

                    var emailUserToken = await _jwtTokenService.GenerateAccessTokenAsync(emailUser);

                    var emailUserResponse = _mapper.Map<UserDto>(emailUser);
                    var emailUserAuthResponse = new AuthResponse
                    {
                        Token = emailUserToken,
                        RefreshToken = emailUserRefreshToken.Token,
                        User = emailUserResponse
                    };

                    return Result.Success(emailUserAuthResponse);
                }

                _logger.LogInformation("Found existing user with email {Email}, linking Google account. Current provider: {Provider}", email, emailUser.Provider);

                // Link Google account to existing user (must be local provider)
                if (emailUser.Provider != "local")
                {
                    _logger.LogWarning("Cannot link Google account to user {UserId} with provider {Provider}. Only local accounts can be linked.", emailUser.Id, emailUser.Provider);
                    return Result.Failure<AuthResponse>(Error.Conflict(
                        "Auth.Error.AccountAlreadyLinkedToProvider",
                        $"This email is already associated with a {emailUser.Provider} account. Please sign in using {emailUser.Provider} instead."));
                }

                emailUser.LinkGoogleAccount(googleId, profilePictureUrl);

                // Confirm email if not already confirmed (Google emails are pre-verified)
                if (!emailUser.IsEmailConfirmed)
                {
                    emailUser.ConfirmEmail();
                }

                emailUser.UpdateLastLogin();

                // Revoke all active refresh tokens for this user to prevent duplicate key conflicts
                var linkingUserActiveTokens = await _context.RefreshTokens
                    .Where(rt => rt.UserId == emailUser.Id && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
                    .ToListAsync(cancellationToken);

                foreach (var oldToken in linkingUserActiveTokens)
                {
                    oldToken.Revoke(GetClientIpAddress(), "Revoked due to new login");
                }

                _logger.LogInformation("Revoked {Count} active refresh tokens for user {UserId}", linkingUserActiveTokens.Count, emailUser.Id);

                // Generate refresh token (adds to context but doesn't save)
                var refreshTokenEntity = await _jwtTokenService.GenerateRefreshTokenAsync(emailUser.Id, GetClientIpAddress());

                // Save both user changes and refresh token in one call
                await _context.SaveChangesAsync(cancellationToken);

                // Generate JWT token
                var token = await _jwtTokenService.GenerateAccessTokenAsync(emailUser);

                var userResponse = _mapper.Map<UserDto>(emailUser);
                var authResponse = new AuthResponse
                {
                    Token = token,
                    RefreshToken = refreshTokenEntity.Token,
                    User = userResponse
                };

                return Result.Success(authResponse);
            }

            // Create new user
            _logger.LogInformation("Creating new Google user for email: {Email}", email);
            
            var emailAddress = new EmailAddress(email);
            var newUser = User.CreateGoogleUser(googleId, firstName, lastName, emailAddress, profilePictureUrl);
            
            _context.Users.Add(newUser);
            
            // Generate refresh token (adds to context but doesn't save)
            // Note: We need to save the user first to get the ID, but we'll do it in one call
            // Actually, we can't generate the refresh token before saving the user because we need the user ID
            // So we'll save the user first, then generate and save the refresh token
            await _context.SaveChangesAsync(cancellationToken);

            // Send welcome email (email is already verified by Google, so no verification needed)
            try
            {
                _logger.LogInformation("Sending welcome email to new Google user: {Email}", email);
                await _emailService.SendWelcomeEmailAsync(email, firstName, lastName);
                _logger.LogInformation("Welcome email sent successfully to {Email}", email);
            }
            catch (Exception emailEx)
            {
                // Log email failure but don't fail registration
                _logger.LogError(emailEx, "Failed to send welcome email to {Email}. User registration completed but email not sent.", email);
            }

            // Generate JWT token
            var newToken = await _jwtTokenService.GenerateAccessTokenAsync(newUser);
            
            // Generate refresh token (adds to context but doesn't save)
            var newRefreshTokenEntity = await _jwtTokenService.GenerateRefreshTokenAsync(newUser.Id, GetClientIpAddress());
            
            // Save refresh token
            await _context.SaveChangesAsync(cancellationToken);

            var newUserResponse = _mapper.Map<UserDto>(newUser);
            var newAuthResponse = new AuthResponse
            {
                Token = newToken,
                RefreshToken = newRefreshTokenEntity.Token,
                User = newUserResponse
            };

            return Result.Success(newAuthResponse);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation during Google authentication for ID: {GoogleId}. Message: {Message}", googleId, ex.Message);
            return Result.Failure<AuthResponse>(Error.Conflict("Auth.Error.GoogleAuthenticationConflict", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to authenticate with Google for ID: {GoogleId}. Exception type: {ExceptionType}, Message: {Message}", googleId, ex.GetType().Name, ex.Message);
            return Result.Failure<AuthResponse>(Error.Failure("Auth.Error.GoogleAuthenticationFailed", _localizationService.GetString("Auth.Error.GoogleAuthenticationFailed")));
        }
    }

    public async Task<Result<AuthResponse>> LinkGoogleAccountAsync(Guid userId, string googleId, string? profilePictureUrl = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Linking Google account {GoogleId} to user {UserId}", googleId, userId);

            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null)
            {
                return Result.Failure<AuthResponse>(Error.NotFound("Auth.Error.UserNotFound", _localizationService.GetString("Auth.Error.UserNotFound")));
            }

            // Check if Google ID is already linked to another user
            var existingGoogleUser = await _context.Users
                .FirstOrDefaultAsync(u => u.GoogleId == googleId && u.Id != userId, cancellationToken);

            if (existingGoogleUser != null)
            {
                return Result.Failure<AuthResponse>(Error.Conflict("Auth.Error.GoogleAccountAlreadyLinked", _localizationService.GetString("Auth.Error.GoogleAccountAlreadyLinked")));
            }

            // Link Google account
            user.LinkGoogleAccount(googleId, profilePictureUrl);
            
            // Confirm email if not already confirmed (Google emails are pre-verified)
            if (!user.IsEmailConfirmed)
            {
                user.ConfirmEmail();
                _logger.LogInformation("Email confirmed for user {UserId} after linking Google account", userId);
            }
            
            // Generate refresh token (adds to context but doesn't save)
            var refreshTokenEntity = await _jwtTokenService.GenerateRefreshTokenAsync(user.Id, GetClientIpAddress());
            
            // Save both user changes and refresh token in one call
            await _context.SaveChangesAsync(cancellationToken);

            // Generate new JWT token
            var token = await _jwtTokenService.GenerateAccessTokenAsync(user);

            var userResponse = _mapper.Map<UserDto>(user);
            var authResponse = new AuthResponse
            {
                Token = token,
                RefreshToken = refreshTokenEntity.Token,
                User = userResponse
            };

            return Result.Success(authResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to link Google account {GoogleId} to user {UserId}", googleId, userId);
            return Result.Failure<AuthResponse>(Error.InternalServerError("Auth.Error.GoogleLinkFailed", _localizationService.GetString("Auth.Error.GoogleLinkFailed")));
        }
    }

    public async Task<Result> UnlinkGoogleAccountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Unlinking Google account for user {UserId}", userId);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null)
            {
                return Result.Failure(Error.NotFound("Auth.Error.UserNotFound", _localizationService.GetString("Auth.Error.UserNotFound")));
            }

            if (!user.IsGoogleUser)
            {
                return Result.Failure(Error.Validation("Auth.Error.NotGoogleUser", _localizationService.GetString("Auth.Error.NotGoogleUser")));
            }

            // Unlink Google account
            user.UnlinkGoogleAccount();
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unlink Google account for user {UserId}", userId);
            return Result.Failure(Error.InternalServerError("Auth.Error.GoogleUnlinkFailed", _localizationService.GetString("Auth.Error.GoogleUnlinkFailed")));
        }
    }
}
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.Auth;
using Sqordia.Contracts.Responses.Auth;
using UserDto = Sqordia.Contracts.Responses.Auth.UserDto;
using Sqordia.Domain.Entities.Identity;
using Sqordia.Domain.ValueObjects;

namespace Sqordia.Application.Services.Implementations;

/// <summary>
/// Service for handling OAuth authentication with external providers (Google, Microsoft)
/// </summary>
public class OAuthService : IOAuthService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IEmailService _emailService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<OAuthService> _logger;
    private readonly ILocalizationService _localizationService;

    public OAuthService(
        IApplicationDbContext context,
        IMapper mapper,
        IJwtTokenService jwtTokenService,
        IEmailService emailService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<OAuthService> logger,
        ILocalizationService localizationService)
    {
        _context = context;
        _mapper = mapper;
        _jwtTokenService = jwtTokenService;
        _emailService = emailService;
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

    /// <inheritdoc />
    public async Task<Result<AuthResponse>> AuthenticateWithGoogleAsync(
        GoogleLoginRequest request,
        CancellationToken cancellationToken = default)
    {
        // This delegates to the existing AuthenticationService.AuthenticateWithGoogleAsync
        // The validation of the Google token is done in the controller
        // This method expects the token to already be validated and user info extracted

        _logger.LogInformation("Google OAuth authentication requested");

        if (string.IsNullOrWhiteSpace(request.IdToken))
        {
            return Result.Failure<AuthResponse>(Error.Validation(
                "OAuth.Validation.GoogleIdTokenRequired",
                "Google ID token is required"));
        }

        // Note: The actual Google token validation happens in the controller
        // because it requires the Google SDK which is not available in the Application layer
        // This method is called after validation with extracted user info

        return Result.Failure<AuthResponse>(Error.Failure(
            "OAuth.Error.UseControllerForGoogleAuth",
            "Google authentication should be performed through the OAuth controller which handles token validation"));
    }

    /// <inheritdoc />
    public async Task<Result<AuthResponse>> AuthenticateWithMicrosoftAsync(
        MicrosoftLoginRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Microsoft OAuth authentication requested");

        if (string.IsNullOrWhiteSpace(request.AccessToken))
        {
            return Result.Failure<AuthResponse>(Error.Validation(
                "OAuth.Validation.MicrosoftAccessTokenRequired",
                "Microsoft access token is required"));
        }

        // Note: The actual Microsoft token validation happens in the controller
        // because it requires HTTP calls to Microsoft Graph API
        // This method is called after validation with extracted user info

        return Result.Failure<AuthResponse>(Error.Failure(
            "OAuth.Error.UseControllerForMicrosoftAuth",
            "Microsoft authentication should be performed through the OAuth controller which handles token validation"));
    }

    /// <summary>
    /// Internal method to authenticate with OAuth provider after token validation
    /// </summary>
    public async Task<Result<AuthResponse>> AuthenticateOAuthUserAsync(
        string provider,
        string providerId,
        string email,
        string firstName,
        string lastName,
        string? profilePictureUrl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Authenticating OAuth user. Provider: {Provider}, ProviderId: {ProviderId}", provider, providerId);

            // Check if user already exists with this provider ID
            User? existingUser = null;

            if (provider.Equals("google", StringComparison.OrdinalIgnoreCase))
            {
                existingUser = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.GoogleId == providerId, cancellationToken);
            }
            else if (provider.Equals("microsoft", StringComparison.OrdinalIgnoreCase))
            {
                existingUser = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.MicrosoftId == providerId, cancellationToken);
            }

            if (existingUser != null)
            {
                _logger.LogInformation("Found existing {Provider} user: {UserId}", provider, existingUser.Id);
                return await GenerateAuthResponseAsync(existingUser, cancellationToken);
            }

            // Check if user exists with this email
            var emailUser = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email.Value == email, cancellationToken);

            if (emailUser != null)
            {
                _logger.LogInformation("Found existing user with email {Email}, linking {Provider} account", email, provider);

                // Link the provider account
                if (provider.Equals("google", StringComparison.OrdinalIgnoreCase))
                {
                    emailUser.LinkGoogleAccount(providerId, profilePictureUrl);
                }
                else if (provider.Equals("microsoft", StringComparison.OrdinalIgnoreCase))
                {
                    emailUser.LinkMicrosoftAccount(providerId, profilePictureUrl);
                }

                // Confirm email if not already confirmed
                if (!emailUser.IsEmailConfirmed)
                {
                    emailUser.ConfirmEmail();
                }

                emailUser.UpdateLastLogin();
                await _context.SaveChangesAsync(cancellationToken);

                return await GenerateAuthResponseAsync(emailUser, cancellationToken);
            }

            // Create new user
            _logger.LogInformation("Creating new {Provider} user for email: {Email}", provider, email);

            var emailAddress = new EmailAddress(email);
            User newUser;

            if (provider.Equals("google", StringComparison.OrdinalIgnoreCase))
            {
                newUser = User.CreateGoogleUser(providerId, firstName, lastName, emailAddress, profilePictureUrl);
            }
            else if (provider.Equals("microsoft", StringComparison.OrdinalIgnoreCase))
            {
                newUser = User.CreateMicrosoftUser(providerId, firstName, lastName, emailAddress, profilePictureUrl);
            }
            else
            {
                return Result.Failure<AuthResponse>(Error.Validation(
                    "OAuth.Error.UnsupportedProvider",
                    $"Unsupported OAuth provider: {provider}"));
            }

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync(cancellationToken);

            // Send welcome email
            try
            {
                _logger.LogInformation("Sending welcome email to new {Provider} user: {Email}", provider, email);
                await _emailService.SendWelcomeEmailAsync(email, firstName, lastName);
            }
            catch (Exception emailEx)
            {
                _logger.LogError(emailEx, "Failed to send welcome email to {Email}", email);
            }

            return await GenerateAuthResponseAsync(newUser, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to authenticate OAuth user. Provider: {Provider}", provider);
            return Result.Failure<AuthResponse>(Error.InternalServerError(
                "OAuth.Error.AuthenticationFailed",
                $"{provider} authentication failed"));
        }
    }

    private async Task<Result<AuthResponse>> GenerateAuthResponseAsync(User user, CancellationToken cancellationToken)
    {
        // Revoke existing refresh tokens
        var activeTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == user.Id && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        foreach (var oldToken in activeTokens)
        {
            oldToken.Revoke(GetClientIpAddress(), "Revoked due to new OAuth login");
        }

        user.UpdateLastLogin();

        // Generate new tokens
        var accessToken = await _jwtTokenService.GenerateAccessTokenAsync(user);
        var refreshToken = await _jwtTokenService.GenerateRefreshTokenAsync(user.Id, GetClientIpAddress());

        await _context.SaveChangesAsync(cancellationToken);

        var userResponse = _mapper.Map<UserDto>(user);
        var authResponse = new AuthResponse
        {
            Token = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            User = userResponse
        };

        return Result.Success(authResponse);
    }
}

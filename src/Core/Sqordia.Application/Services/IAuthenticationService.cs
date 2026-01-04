using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.Auth;
using Sqordia.Contracts.Responses.Auth;

namespace Sqordia.Application.Services;

public interface IAuthenticationService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
    Task<Result> RevokeTokenAsync(RevokeTokenRequest request, CancellationToken cancellationToken = default);
    Task<Result> LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default);
    Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);
    Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
    Task<Result> SendEmailVerificationAsync(SendEmailVerificationRequest request, CancellationToken cancellationToken = default);
    Task<Result> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken = default);
    Task<Result<UserResponse>> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default);
    
    // Google OAuth methods
    Task<Result<AuthResponse>> AuthenticateWithGoogleAsync(string googleId, string email, string firstName, string lastName, string? profilePictureUrl = null, CancellationToken cancellationToken = default);
    Task<Result<AuthResponse>> LinkGoogleAccountAsync(Guid userId, string googleId, string? profilePictureUrl = null, CancellationToken cancellationToken = default);
    Task<Result> UnlinkGoogleAccountAsync(Guid userId, CancellationToken cancellationToken = default);
}
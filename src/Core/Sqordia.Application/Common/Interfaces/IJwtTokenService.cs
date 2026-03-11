using Sqordia.Domain.Entities.Identity;

namespace Sqordia.Application.Common.Interfaces;

public interface IJwtTokenService
{
    Task<string> GenerateAccessTokenAsync(User user);
    Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId, string ipAddress);
    Task<string?> ValidateAccessTokenAsync(string token);
    Task<RefreshToken?> GetRefreshTokenAsync(string token);
    Task RevokeRefreshTokenAsync(RefreshToken refreshToken, string ipAddress, string? replacedByToken = null);
    Task<bool> IsAccessTokenValidAsync(string token);

    /// <summary>
    /// Generates a short-lived JWT (5 min) for 2FA verification during login.
    /// </summary>
    string GenerateTwoFactorToken(Guid userId);

    /// <summary>
    /// Validates a 2FA token and returns the userId if valid, null otherwise.
    /// </summary>
    Guid? ValidateTwoFactorToken(string token);
}
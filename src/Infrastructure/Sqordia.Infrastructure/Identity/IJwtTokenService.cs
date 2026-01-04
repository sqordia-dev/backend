using Sqordia.Domain.Entities.Identity;

namespace Sqordia.Infrastructure.Identity;

public interface IJwtTokenService
{
    Task<string> GenerateAccessTokenAsync(User user);
    Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId, string ipAddress);
    Task<string?> ValidateAccessTokenAsync(string token);
    Task<RefreshToken?> GetRefreshTokenAsync(string token);
    Task RevokeRefreshTokenAsync(RefreshToken refreshToken, string ipAddress, string? replacedByToken = null);
    Task<bool> IsAccessTokenValidAsync(string token);
}
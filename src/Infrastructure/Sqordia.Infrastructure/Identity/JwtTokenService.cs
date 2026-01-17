using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Domain.Entities.Identity;

namespace Sqordia.Infrastructure.Identity;

public class JwtTokenService : IJwtTokenService, Application.Common.Interfaces.IJwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly IApplicationDbContext _context;
    private readonly TokenValidationParameters _tokenValidationParameters;

    public JwtTokenService(IConfiguration configuration, IApplicationDbContext context)
    {
        _configuration = configuration;
        _context = context;
        
        var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? 
                       _configuration["JwtSettings:Secret"] ?? 
                       throw new InvalidOperationException("JWT Secret must be configured");

        _tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = _configuration["JwtSettings:Issuer"],
            ValidateAudience = true,
            ValidAudience = _configuration["JwtSettings:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    }

    public async Task<string> GenerateAccessTokenAsync(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? _configuration["JwtSettings:Secret"]!;
        var key = Encoding.UTF8.GetBytes(jwtSecret);
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email.Value),
            new(ClaimTypes.Name, user.UserName),
            new("firstName", user.FirstName),
            new("lastName", user.LastName)
        };

        // Add roles
        foreach (var userRole in user.UserRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Name));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["JwtSettings:ExpirationInMinutes"]!)),
            Issuer = _configuration["JwtSettings:Issuer"],
            Audience = _configuration["JwtSettings:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return await Task.FromResult(tokenHandler.WriteToken(token));
    }

    public async Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId, string ipAddress)
    {
        using var rng = RandomNumberGenerator.Create();
        var tokenBytes = new byte[64];
        rng.GetBytes(tokenBytes);
        var refreshToken = Convert.ToBase64String(tokenBytes);

        var token = new RefreshToken(
            userId,
            refreshToken,
            DateTime.UtcNow.AddDays(7), // 7 days expiration
            ipAddress
        );

        // Add to context but don't save - let the caller save changes
        _context.RefreshTokens.Add(token);

        return await Task.FromResult(token);
    }

    public Task<bool> ValidateTokenAsync(string token)
    {
        throw new NotImplementedException();
    }

    public Task<User?> GetUserFromTokenAsync(string token)
    {
        throw new NotImplementedException();
    }

    public async Task<string?> ValidateAccessTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, _tokenValidationParameters, out var validatedToken);
            
            if (validatedToken is JwtSecurityToken jwtToken &&
                jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return await Task.FromResult(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            }
        }
        catch
        {
            // Token validation failed
        }

        return await Task.FromResult<string?>(null);
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }

    public async Task RevokeRefreshTokenAsync(RefreshToken refreshToken, string ipAddress, string? replacedByToken = null)
    {
        refreshToken.Revoke(ipAddress, replacedByToken);
        _context.RefreshTokens.Update(refreshToken);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsAccessTokenValidAsync(string token)
    {
        var userId = await ValidateAccessTokenAsync(token);
        return !string.IsNullOrEmpty(userId);
    }
}
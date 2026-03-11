namespace Sqordia.Contracts.Responses.Auth;

/// <summary>
/// Authentication response containing JWT token and user information.
/// When RequiresTwoFactor is true, Token/RefreshToken/User will be null
/// and the client must verify with a TOTP code using the TwoFactorToken.
/// </summary>
public class AuthResponse
{
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }
    public UserDto? User { get; set; }

    /// <summary>
    /// When true, credentials were valid but a 2FA code is required to complete login.
    /// </summary>
    public bool RequiresTwoFactor { get; set; }

    /// <summary>
    /// Short-lived token (5 min) to identify the pending 2FA verification.
    /// Only set when RequiresTwoFactor is true.
    /// </summary>
    public string? TwoFactorToken { get; set; }
}

public class UserDto
{
    public Guid Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string UserName { get; set; }
    public required IEnumerable<string> Roles { get; set; }
    public bool OnboardingCompleted { get; set; }
    public string? Persona { get; set; }
}

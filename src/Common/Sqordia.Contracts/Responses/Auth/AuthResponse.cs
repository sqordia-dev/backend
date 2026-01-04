namespace Sqordia.Contracts.Responses.Auth;

/// <summary>
/// Authentication response containing JWT token and user information
/// </summary>
public class AuthResponse
{
    public required string Token { get; set; }
    public required string RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }
    public required UserDto User { get; set; }
}

public class UserDto
{
    public Guid Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string UserName { get; set; }
    public required IEnumerable<string> Roles { get; set; }
}

namespace Sqordia.Contracts.Requests.Auth;

public class RevokeTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
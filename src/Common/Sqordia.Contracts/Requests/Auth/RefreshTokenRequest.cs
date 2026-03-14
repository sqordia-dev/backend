namespace Sqordia.Contracts.Requests.Auth;

public class RefreshTokenRequest
{
    public string? Token { get; set; }

    public string? RefreshToken { get; set; }
}

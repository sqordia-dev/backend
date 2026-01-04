namespace Sqordia.Contracts.Responses.Auth;

public class VerifyEmailResponse
{
    public string Message { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
}


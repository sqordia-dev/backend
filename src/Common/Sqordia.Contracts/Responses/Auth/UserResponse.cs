namespace Sqordia.Contracts.Responses.Auth;

public class UserResponse
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserType { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; }
    public IEnumerable<string> Roles { get; set; } = new List<string>();
}
namespace Sqordia.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<string> HashPasswordAsync(string password);
    Task<bool> VerifyPasswordAsync(string password, string hashedPassword);
}
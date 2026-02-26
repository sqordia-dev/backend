namespace Sqordia.Application.Common.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? UserEmail { get; }
    bool IsAuthenticated { get; }
    string? GetUserId();
    Guid? GetUserIdAsGuid();
    string? GetUserEmail();
    bool IsInRole(string role);
    bool HasPermission(string permission);
}

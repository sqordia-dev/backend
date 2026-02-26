using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Sqordia.Application.Common.Interfaces;

namespace Sqordia.Application.Services.Implementations;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => GetUserId();

    public string? UserEmail => GetUserEmail();

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public string? GetUserId()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value
               ?? _httpContextAccessor.HttpContext?.User?.FindFirst("user_id")?.Value
               ?? _httpContextAccessor.HttpContext?.User?.FindFirst("id")?.Value;
    }

    public Guid? GetUserIdAsGuid()
    {
        var userIdString = GetUserId();
        return Guid.TryParse(userIdString, out var userId) ? userId : null;
    }

    public string? GetUserEmail()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst("email")?.Value
               ?? _httpContextAccessor.HttpContext?.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;
    }

    public bool IsInRole(string role)
    {
        return _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
    }

    public bool HasPermission(string permission)
    {
        return _httpContextAccessor.HttpContext?.User?.HasClaim("permission", permission) ?? false;
    }
}

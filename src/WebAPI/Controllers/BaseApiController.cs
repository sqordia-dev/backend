using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Common.Models;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System.Security.Claims;
using WebAPI.Constants;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public abstract class BaseApiController : ControllerBase
{
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess && result.Value != null)
        {
            return Ok(result.Value);
        }

        if (result.IsSuccess && result.Value == null)
        {
            return NotFound();
        }

        return MapErrorToStatusCode(result.Error);
    }

    protected IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
        {
            return Ok();
        }

        return MapErrorToStatusCode(result.Error);
    }

    /// <summary>
    /// Maps an Error to the appropriate HTTP status code based on the error code
    /// </summary>
    private IActionResult MapErrorToStatusCode(Error? error)
    {
        if (error == null)
        {
            return BadRequest(new { message = ControllerConstants.ErrorAnErrorOccurred });
        }

        var errorCode = error.Code;
        var errorMessage = error.Message;

        // Map error codes to HTTP status codes
        if (errorCode.Contains("Unauthorized", StringComparison.OrdinalIgnoreCase))
        {
            return Unauthorized(error);
        }

        if (errorCode.Contains("Forbidden", StringComparison.OrdinalIgnoreCase))
        {
            return StatusCode(403, error);
        }

        if (errorCode.Contains("NotFound", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(error);
        }

        if (errorCode.Contains("Conflict", StringComparison.OrdinalIgnoreCase))
        {
            return Conflict(error);
        }

        // Default to BadRequest for validation errors and other failures
        return BadRequest(error);
    }

    /// <summary>
    /// Gets the current user ID from the JWT token
    /// </summary>
    protected Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    /// <summary>
    /// Gets the current user's email from the JWT token
    /// </summary>
    protected string? GetCurrentUserEmail()
    {
        return User.FindFirst(ClaimTypes.Email)?.Value;
    }

    /// <summary>
    /// Gets the current user's username from the JWT token
    /// </summary>
    protected string? GetCurrentUserUsername()
    {
        return User.FindFirst(ClaimTypes.Name)?.Value;
    }

    /// <summary>
    /// Checks if the current user has a specific role
    /// </summary>
    protected bool HasRole(string role)
    {
        return User.IsInRole(role);
    }

    /// <summary>
    /// Checks if the current user has any of the specified roles
    /// </summary>
    protected bool HasAnyRole(params string[] roles)
    {
        return roles.Any(role => User.IsInRole(role));
    }

    /// <summary>
    /// Checks if the current user is an admin
    /// </summary>
    protected bool IsAdmin()
    {
        return HasRole(ControllerConstants.RoleAdmin);
    }

    /// <summary>
    /// Checks if the current user is an organization owner
    /// </summary>
    protected bool IsOrganizationOwner()
    {
        return HasRole(ControllerConstants.RoleOwner);
    }

    /// <summary>
    /// Checks if the current user is an organization admin
    /// </summary>
    protected bool IsOrganizationAdmin()
    {
        return HasAnyRole(ControllerConstants.RoleAdmin, ControllerConstants.RoleOwner);
    }

    /// <summary>
    /// Checks if the current user is a collaborator
    /// </summary>
    protected bool IsCollaborator()
    {
        return HasAnyRole("Collaborateur", "Admin", "Owner");
    }

    /// <summary>
    /// Checks if the current user is a reader
    /// </summary>
    protected bool IsReader()
    {
        return HasAnyRole("Lecteur", "Member", "Collaborateur", "Admin", "Owner");
    }

    /// <summary>
    /// Returns unauthorized if user is not authenticated
    /// </summary>
    protected IActionResult? RequireAuthentication()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return Unauthorized(new { message = "Authentication required" });
        }
        return null;
    }

    /// <summary>
    /// Returns forbidden if user doesn't have required role
    /// </summary>
    protected IActionResult? RequireRole(string role)
    {
        if (!HasRole(role))
        {
            return Forbid($"Role '{role}' required");
        }
        return null;
    }

    /// <summary>
    /// Returns forbidden if user doesn't have any of the required roles
    /// </summary>
    protected IActionResult? RequireAnyRole(params string[] roles)
    {
        if (!HasAnyRole(roles))
        {
            return Forbid($"One of the following roles required: {string.Join(", ", roles)}");
        }
        return null;
    }

    /// <summary>
    /// Returns forbidden if user is not an admin
    /// </summary>
    protected IActionResult? RequireAdmin()
    {
        if (!IsAdmin())
        {
            return Forbid("Admin role required");
        }
        return null;
    }

    /// <summary>
    /// Returns forbidden if user is not an organization admin
    /// </summary>
    protected IActionResult? RequireOrganizationAdmin()
    {
        if (!IsOrganizationAdmin())
        {
            return Forbid("Organization admin or owner role required");
        }
        return null;
    }

    /// <summary>
    /// Returns forbidden if user is not an organization owner
    /// </summary>
    protected IActionResult? RequireOrganizationOwner()
    {
        if (!IsOrganizationOwner())
        {
            return Forbid("Organization owner role required");
        }
        return null;
    }
}

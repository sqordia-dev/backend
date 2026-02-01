using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services;
using Sqordia.Contracts.Requests.Role;

namespace WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/roles")]
public class RoleManagementController : BaseApiController
{
    private readonly IRoleManagementService _roleManagementService;

    public RoleManagementController(IRoleManagementService roleManagementService)
    {
        _roleManagementService = roleManagementService;
    }

    /// <summary>
    /// Get all roles
    /// </summary>
    /// <returns>List of all roles</returns>
    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetAllRoles(CancellationToken cancellationToken = default)
    {
        var result = await _roleManagementService.GetAllRolesAsync();
        return HandleResult(result);
    }

    /// <summary>
    /// Get role by ID
    /// </summary>
    /// <param name="id">Role ID</param>
    /// <returns>Role details</returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetRoleById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _roleManagementService.GetRoleByIdAsync(id);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a new role
    /// </summary>
    /// <param name="request">Role creation request</param>
    /// <returns>Created role</returns>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _roleManagementService.CreateRoleAsync(request);
        return HandleResult(result);
    }

    /// <summary>
    /// Update an existing role
    /// </summary>
    /// <param name="id">Role ID</param>
    /// <param name="request">Role update request</param>
    /// <returns>Updated role</returns>
    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _roleManagementService.UpdateRoleAsync(id, request);
        return HandleResult(result);
    }

    /// <summary>
    /// Delete a role
    /// </summary>
    /// <param name="id">Role ID</param>
    /// <returns>Success or failure</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteRole(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _roleManagementService.DeleteRoleAsync(id);
        return HandleResult(result);
    }

    /// <summary>
    /// Assign a role to a user
    /// </summary>
    /// <param name="request">Role assignment request</param>
    /// <returns>Success or failure</returns>
    [HttpPost("assign")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _roleManagementService.AssignRoleToUserAsync(request);
        return HandleResult(result);
    }

    /// <summary>
    /// Remove a role from a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="roleId">Role ID</param>
    /// <returns>Success or failure</returns>
    [HttpDelete("users/{userId}/roles/{roleId}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> RemoveRole(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
    {
        var result = await _roleManagementService.RemoveRoleFromUserAsync(userId, roleId);
        return HandleResult(result);
    }

    /// <summary>
    /// Get all roles for a specific user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>List of user's roles</returns>
    [HttpGet("users/{userId}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetUserRoles(Guid userId, CancellationToken cancellationToken = default)
    {
        var result = await _roleManagementService.GetUserRolesAsync(userId);
        return HandleResult(result);
    }

    /// <summary>
    /// Get all available permissions
    /// </summary>
    /// <returns>List of all permissions</returns>
    [HttpGet("permissions")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetAllPermissions(CancellationToken cancellationToken = default)
    {
        var result = await _roleManagementService.GetAllPermissionsAsync();
        return HandleResult(result);
    }
}

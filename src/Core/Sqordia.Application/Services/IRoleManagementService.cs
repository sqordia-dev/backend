using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.Role;
using Sqordia.Contracts.Responses.Role;

namespace Sqordia.Application.Services;

public interface IRoleManagementService
{
    // Role CRUD
    Task<Result<RoleResponse>> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken = default);
    Task<Result<RoleResponse>> GetRoleByIdAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<Result<List<RoleResponse>>> GetAllRolesAsync(CancellationToken cancellationToken = default);
    Task<Result<RoleResponse>> UpdateRoleAsync(Guid roleId, UpdateRoleRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteRoleAsync(Guid roleId, CancellationToken cancellationToken = default);

    // User Role Assignment
    Task<Result> AssignRoleToUserAsync(AssignRoleRequest request, CancellationToken cancellationToken = default);
    Task<Result> RemoveRoleFromUserAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);
    Task<Result<List<RoleResponse>>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default);

    // Permissions
    Task<Result<List<PermissionDto>>> GetAllPermissionsAsync(CancellationToken cancellationToken = default);
}


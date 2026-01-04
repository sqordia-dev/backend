using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.Role;
using Sqordia.Contracts.Responses.Role;
using Sqordia.Domain.Entities.Identity;

namespace Sqordia.Application.Services.Implementations;

public class RoleManagementService : IRoleManagementService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<RoleManagementService> _logger;
    private readonly ILocalizationService _localizationService;

    public RoleManagementService(
        IApplicationDbContext context,
        ILogger<RoleManagementService> logger,
        ILocalizationService localizationService)
    {
        _context = context;
        _logger = logger;
        _localizationService = localizationService;
    }

    public async Task<Result<RoleResponse>> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if role name already exists
            var existingRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == request.Name, cancellationToken);

            if (existingRole != null)
            {
                return Result.Failure<RoleResponse>(Error.Conflict("Role.Error.AlreadyExists", _localizationService.GetString("Role.Error.AlreadyExists")));
            }

            // Validate permissions exist
            if (request.PermissionIds.Any())
            {
                var permissionCount = await _context.Permissions
                    .CountAsync(p => request.PermissionIds.Contains(p.Id), cancellationToken);

                if (permissionCount != request.PermissionIds.Count)
                {
                    return Result.Failure<RoleResponse>(Error.Validation("Role.Error.InvalidPermissions", _localizationService.GetString("Role.Error.InvalidPermissions")));
                }
            }

            // Create role
            var role = new Role(request.Name, request.Description, request.IsDefault);
            _context.Roles.Add(role);
            await _context.SaveChangesAsync(cancellationToken);

            // Add permissions to role
            foreach (var permissionId in request.PermissionIds)
            {
                var rolePermission = new RolePermission(role.Id, permissionId);
                _context.RolePermissions.Add(rolePermission);
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Role '{RoleName}' created successfully with ID {RoleId}", request.Name, role.Id);

            // Fetch created role with permissions
            return await GetRoleByIdAsync(role.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role");
            return Result.Failure<RoleResponse>(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    public async Task<Result<RoleResponse>> GetRoleByIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        try
        {
            var role = await _context.Roles
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

            if (role == null)
            {
                return Result.Failure<RoleResponse>(Error.NotFound("Role.Error.NotFound", _localizationService.GetString("Role.Error.NotFound")));
            }

            var response = MapToRoleResponse(role);
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting role by ID {RoleId}", roleId);
            return Result.Failure<RoleResponse>(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    public async Task<Result<List<RoleResponse>>> GetAllRolesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var roles = await _context.Roles
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .OrderBy(r => r.Name)
                .ToListAsync(cancellationToken);

            var response = roles.Select(MapToRoleResponse).ToList();
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all roles");
            return Result.Failure<List<RoleResponse>>(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    public async Task<Result<RoleResponse>> UpdateRoleAsync(Guid roleId, UpdateRoleRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var role = await _context.Roles
                .Include(r => r.RolePermissions)
                .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

            if (role == null)
            {
                return Result.Failure<RoleResponse>(Error.NotFound("Role.Error.NotFound", _localizationService.GetString("Role.Error.NotFound")));
            }

            // Check if role is system role
            if (role.IsSystemRole)
            {
                return Result.Failure<RoleResponse>(Error.Forbidden("Role.Error.SystemRole", _localizationService.GetString("Role.Error.SystemRole")));
            }

            // Check if new name conflicts with existing role
            if (role.Name != request.Name)
            {
                var existingRole = await _context.Roles
                    .FirstOrDefaultAsync(r => r.Name == request.Name && r.Id != roleId, cancellationToken);

                if (existingRole != null)
                {
                    return Result.Failure<RoleResponse>(Error.Conflict("Role.Error.AlreadyExists", _localizationService.GetString("Role.Error.AlreadyExists")));
                }
            }

            // Validate permissions exist
            if (request.PermissionIds.Any())
            {
                var permissionCount = await _context.Permissions
                    .CountAsync(p => request.PermissionIds.Contains(p.Id), cancellationToken);

                if (permissionCount != request.PermissionIds.Count)
                {
                    return Result.Failure<RoleResponse>(Error.Validation("Role.Error.InvalidPermissions", _localizationService.GetString("Role.Error.InvalidPermissions")));
                }
            }

            // Update role
            role.Update(request.Name, request.Description);

            // Update permissions - remove old ones and add new ones
            var existingPermissions = role.RolePermissions.ToList();
            foreach (var rp in existingPermissions)
            {
                _context.RolePermissions.Remove(rp);
            }

            foreach (var permissionId in request.PermissionIds)
            {
                var rolePermission = new RolePermission(role.Id, permissionId);
                _context.RolePermissions.Add(rolePermission);
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Role '{RoleName}' updated successfully", request.Name);

            return await GetRoleByIdAsync(roleId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role {RoleId}", roleId);
            return Result.Failure<RoleResponse>(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    public async Task<Result> DeleteRoleAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        try
        {
            var role = await _context.Roles
                .Include(r => r.UserRoles)
                .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

            if (role == null)
            {
                return Result.Failure(Error.NotFound("Role.Error.NotFound", _localizationService.GetString("Role.Error.NotFound")));
            }

            // Check if role is system role
            if (role.IsSystemRole)
            {
                return Result.Failure(Error.Forbidden("Role.SystemRole", "Cannot delete system roles"));
            }

            // Check if role is assigned to any users
            if (role.UserRoles.Any())
            {
                return Result.Failure(Error.Conflict("Role.Error.InUse", _localizationService.GetString("Role.Error.InUse")));
            }

            // Remove role permissions first
            var rolePermissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .ToListAsync(cancellationToken);

            _context.RolePermissions.RemoveRange(rolePermissions);

            // Remove role
            _context.Roles.Remove(role);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Role {RoleId} deleted successfully", roleId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role {RoleId}", roleId);
            return Result.Failure(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    public async Task<Result> AssignRoleToUserAsync(AssignRoleRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Verify user exists
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId && !u.IsDeleted, cancellationToken);

            if (user == null)
            {
                return Result.Failure(Error.NotFound("Auth.Error.UserNotFound", _localizationService.GetString("Auth.Error.UserNotFound")));
            }

            // Verify role exists
            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);

            if (role == null)
            {
                return Result.Failure(Error.NotFound("Role.Error.NotFound", _localizationService.GetString("Role.Error.NotFound")));
            }

            // Check if user already has this role
            var existingUserRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == request.UserId && ur.RoleId == request.RoleId, cancellationToken);

            if (existingUserRole != null)
            {
                return Result.Failure(Error.Conflict("Role.Error.AlreadyAssigned", _localizationService.GetString("Role.Error.AlreadyAssigned")));
            }

            // Assign role to user
            var userRole = new UserRole(request.UserId, request.RoleId);
            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Role '{RoleName}' assigned to user {UserId}", role.Name, request.UserId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role to user");
            return Result.Failure(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    public async Task<Result> RemoveRoleFromUserAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
    {
        try
        {
            var userRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId, cancellationToken);

            if (userRole == null)
            {
                return Result.Failure(Error.NotFound("Role.Error.NotAssigned", _localizationService.GetString("Role.Error.NotAssigned")));
            }

            _context.UserRoles.Remove(userRole);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Role {RoleId} removed from user {UserId}", roleId, userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing role from user");
            return Result.Failure(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    public async Task<Result<List<RoleResponse>>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);

            if (user == null)
            {
                return Result.Failure<List<RoleResponse>>(Error.NotFound("Auth.Error.UserNotFound", _localizationService.GetString("Auth.Error.UserNotFound")));
            }

            var roles = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Include(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                .Select(ur => ur.Role)
                .ToListAsync(cancellationToken);

            var response = roles.Select(MapToRoleResponse).ToList();
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user roles for user {UserId}", userId);
            return Result.Failure<List<RoleResponse>>(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    public async Task<Result<List<PermissionDto>>> GetAllPermissionsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var permissions = await _context.Permissions
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Name)
                .ToListAsync(cancellationToken);

            var response = permissions.Select(p => new PermissionDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Category = p.Category
            }).ToList();

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all permissions");
            return Result.Failure<List<PermissionDto>>(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    private static RoleResponse MapToRoleResponse(Role role)
    {
        return new RoleResponse
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IsDefault = role.IsSystemRole,
            Created = DateTime.UtcNow, // BaseEntity doesn't have Created/Updated timestamps
            LastModified = null,
            Permissions = role.RolePermissions.Select(rp => new PermissionDto
            {
                Id = rp.Permission.Id,
                Name = rp.Permission.Name,
                Description = rp.Permission.Description,
                Category = rp.Permission.Category
            }).ToList()
        };
    }
}


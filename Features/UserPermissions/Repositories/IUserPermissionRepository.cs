using LMS_SoulCode.Features.UserPermissions.DTOs;

namespace LMS_SoulCode.Features.UserPermissions.Repositories
{
    public interface IUserPermissionRepository
    {
        Task AssignRoleAsync(int userId, int roleId, int? tenantId, CancellationToken cancellationToken = default);
        Task AssignPermissionsAsync(AssignPermissionDto dto, int? tenantId, CancellationToken cancellationToken = default);
        Task<List<UserPermissionDto>> GetUserPermissionsAsync(int userId, int? targetTenantId = null, CancellationToken cancellationToken = default);
        Task<bool> CheckUserPermissionAsync(int userId, string moduleCode, string permissionCode, int? targetTenantId = null, CancellationToken cancellationToken = default);
        Task RemoveRoleFromUserAsync(int userId, int roleId, CancellationToken cancellationToken = default);
        Task<List<string>> GetUserRolesAsync(int userId, int? targetTenantId = null, CancellationToken cancellationToken = default);
        Task<bool> UserExistsAsync(int userId, CancellationToken cancellationToken = default);
        Task<bool> RoleExistsAsync(int roleId, CancellationToken cancellationToken = default);
        Task<bool> ModuleExistsAsync(int moduleId, CancellationToken cancellationToken = default);
        Task<List<int>> ValidatePermissionsAsync(List<int> permissionIds, CancellationToken cancellationToken = default);
        Task<bool> UserRoleExistsAsync(int userId, int roleId, CancellationToken cancellationToken = default);
        Task<Common.ApiResponse<List<string>>> UpdateUserRoleStatusAsync(int userId, int roleId, bool isActive, CancellationToken cancellationToken = default);
        Task<List<UserRoleDto>> GetUserRolesWithStatusAsync(int userId, CancellationToken cancellationToken = default);
        Task<(IEnumerable<UserRoleDto> Items, int TotalCount)> GetUserRolesPagedAsync(int? userId, int? roleId, bool? isActive, string? searchTerm, int pageNumber, int pageSize, int? tenantId, CancellationToken cancellationToken);
        Task<List<UserPermissionDto>> GetRoleModulePermissionsAsync(int roleId, int moduleId, int? tenantId, CancellationToken cancellationToken = default);
        Task<bool> SoftDeleteUserRoleAsync(int userId, int roleId, CancellationToken cancellationToken = default);
        Task<bool> RestoreUserRoleAsync(int userId, int roleId, CancellationToken cancellationToken = default);
    }
}

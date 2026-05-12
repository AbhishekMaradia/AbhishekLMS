using LMS_SoulCode.Features.UserPermissions.DTOs;
using LMS_SoulCode.Features.Common;

namespace LMS_SoulCode.Features.UserPermissions.Services
{
    public interface IUserPermissionService
    {
        Task<ApiResponse<List<string>>> AssignRoleToUserAsync(AssignRoleDto dto, int? tenantId, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<string>>> AssignPermissionsAsync(AssignPermissionDto dto, int? tenantId, CancellationToken cancellationToken = default);
        Task<ApiResponse<IEnumerable<UserPermissionDto>>> GetUserPermissionsAsync(int userId, int? tenantId, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<bool>>> CheckUserPermissionAsync(int userId, string moduleCode, string permissionCode, int? tenantId, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<string>>> RemoveRoleFromUserAsync(int userId, int roleId, int? tenantId, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<string>>> UpdateUserRoleStatusAsync(int userId, int roleId, bool isActive, int? tenantId, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<string>>> UpdateUserRoleAsync(int userId, int oldRoleId, int? oldTenantId, int newRoleId, int? newTenantId, CancellationToken cancellationToken = default);
        Task<ApiResponse<PagedApiResponse<UserRoleDto>>> GetUserRolesPagedAsync(UserRoleListRequest request, int? tenantId, CancellationToken cancellationToken);
        Task<ApiResponse<IEnumerable<UserPermissionDto>>> GetRoleModulePermissionsAsync(int roleId, int moduleId, int? tenantId, CancellationToken cancellationToken = default);
    }
}

using LMS_SoulCode.Features.UserPermissions.Models;
using LMS_SoulCode.Features.UserPermissions.DTOs;
using LMS_SoulCode.Features.Common.Repositories;
using LMS_SoulCode.Features.Common;

namespace LMS_SoulCode.Features.UserPermissions.Repositories
{
    public interface IRoleModulePermissionRepository : IBaseRepository<RoleModulePermission>
    {
        Task<bool> RoleModuleExistsAsync(int roleModuleId, CancellationToken cancellationToken = default);
        Task<bool> PermissionExistsAsync(int permissionId, CancellationToken cancellationToken = default);
        Task<bool> RoleModulePermissionExistsAsync(int roleModuleId, int permissionId, CancellationToken cancellationToken = default);
        Task<(IEnumerable<RoleModulePermissionDto> Items, int TotalCount)> GetPagedAsync(
            int? roleId,
            int? moduleId,
            int? permissionId,
            int? roleModuleId,
            string? searchTerm,
            int pageNumber,
            int pageSize,
            int? tenantId,
            CancellationToken cancellationToken);
    }
}

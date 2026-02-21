using LMS_SoulCode.Features.UserPermissions.DTOs;
using LMS_SoulCode.Features.UserPermissions.Models;
using LMS_SoulCode.Features.Common.Repositories;

namespace LMS_SoulCode.Features.UserPermissions.Repositories
{
    public interface IModulePermissionRepository : IBaseRepository<ModulePermission>
    {
        Task<(IEnumerable<ModulePermissionDto> Items, int TotalCount)> GetPagedAsync(
            int? moduleId,
            int? permissionId,
            string? searchTerm,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken);
    }
}
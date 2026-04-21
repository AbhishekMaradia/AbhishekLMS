using LMS_SoulCode.Features.UserPermissions.Models;
using LMS_SoulCode.Features.UserPermissions.DTOs;
using LMS_SoulCode.Features.Common.Repositories;

namespace LMS_SoulCode.Features.UserPermissions.Repositories
{
    public interface IRoleModuleRepository : IBaseRepository<RoleModule>
    {
        Task<List<RoleModule>> GetByRoleIdAsync(int roleId, int? tenantId, CancellationToken cancellationToken = default);
        Task<RoleModule?> FindByRoleAndModuleAsync(int roleId, int moduleId, CancellationToken cancellationToken = default);
        Task<(IEnumerable<RoleModuleListDto> Items, int TotalCount)> GetRoleModulesAsync(string? searchTerm, int pageNumber, int pageSize, int? roleId, int? moduleId, int? tenantId, CancellationToken cancellationToken);
    }
}

using LMS_SoulCode.Features.UserPermissions.Models;
using static LMS_SoulCode.Features.UserPermissions.DTOs.RoleDtos;
using LMS_SoulCode.Features.Common.Repositories;

namespace LMS_SoulCode.Features.UserPermissions.Repositories
{
    public interface IRoleRepository : IBaseRepository<Role>
    {
        Task<(IEnumerable<GetRoleDto> Items, int TotalCount)> GetRolesAsync(string? searchTerm, int pageNumber, int pageSize, bool? isActive, int? tenantId, CancellationToken cancellationToken);
        Task<GetRoleDto?> GetDtoByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Role?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
        Task<int> CascadeRoleIsActiveToUserRolesAsync(int roleId, bool isActive, CancellationToken cancellationToken = default);
    }
}

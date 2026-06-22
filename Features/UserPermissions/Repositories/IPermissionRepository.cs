using LMS_SoulCode.Features.UserPermissions.Models;
using LMS_SoulCode.Features.UserPermissions.DTOs;
using LMS_SoulCode.Features.Common.Repositories;

namespace LMS_SoulCode.Features.UserPermissions.Repositories
{
    public interface IPermissionRepository : IBaseRepository<Permission>
    {
        Task<(IEnumerable<GetPermissionDto> Items, int TotalCount)> GetPermissionsAsync(string? searchTerm, int pageNumber, int pageSize, bool? isActive, CancellationToken cancellationToken);
        Task<GetPermissionDto?> GetDtoByIdAsync(int id, CancellationToken cancellationToken = default);
    }
}

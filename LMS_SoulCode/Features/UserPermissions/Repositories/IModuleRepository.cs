using LMS_SoulCode.Features.UserPermissions.Models;
using LMS_SoulCode.Features.UserPermissions.DTOs;
using LMS_SoulCode.Features.Common.Repositories;
using static LMS_SoulCode.Features.UserPermissions.DTOs.ModuleDtos;

namespace LMS_SoulCode.Features.UserPermissions.Repositories
{
    public interface IModuleRepository : IBaseRepository<Module>
    {
        Task<List<GetModuleDto>> GetAllDtoAsync(CancellationToken cancellationToken = default);
        Task<GetModuleDto?> GetDtoByIdAsync(int id, CancellationToken cancellationToken = default);
        Task LinkPermissionsAsync(int moduleId, List<int> permissionIds, CancellationToken cancellationToken = default);
        Task<(IEnumerable<GetModuleDto> Items, int TotalCount)> GetModulesAsync(string? searchTerm, int pageNumber, int pageSize, bool? isActive, CancellationToken cancellationToken);
        Task<List<PermissionDtos.GetPermissionDto>> GetModulePermissionsAsync(int moduleId, CancellationToken cancellationToken = default);
    }
}

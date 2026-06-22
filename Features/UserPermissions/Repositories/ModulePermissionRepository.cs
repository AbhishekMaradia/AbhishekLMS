using LMS_SoulCode.Data;
using LMS_SoulCode.Features.UserPermissions.Models;
using LMS_SoulCode.Features.Common.Repositories;
using LMS_SoulCode.Features.UserPermissions.DTOs;
using LMS_SoulCode.Features.Common;
using Microsoft.EntityFrameworkCore;

namespace LMS_SoulCode.Features.UserPermissions.Repositories
{
    public class ModulePermissionRepository : BaseRepository<ModulePermission>, IModulePermissionRepository
    {
        public ModulePermissionRepository(LmsDbContext context) : base(context)
        {
        }

        public override async Task<ModulePermission?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
            await _context.ModulePermissions
                .Include(mp => mp.Module)
                .Include(mp => mp.Permission)
                .FirstOrDefaultAsync(mp => mp.Id == id, cancellationToken);

        public async Task<(IEnumerable<ModulePermissionDto> Items, int TotalCount)> GetPagedAsync(
            int? moduleId,
            int? permissionId,
            string? searchTerm,
            int pageNumber,
            int pageSize,
            int? tenantId,
            CancellationToken cancellationToken)
        {
            return await GetPagedAsync<ModulePermissionDto>(
                pageNumber,
                pageSize,
                filter: mp => (!tenantId.HasValue || mp.TenantId == tenantId.Value || mp.TenantId == null) &&
                             (!moduleId.HasValue || mp.ModuleId == moduleId.Value) &&
                             (!permissionId.HasValue || mp.PermissionId == permissionId.Value) &&
                             (string.IsNullOrWhiteSpace(searchTerm) ||
                              mp.Module.Name.ToLower().Contains(searchTerm.ToLower()) ||
                              mp.Module.Code.ToLower().Contains(searchTerm.ToLower()) ||
                              mp.Permission.Name.ToLower().Contains(searchTerm.ToLower()) ||
                              mp.Permission.Code.ToLower().Contains(searchTerm.ToLower())),
                projection: q => q.AsNoTracking().OrderByDescending(mp => mp.Id).Select(mp => new ModulePermissionDto
                {
                    Id = mp.Id,
                    ModuleId = mp.ModuleId,
                    PermissionId = mp.PermissionId,
                    ModuleName = mp.Module.Name,
                    ModuleCode = mp.Module.Code,
                    PermissionName = mp.Permission.Name,
                    PermissionCode = mp.Permission.Code
                }),
                cancellationToken: cancellationToken
            );
        }
    }
}
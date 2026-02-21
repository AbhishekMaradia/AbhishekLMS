using LMS_SoulCode.Data;
using LMS_SoulCode.Features.UserPermissions.Models;
using LMS_SoulCode.Features.Common.Repositories;
using LMS_SoulCode.Features.UserPermissions.DTOs;
using LMS_SoulCode.Features.Common;
using Microsoft.EntityFrameworkCore;

namespace LMS_SoulCode.Features.UserPermissions.Repositories
{
    public class RoleModulePermissionRepository : BaseRepository<RoleModulePermission>, IRoleModulePermissionRepository
    {
        public RoleModulePermissionRepository(LmsDbContext context) : base(context)
        {
        }

        public override async Task<RoleModulePermission?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
            await _context.RoleModulePermissions
                .Include(rmp => rmp.RoleModule)
                    .ThenInclude(rm => rm.Role)
                .Include(rmp => rmp.RoleModule)
                    .ThenInclude(rm => rm.Module)
                .Include(rmp => rmp.Permission)
                .FirstOrDefaultAsync(rmp => rmp.Id == id, cancellationToken);

        public async Task<bool> RoleModuleExistsAsync(int roleModuleId, CancellationToken cancellationToken = default) =>
            await _context.RoleModules.AnyAsync(rm => rm.Id == roleModuleId, cancellationToken);

        public async Task<bool> PermissionExistsAsync(int permissionId, CancellationToken cancellationToken = default) =>
            await _context.Permissions.AnyAsync(p => p.Id == permissionId && (p.IsActive ?? true), cancellationToken);

        public async Task<bool> RoleModulePermissionExistsAsync(int roleModuleId, int permissionId, CancellationToken cancellationToken = default) =>
            await _context.RoleModulePermissions.AnyAsync(rmp => 
                rmp.RoleModuleId == roleModuleId && rmp.PermissionId == permissionId, cancellationToken);

        public async Task<(IEnumerable<RoleModulePermissionDto> Items, int TotalCount)> GetPagedAsync(
            int? roleId,
            int? moduleId,
            int? permissionId,
            int? roleModuleId,
            string? searchTerm,
            int pageNumber,
            int pageSize,
            int? tenantId,
            CancellationToken cancellationToken)
        {
            return await GetPagedAsync<RoleModulePermissionDto>(
                pageNumber,
                pageSize,
                filter: rmp => (!tenantId.HasValue || (rmp.RoleModule.Role.TenantId == tenantId.Value || (rmp.RoleModule.Role.TenantId == null && rmp.RoleModule.Role.IsDefault))) &&
                             (!roleId.HasValue || rmp.RoleModule.Role.Id == roleId.Value) &&
                             (!moduleId.HasValue || rmp.RoleModule.Module.Id == moduleId.Value) &&
                             (!permissionId.HasValue || rmp.PermissionId == permissionId.Value) &&
                             (!roleModuleId.HasValue || rmp.RoleModuleId == roleModuleId.Value) &&
                             (string.IsNullOrWhiteSpace(searchTerm) ||
                              rmp.RoleModule.Role.Name.ToLower().Contains(searchTerm.ToLower()) ||
                              rmp.RoleModule.Role.Code.ToLower().Contains(searchTerm.ToLower()) ||
                              rmp.RoleModule.Module.Name.ToLower().Contains(searchTerm.ToLower()) ||
                              rmp.RoleModule.Module.Code.ToLower().Contains(searchTerm.ToLower()) ||
                              rmp.Permission.Name.ToLower().Contains(searchTerm.ToLower()) ||
                              rmp.Permission.Code.ToLower().Contains(searchTerm.ToLower())),
                projection: q => q.AsNoTracking().OrderByDescending(rmp => rmp.Id).Select(rmp => new RoleModulePermissionDto
                {
                    Id = rmp.Id,
                    RoleModuleId = rmp.RoleModuleId,
                    PermissionId = rmp.PermissionId,
                    RoleName = rmp.RoleModule.Role.Name,
                    RoleCode = rmp.RoleModule.Role.Code,
                    ModuleName = rmp.RoleModule.Module.Name,
                    ModuleCode = rmp.RoleModule.Module.Code,
                    PermissionName = rmp.Permission.Name,
                    PermissionCode = rmp.Permission.Code
                }),
                cancellationToken: cancellationToken
            );
        }
    }
}

using LMS_SoulCode.Data;
using LMS_SoulCode.Features.UserPermissions.Models;
using LMS_SoulCode.Features.Common.Repositories;
using LMS_SoulCode.Features.UserPermissions.DTOs;
using LMS_SoulCode.Features.Common;
using Microsoft.EntityFrameworkCore;

namespace LMS_SoulCode.Features.UserPermissions.Repositories
{
    public class RoleModuleRepository : BaseRepository<RoleModule>, IRoleModuleRepository
    {
        public RoleModuleRepository(LmsDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<RoleModule>> GetAllAsync(CancellationToken cancellationToken = default) =>
            await _context.RoleModules
                .Include(rm => rm.Role)
                .Include(rm => rm.Module)
                .Where(rm => rm.Role.IsActive && (rm.Module.IsActive ?? true))
                .AsNoTracking()
                .ToListAsync(cancellationToken);

        public override async Task<RoleModule?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
            await _context.RoleModules
                .Include(rm => rm.Role)
                .Include(rm => rm.Module)
                .Where(rm => rm.Id == id && rm.Role.IsActive && (rm.Module.IsActive ?? true))
                .FirstOrDefaultAsync(cancellationToken);

        public async Task<List<RoleModule>> GetByRoleIdAsync(int roleId, int? tenantId, CancellationToken cancellationToken = default)
        {
            var query = _context.RoleModules
                .Include(rm => rm.Module)
                .Include(rm => rm.Role)
                .Where(rm => rm.RoleId == roleId && (rm.Module.IsActive ?? true));

            if (tenantId.HasValue)
            {
                query = query.Where(rm => rm.Role.TenantId == tenantId.Value || (rm.Role.TenantId == null && rm.Role.IsDefault));
            }

            return await query.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<RoleModule?> FindByRoleAndModuleAsync(int roleId, int moduleId, CancellationToken cancellationToken = default) =>
            await _context.RoleModules
                .FirstOrDefaultAsync(rm => rm.RoleId == roleId && rm.ModuleId == moduleId, cancellationToken);

        public async Task<(IEnumerable<RoleModuleListDto> Items, int TotalCount)> GetRoleModulesAsync(
            string? searchTerm, 
            int pageNumber, 
            int pageSize, 
            int? roleId, 
            int? moduleId, 
            int? tenantId,
            CancellationToken cancellationToken)
        {
            return await GetPagedAsync<RoleModuleListDto>(
                pageNumber,
                pageSize,
                filter: rm => rm.Role.IsActive && (rm.Module.IsActive ?? true) &&
                             (!tenantId.HasValue || (rm.Role.TenantId == tenantId.Value || (rm.Role.TenantId == null && rm.Role.IsDefault))) &&
                             (!roleId.HasValue || rm.RoleId == roleId.Value) &&
                             (!moduleId.HasValue || rm.ModuleId == moduleId.Value) &&
                             (string.IsNullOrWhiteSpace(searchTerm) || 
                              rm.Role.Name.ToLower().Contains(searchTerm.ToLower()) ||
                              rm.Role.Code.ToLower().Contains(searchTerm.ToLower()) ||
                              rm.Module.Name.ToLower().Contains(searchTerm.ToLower()) ||
                              rm.Module.Code.ToLower().Contains(searchTerm.ToLower())),
                projection: q => q.AsNoTracking().OrderByDescending(rm => rm.Id).Select(rm => new RoleModuleListDto
                {
                    Id = rm.Id,
                    RoleId = rm.RoleId,
                    RoleName = rm.Role.Name,
                    RoleCode = rm.Role.Code,
                    ModuleId = rm.ModuleId,
                    ModuleName = rm.Module.Name,
                    ModuleCode = rm.Module.Code,
                    CreatedAt = rm.CreatedAt
                }),
                cancellationToken: cancellationToken
            );
        }
    }
}

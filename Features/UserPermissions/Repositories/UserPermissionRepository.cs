using AutoMapper;
using AutoMapper.QueryableExtensions;
using LMS_SoulCode.Data;
using LMS_SoulCode.Features.UserPermissions.Models;
using LMS_SoulCode.Features.UserPermissions.DTOs;
using LMS_SoulCode.Features.Common;
using LMS_SoulCode.Features.Common.Pagination;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace LMS_SoulCode.Features.UserPermissions.Repositories
{
    public class UserPermissionRepository : IUserPermissionRepository
    {
        private readonly LmsDbContext _context;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;

        public UserPermissionRepository(LmsDbContext context, IMapper mapper, IMemoryCache cache)
        {
            _context = context;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task AssignRoleAsync(int userId, int roleId, int? tenantId, CancellationToken cancellationToken = default)
        {
            // Check if record exists even if soft-deleted (IgnoreQueryFilters)
            var existing = await _context.UserRoles
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId && (ur.TenantId == tenantId || (tenantId == null && ur.TenantId == 0) || (tenantId == 0 && ur.TenantId == null)), cancellationToken);

            if (existing != null)
            {
                existing.IsDeleted = false;
                existing.IsActive = true;
                existing.CreatedAt = DateTime.UtcNow; // Reset creation time to now
                _context.UserRoles.Update(existing);
            }
            else
            {
                var userRole = new UserRole
                {
                    UserId = userId,
                    RoleId = roleId,
                    TenantId = tenantId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.UserRoles.AddAsync(userRole, cancellationToken);
            }
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task AssignPermissionsAsync(AssignPermissionDto dto, int? tenantId, CancellationToken cancellationToken = default)
        {
            // 1. Find or create RoleModule
            var roleModule = await _context.RoleModules
                .FirstOrDefaultAsync(rm => rm.RoleId == dto.RoleId && rm.ModuleId == dto.ModuleId && (rm.TenantId == tenantId || (tenantId == null && rm.TenantId == 0) || (tenantId == 0 && rm.TenantId == null)), cancellationToken);

            if (roleModule == null)
            {
                roleModule = new RoleModule
                {
                    RoleId = dto.RoleId,
                    ModuleId = dto.ModuleId,
                    TenantId = tenantId,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                await _context.RoleModules.AddAsync(roleModule, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }

            // 2. Validate that these permissions are actually allowed for this module
            var allowedPermissions = await _context.ModulePermissions
                .Where(mp => mp.ModuleId == dto.ModuleId)
                .Select(mp => mp.PermissionId)
                .ToListAsync(cancellationToken);

            var validPermissionIds = dto.PermissionIds.Intersect(allowedPermissions).ToList();

            // 3. Clear existing permissions for this RoleModule to avoid duplicates
            var existing = await _context.RoleModulePermissions
                .Where(rmp => rmp.RoleModuleId == roleModule.Id)
                .ToListAsync(cancellationToken);
            _context.RoleModulePermissions.RemoveRange(existing);

            // 4. Assign newly validated permissions
            foreach (var permissionId in validPermissionIds)
            {
                var roleModulePermission = new RoleModulePermission
                {
                    RoleModuleId = roleModule.Id,
                    PermissionId = permissionId,
                    TenantId = tenantId,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                await _context.RoleModulePermissions.AddAsync(roleModulePermission, cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<UserPermissionDto>> GetUserPermissionsAsync(int userId, int? targetTenantId = null, CancellationToken cancellationToken = default)
        {
            var currentTenantId = targetTenantId > 0 ? targetTenantId : _context.CurrentTenantId;

            // OPTIMIZED: Single query with IgnoreQueryFilters to include Global (Shared) metadata
            var query = from ur in _context.UserRoles.IgnoreQueryFilters()
                        join u in _context.Users.IgnoreQueryFilters() on ur.UserId equals u.Id
                        join r in _context.Roles.IgnoreQueryFilters() on ur.RoleId equals r.Id
                        join rm in _context.RoleModules.IgnoreQueryFilters() on r.Id equals rm.RoleId
                        join m in _context.Modules.IgnoreQueryFilters() on rm.ModuleId equals m.Id
                        join rmp in _context.RoleModulePermissions.IgnoreQueryFilters() on rm.Id equals rmp.RoleModuleId
                        join p in _context.Permissions.IgnoreQueryFilters() on rmp.PermissionId equals p.Id
                        where ur.UserId == userId 
                              && u.IsActive
                              && !u.IsDeleted
                              && ur.IsActive 
                              && !ur.IsDeleted
                              && r.IsActive 
                              && !r.IsDeleted
                              && !rm.IsDeleted
                              && !rmp.IsDeleted
                              && (m.IsActive ?? true)
                              && !m.IsDeleted
                              && (p.IsActive ?? true)
                              && !p.IsDeleted
                              // Manual Tenant Filtering Logic
                              && (r.TenantId == null || r.TenantId == 0 || r.TenantId == currentTenantId)
                              && (rm.Role.TenantId == null || rm.Role.TenantId == 0 || rm.Role.TenantId == currentTenantId) // Redundant but safe
                        select new UserPermissionDto
                        {
                            RoleCode = r.Code,
                            ModuleCode = m.Code,
                            PermissionCode = p.Code,
                            PermissionName = p.Name
                        };

            return await query.AsNoTracking().Distinct().ToListAsync(cancellationToken);
        }


        public async Task<bool> CheckUserPermissionAsync(int userId, string moduleCode, string permissionCode, int? targetTenantId = null, CancellationToken cancellationToken = default)
        {
            var currentTenantId = targetTenantId > 0 ? targetTenantId : _context.CurrentTenantId;

            // OPTIMIZED: Direct JOIN query with IgnoreQueryFilters
            return await (from ur in _context.UserRoles.IgnoreQueryFilters()
                         join u in _context.Users.IgnoreQueryFilters() on ur.UserId equals u.Id
                         join r in _context.Roles.IgnoreQueryFilters() on ur.RoleId equals r.Id
                         join rm in _context.RoleModules.IgnoreQueryFilters() on r.Id equals rm.RoleId
                         join m in _context.Modules.IgnoreQueryFilters() on rm.ModuleId equals m.Id
                         join rmp in _context.RoleModulePermissions.IgnoreQueryFilters() on rm.Id equals rmp.RoleModuleId
                         join p in _context.Permissions.IgnoreQueryFilters() on rmp.PermissionId equals p.Id
                         where ur.UserId == userId
                               && u.IsActive
                               && !u.IsDeleted
                               && ur.IsActive
                               && !ur.IsDeleted
                               && r.IsActive
                               && !r.IsDeleted
                               && !rm.IsDeleted
                               && !rmp.IsDeleted
                               && m.Code == moduleCode.ToUpperInvariant()
                               && !m.IsDeleted
                               && p.Code == permissionCode.ToUpperInvariant()
                               && (p.IsActive ?? true)
                               && !p.IsDeleted
                               // Manual Tenant Filtering Logic
                               && (r.TenantId == null || r.TenantId == 0 || r.TenantId == currentTenantId)
                         select ur)
                         .AsNoTracking()
                         .AnyAsync(cancellationToken);
        }

        public async Task RemoveRoleFromUserAsync(int userId, int roleId, CancellationToken cancellationToken = default)
        {
            var userRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId && !ur.IsDeleted, cancellationToken);
            
            if (userRole != null)
            {
                // Change from hard delete to soft delete
                userRole.IsDeleted = true;
                // DeletedAt will be set automatically by DbContext SaveChangesAsync override
                _context.UserRoles.Update(userRole);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<bool> UserExistsAsync(int userId, CancellationToken cancellationToken = default)
        => await _context.Users.AnyAsync(u => u.Id == userId, cancellationToken);
        

        public async Task<bool> RoleExistsAsync(int roleId, CancellationToken cancellationToken = default)
        => await _context.Roles.AnyAsync(r => r.Id == roleId && r.IsActive, cancellationToken);
        

        public async Task<bool> ModuleExistsAsync(int moduleId, CancellationToken cancellationToken = default)
        => await _context.Modules.AnyAsync(m => m.Id == moduleId && (m.IsActive ?? true), cancellationToken);
        

        public async Task<List<int>> ValidatePermissionsAsync(List<int> permissionIds, CancellationToken cancellationToken = default)
        => await _context.Permissions
                .Where(p => permissionIds.Contains(p.Id) && (p.IsActive ?? true))
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);
        

        public async Task<bool> UserRoleExistsAsync(int userId, int roleId, CancellationToken cancellationToken = default)
        => await _context.UserRoles
                .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId && ur.IsActive && !ur.IsDeleted, cancellationToken);
        

        public async Task<List<string>> GetUserRolesAsync(int userId, int? targetTenantId = null, CancellationToken cancellationToken = default)
        {
            // Check cache first
            var cacheKey = $"user_roles_{userId}_{(targetTenantId.HasValue ? targetTenantId.Value.ToString() : "global")}";
            if (_cache.TryGetValue(cacheKey, out List<string> cachedRoles))
            {
                return cachedRoles;
            }

            var currentTenantId = targetTenantId > 0 ? targetTenantId : _context.CurrentTenantId;

            // OPTIMIZED: Direct JOIN query for maximum performance (even first time)
            var roles = await (from ur in _context.UserRoles.IgnoreQueryFilters()
                              join u in _context.Users.IgnoreQueryFilters() on ur.UserId equals u.Id
                              join r in _context.Roles.IgnoreQueryFilters() on ur.RoleId equals r.Id
                              where ur.UserId == userId 
                                    && u.IsActive
                                    && !u.IsDeleted
                                    && ur.IsActive 
                                    && !ur.IsDeleted 
                                    && r.IsActive
                                    && !r.IsDeleted
                                    // Manual Tenant Filtering Logic
                                    && (r.TenantId == null || r.TenantId == 0 || r.TenantId == currentTenantId)
                              select r.Code)
                              .AsNoTracking()
                              .ToListAsync(cancellationToken);

            // Cache for 10 minutes with size
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                Size = 1 // Small size for role list
            };
            _cache.Set(cacheKey, roles, cacheOptions);

            return roles;
        }

        public async Task<Common.ApiResponse<List<string>>> UpdateUserRoleStatusAsync(int userId, int roleId, bool isActive, CancellationToken cancellationToken = default)
        {
            var userRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId && !ur.IsDeleted, cancellationToken);

            if (userRole == null)
                return Common.ApiResponse<List<string>>.Fail("User-Role assignment not found", Common.StatusCodes.NotFound);

            if (userRole.IsActive == isActive)
            {
                var statusText = isActive ? "active" : "inactive";
                return Common.ApiResponse<List<string>>.Fail($"User-Role is already {statusText}", Common.StatusCodes.BadRequest);
            }

            userRole.IsActive = isActive;
            await _context.SaveChangesAsync(cancellationToken);
            
            var message = isActive ? "User-Role activated successfully" : "User-Role deactivated successfully";
            return Common.ApiResponse<List<string>>.Success(new List<string>(), message);
        }

        public async Task<List<UserRoleDto>> GetUserRolesWithStatusAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _context.UserRoles
                .AsNoTracking()
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == userId && !ur.IsDeleted)  // Exclude soft deleted UserRoles
                .Select(ur => new UserRoleDto
                {
                    UserId = ur.UserId,
                    RoleId = ur.RoleId,
                    RoleName = ur.Role.Name,
                    RoleCode = ur.Role.Code,
                    IsActive = ur.IsActive,
                    RoleIsActive = ur.Role.IsActive
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<(IEnumerable<UserRoleDto> Items, int TotalCount)> GetUserRolesPagedAsync(
            int? userId,
            int? roleId,
            bool? isActive,
            string? searchTerm,
            int pageNumber,
            int pageSize,
            int? tenantId,
            CancellationToken cancellationToken)
        {
            var query = _context.UserRoles
                .AsNoTracking()
                .Include(ur => ur.Role)
                .Include(ur => ur.User)
                .Where(ur => !ur.IsDeleted)  // Exclude soft deleted UserRoles
                .AsQueryable();

            // Filter by TenantId
            if (tenantId.HasValue)
            {
                query = query.Where(ur => ur.User.TenantId == tenantId.Value || (tenantId.Value == 0 && ur.User.TenantId == null));
            }

            // Filter by UserId
            if (userId.HasValue)
            {
                query = query.Where(ur => ur.UserId == userId.Value);
            }

            // Filter by RoleId
            if (roleId.HasValue)
            {
                query = query.Where(ur => ur.RoleId == roleId.Value);
            }

            // Filter by IsActive
            if (isActive.HasValue)
            {
                query = query.Where(ur => ur.IsActive == isActive.Value);
            }

            // Search by User name/email or Role name/code
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(ur =>
                    ur.User.Email.ToLower().Contains(searchTerm) ||
                    ur.Role.Name.ToLower().Contains(searchTerm) ||
                    ur.Role.Code.ToLower().Contains(searchTerm)
                );
            }

            // Project to DTO
            var dtoQuery = query
                .OrderByDescending(ur => ur.Id)
                .Select(ur => new UserRoleDto
                {
                    UserId = ur.UserId,
                    RoleId = ur.RoleId,
                    RoleName = ur.Role.Name,
                    RoleCode = ur.Role.Code,
                    IsActive = ur.IsActive,
                    RoleIsActive = ur.Role.IsActive,
                    UserEmail = ur.User.Email
                });

            return await dtoQuery.ToPagedListAsync(pageNumber, pageSize, cancellationToken);
        }

        public async Task<List<UserPermissionDto>> GetRoleModulePermissionsAsync(int roleId, int moduleId, int? tenantId, CancellationToken cancellationToken = default)
        => await _context.RoleModulePermissions
                .AsNoTracking()
                .Include(rmp => rmp.RoleModule)
                    .ThenInclude(rm => rm.Role)
                .Include(rmp => rmp.RoleModule)
                    .ThenInclude(rm => rm.Module)
                .Include(rmp => rmp.Permission)
                .Where(rmp => 
                    rmp.RoleModule.RoleId == roleId && 
                    rmp.RoleModule.ModuleId == moduleId &&
                    (rmp.TenantId == tenantId || (tenantId == null && rmp.TenantId == 0) || (tenantId == 0 && rmp.TenantId == null)) &&
                    (rmp.Permission.IsActive ?? true))
                .ProjectTo<UserPermissionDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

        public async Task<bool> SoftDeleteUserRoleAsync(int userId, int roleId, CancellationToken cancellationToken = default)
        {
            var userRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId && !ur.IsDeleted, cancellationToken);
            
            if (userRole == null || userRole.IsDeleted) return false;

            userRole.IsDeleted = true;
            // DeletedAt will be set automatically by DbContext SaveChangesAsync override
            _context.UserRoles.Update(userRole);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<bool> RestoreUserRoleAsync(int userId, int roleId, CancellationToken cancellationToken = default)
        {
            var userRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId && ur.IsDeleted, cancellationToken);
            
            if (userRole == null || !userRole.IsDeleted) return false;

            userRole.IsDeleted = false;
            // DeletedAt will be cleared automatically by DbContext SaveChangesAsync override
            _context.UserRoles.Update(userRole);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}

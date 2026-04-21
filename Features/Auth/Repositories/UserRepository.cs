using AutoMapper;
using LMS_SoulCode.Features.Auth.Models;
using LMS_SoulCode.Data;
using Microsoft.EntityFrameworkCore;
using LMS_SoulCode.Features.Auth.DTOs;
using LMS_SoulCode.Features.Common;
using LMS_SoulCode.Features.Common.Repositories;
using LMS_SoulCode.Features.UserPermissions.Models;

namespace LMS_SoulCode.Features.Auth.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        private readonly IMapper _mapper;

        public UserRepository(LmsDbContext context, IMapper mapper) : base(context)
        {
            _mapper = mapper;
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await _context.Users
                .Include(u => u.Organization)
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(u => !u.IsDeleted)
                .FirstOrDefaultAsync(u => u.Email == email || u.Mobile == email, cancellationToken);
        

        
        public async Task<bool> IsEmailTakenAsync(string email, int? tenantId = null, CancellationToken cancellationToken = default)
        => await _context.Users
            .IgnoreQueryFilters()
            .AsNoTracking()
            .AnyAsync(u => u.Email == email && !u.IsDeleted, cancellationToken);


        public async Task<bool> IsMobileTakenAsync(string mobile, int? tenantId = null, CancellationToken cancellationToken = default)
        => await _context.Users
            .IgnoreQueryFilters()
            .AsNoTracking()
            .AnyAsync(u => u.Mobile == mobile && !u.IsDeleted, cancellationToken);

        public async Task<(IEnumerable<UserDto> Items, int TotalCount)> GetUsersAsync(string? searchTerm, int pageNumber, int pageSize, int? tenantId, CancellationToken cancellationToken)
        => await GetPagedAsync<UserDto>(
                pageNumber,
                pageSize,
                filter: u => (!tenantId.HasValue || tenantId.Value == 0 || u.TenantId == tenantId.Value) &&
                            (string.IsNullOrWhiteSpace(searchTerm) || 
                             u.Email.ToLower().Contains(searchTerm.ToLower()) ||
                             (u.FirstName + " " + u.LastName).ToLower().Contains(searchTerm.ToLower()) ||
                             u.Mobile.Contains(searchTerm)),
                projection: q => q.AsNoTracking().OrderByDescending(u => u.Id).Select(u => new UserDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Mobile = u.Mobile,
                    Email = u.Email,
                    UserRole = string.Join(", ", _context.UserRoles
                        .Where(ur => ur.UserId == u.Id && ur.Role.IsActive)
                        .Select(ur => ur.Role.Name)
                        .Take(3)) ?? "No Role",
                    TenantId = u.TenantId,
                    OrgName = u.Organization != null ? u.Organization.Name : null,
                    GroupId = u.GroupId,
                    GroupName = u.Group != null ? u.Group.GroupName : null,
                    RoleId = _context.UserRoles.Where(ur => ur.UserId == u.Id && !ur.IsDeleted).Select(ur => (int?)ur.RoleId).FirstOrDefault(),
                    RoleIds = _context.UserRoles.Where(ur => ur.UserId == u.Id && !ur.IsDeleted).Select(ur => ur.RoleId).ToList(),
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                }),
                cancellationToken: cancellationToken
            );
        

        public async Task<string?> GenerateResetTokenAsync(string email, CancellationToken cancellationToken = default)
        {
            var user = await GetByEmailAsync(email, cancellationToken);
            if (user == null) return null;

            var token = Guid.NewGuid().ToString();
            user.ResetToken = token;
            user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);

            _context.Users.Update(user);
            await _context.SaveChangesAsync(cancellationToken);

            return token;
        }

        public async Task<bool> ValidateResetTokenAsync(string token, CancellationToken cancellationToken = default)
        => await _context.Users.AnyAsync(
                u => u.ResetToken == token && u.ResetTokenExpiry > DateTime.UtcNow, cancellationToken
            );
        

        public async Task UpdatePasswordAsync(string email, string newPassword, CancellationToken cancellationToken = default)
        {
            var user = await GetByEmailAsync(email, cancellationToken);
            if (user == null) return;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.ResetToken = null;
            user.ResetTokenExpiry = null;

            _context.Users.Update(user);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<string?> GenerateRefreshTokenAsync(string email, CancellationToken cancellationToken = default)
        {
            var user = await GetByEmailAsync(email, cancellationToken);
            if (user == null) return null;

            var token = Guid.NewGuid().ToString();
            user.RefreshToken = token;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

            _context.Users.Update(user);
            await _context.SaveChangesAsync(cancellationToken);

            return token;
        }

        public async Task<bool> ValidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        => await _context.Users.AnyAsync(
                u => u.RefreshToken == refreshToken && u.RefreshTokenExpiry > DateTime.UtcNow, cancellationToken
            );
        

        public async Task UpdateRefreshTokenAsync(string email, string newRefreshToken, CancellationToken cancellationToken = default)
        {
            var user = await GetByEmailAsync(email, cancellationToken);
            if (user == null) return;

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

            _context.Users.Update(user);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<User?> GetUserByResetTokenAsync(string token, CancellationToken cancellationToken = default)
        => await _context.Users.FirstOrDefaultAsync(
                u => u.ResetToken == token && u.ResetTokenExpiry > DateTime.UtcNow, cancellationToken
            );
        

        public async Task UpdatePasswordAsync(User user, CancellationToken cancellationToken = default)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<UserDto?> GetUserByIdAsync(int id, int? tenantId, CancellationToken cancellationToken = default)
        {
            var query = _context.Users
                .AsNoTracking()
                .Where(u => u.Id == id && !u.IsDeleted);

            if (tenantId.HasValue && tenantId.Value != 0)
            {
                query = query.Where(u => u.TenantId == tenantId.Value);
            }

            return await query
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Mobile = u.Mobile,
                    Email = u.Email,
                    UserRole = string.Join(", ", _context.UserRoles
                        .Where(ur => ur.UserId == u.Id && ur.Role.IsActive)
                        .Select(ur => ur.Role.Name)
                        .Take(3)) // Limit to 3 roles to avoid very long strings
                        ?? "No Role", // Show all active roles, comma-separated
                    TenantId = u.TenantId,
                    OrgName = u.Organization != null ? u.Organization.Name : null,
                    GroupId = u.GroupId,
                    GroupName = u.Group != null ? u.Group.GroupName : null,
                    RoleId = _context.UserRoles.Where(ur => ur.UserId == u.Id && !ur.IsDeleted).Select(ur => (int?)ur.RoleId).FirstOrDefault(),
                    RoleIds = _context.UserRoles.Where(ur => ur.UserId == u.Id && !ur.IsDeleted).Select(ur => ur.RoleId).ToList(),
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<UserDto?> UpdateUserAsync(int id, UpdateUserRequest request, int? tenantId, CancellationToken cancellationToken = default)
        {
            var user = await _context.Users
                .Include(u => u.Organization)
                .Include(u => u.Group)
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted, cancellationToken);
                
            if (user == null || (tenantId.HasValue && tenantId.Value != 0 && user.TenantId != tenantId.Value)) return null;
            
            // Security: If Org Admin is attempting to change TenantId, block it (AutoMapper handles it, but let's be explicit)
            if (tenantId.HasValue && tenantId.Value != 0 && request.TenantId.HasValue && request.TenantId != tenantId.Value)
            {
                 // Effectively ignore the request to change tenant as Org Admin
                 request.TenantId = tenantId.Value;
            }

            // Relationship integrity: If Tenant changes, cleared GroupId to avoid cross-tenant orphaning
            if (request.TenantId.HasValue && user.TenantId != request.TenantId.Value)
            {
                user.GroupId = null; // Reset group as it's tenant-specific
            }

            // Use AutoMapper to update only non-null properties
            _mapper.Map(request, user);

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            }

            if (request.RoleIds != null)
            {
                var uniqueRoleIds = request.RoleIds.Distinct().ToList();
                var existingRoles = await _context.UserRoles.Where(ur => ur.UserId == user.Id && !ur.IsDeleted).ToListAsync(cancellationToken);
                var existingRoleIds = existingRoles.Select(r => r.RoleId).ToList();

                // 1. Remove roles no longer in the request
                var rolesToRemove = existingRoles.Where(r => !uniqueRoleIds.Contains(r.RoleId)).ToList();
                if (rolesToRemove.Any())
                {
                    _context.UserRoles.RemoveRange(rolesToRemove);
                }

                // 2. Add new roles
                var rolesToAdd = uniqueRoleIds.Where(id => !existingRoleIds.Contains(id)).ToList();
                foreach (var roleId in rolesToAdd)
                {
                    await _context.UserRoles.AddAsync(new UserRole
                    {
                        UserId = user.Id,
                        RoleId = roleId,
                        TenantId = user.TenantId,
                        IsActive = true
                    }, cancellationToken);
                }
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync(cancellationToken);

            // Return updated user as DTO directly (optimized)
            var userDto = _mapper.Map<UserDto>(user);
            
            // Map OrgName and GroupName manually if needed or rely on mapping
            if (user.Organization != null) userDto.OrgName = user.Organization.Name;
            if (user.Group != null) userDto.GroupName = user.Group.GroupName;
            
            return userDto;
        }

        public async Task<bool> SoftDeleteUserAsync(int id, int? tenantId, CancellationToken cancellationToken = default)
        {
            var user = await _context.Users.FindAsync(new object[] { id }, cancellationToken);
            if (user == null || user.IsDeleted || (tenantId.HasValue && tenantId.Value != 0 && user.TenantId != tenantId.Value)) return false;

            user.IsDeleted = true;
            _context.Users.Update(user);

            // Cascade Soft Delete: Mark roles as deleted
            var roles = await _context.UserRoles.Where(ur => ur.UserId == id).ToListAsync(cancellationToken);
            foreach (var r in roles)
            {
                r.IsDeleted = true;
            }

            // Cascade Soft Delete: Mark course enrollments as deleted
            var enrollments = await _context.UserCourses.Where(uc => uc.UserId == id).ToListAsync(cancellationToken);
            foreach (var e in enrollments)
            {
                e.IsDeleted = true;
            }

            // Cascade Soft Delete: Mark video progress as deleted
            var videoProgress = await _context.UserVideoProgresses.Where(uvp => uvp.UserId == id).ToListAsync(cancellationToken);
            foreach (var vp in videoProgress)
            {
                vp.IsDeleted = true;
            }

            // Cascade Soft Delete: Mark certificates as deleted
            var certificates = await _context.Certificates.Where(c => c.UserId == id).ToListAsync(cancellationToken);
            foreach (var cert in certificates)
            {
                cert.IsDeleted = true;
            }

            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<bool> RestoreUserAsync(int id, int? tenantId, CancellationToken cancellationToken = default)
        {
            var user = await _context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
            if (user == null || !user.IsDeleted || (tenantId.HasValue && tenantId.Value != 0 && user.TenantId != tenantId.Value)) return false;

            user.IsDeleted = false;
            _context.Users.Update(user);

            // Cascade Restore: Restore roles
            var roles = await _context.UserRoles.IgnoreQueryFilters().Where(ur => ur.UserId == id && ur.IsDeleted).ToListAsync(cancellationToken);
            foreach (var r in roles)
            {
                r.IsDeleted = false;
            }

            // Cascade Restore: Restore course enrollments
            var enrollments = await _context.UserCourses.IgnoreQueryFilters().Where(uc => uc.UserId == id && uc.IsDeleted).ToListAsync(cancellationToken);
            foreach (var e in enrollments)
            {
                e.IsDeleted = false;
            }

            // Cascade Restore: Restore video progress
            var videoProgress = await _context.UserVideoProgresses.IgnoreQueryFilters().Where(uvp => uvp.UserId == id && uvp.IsDeleted).ToListAsync(cancellationToken);
            foreach (var vp in videoProgress)
            {
                vp.IsDeleted = false;
            }

            // Cascade Restore: Restore certificates
            var certificates = await _context.Certificates.IgnoreQueryFilters().Where(c => c.UserId == id && c.IsDeleted).ToListAsync(cancellationToken);
            foreach (var cert in certificates)
            {
                cert.IsDeleted = false;
            }

            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<User?> GetAdminUserByTenantIdAsync(int tenantId, CancellationToken cancellationToken = default)
        {
            // Specifically look for someone with the ORGANIZATION_ADMIN role first
            var adminUser = await _context.UserRoles
                .IgnoreQueryFilters()
                .Include(ur => ur.User)
                .Include(ur => ur.Role)
                .Where(ur => ur.User.TenantId == tenantId && !ur.User.IsDeleted && !ur.IsDeleted && !ur.Role.IsDeleted && (ur.Role.Code == "ORGANIZATION_ADMIN" || ur.Role.Code == "ADMIN"))
                .Select(ur => ur.User)
                .FirstOrDefaultAsync(cancellationToken);

            if (adminUser != null) return adminUser;

            // Fallback to the first created user for that tenant (Ignore tenancy filter but respect soft-delete)
            return await _context.Users
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(u => u.TenantId == tenantId && !u.IsDeleted)
                .OrderBy(u => u.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<bool> IsUserOrgAdminAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _context.UserRoles
                .Include(ur => ur.Role)
                .AnyAsync(ur => ur.UserId == userId && (ur.Role.Code == "ORGANIZATION_ADMIN" || ur.Role.Code == "SUPER_ADMIN") && !ur.IsDeleted, cancellationToken);
        }

        public async Task<bool> IsUserSuperAdminAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _context.UserRoles
                .Include(ur => ur.Role)
                .AnyAsync(ur => ur.UserId == userId && ur.Role.Code == "SUPER_ADMIN" && !ur.IsDeleted, cancellationToken);
        }

        public async Task BulkUpdateGroupAsync(int groupId, List<int> userIds, int? tenantId, CancellationToken cancellationToken = default)
        {
            // 1. Clear GroupId for users who were in this group but are no longer in the list
            var usersToClear = await _context.Users
                .Where(u => u.GroupId == groupId && (!tenantId.HasValue || u.TenantId == tenantId.Value) && !userIds.Contains(u.Id))
                .ToListAsync(cancellationToken);

            foreach (var user in usersToClear)
            {
                user.GroupId = null;
            }

            // 2. Set GroupId for users in the list
            var usersToAssign = await _context.Users
                .Where(u => userIds.Contains(u.Id) && (!tenantId.HasValue || u.TenantId == tenantId.Value))
                .ToListAsync(cancellationToken);

            foreach (var user in usersToAssign)
            {
                user.GroupId = groupId;
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> AnyUsersInTenantAsync(int tenantId, CancellationToken cancellationToken = default)
        => await _context.Users.AnyAsync(u => u.TenantId == tenantId && !u.IsDeleted, cancellationToken);

        public async Task<bool> AnyUsersInGroupAsync(int groupId, CancellationToken cancellationToken = default)
        => await _context.Users.AnyAsync(u => u.GroupId == groupId && !u.IsDeleted, cancellationToken);
        
        public async Task UnassignUsersFromGroupAsync(int groupId, CancellationToken cancellationToken = default)
        {
            var users = await _context.Users.Where(u => u.GroupId == groupId).ToListAsync(cancellationToken);
            foreach (var user in users)
            {
                user.GroupId = null;
            }
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> AddUserWithSecurityAsync(User user, List<int> roleIds, int? groupId, bool isActive = true, CancellationToken cancellationToken = default)
        {
            try
            {
                user.GroupId = groupId;
                user.IsActive = isActive; 
                await _context.Users.AddAsync(user, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                
                if (roleIds != null && roleIds.Any())
                {
                    var uniqueRoleIds = roleIds.Distinct().ToList();
                    foreach (var roleId in uniqueRoleIds)
                    {
                        var userRole = new UserRole
                        {
                            UserId = user.Id, 
                            RoleId = roleId,
                            TenantId = user.TenantId,
                            IsActive = true
                        };
                        await _context.UserRoles.AddAsync(userRole, cancellationToken);
                    }
                    await _context.SaveChangesAsync(cancellationToken);
                }

               
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AddUserWithSecurityAsync Error] {ex}");
                return false;
            }
        }

        public async Task SaveAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
    }
}


using AutoMapper;
using LMS_SoulCode.Features.Auth.Models;
using LMS_SoulCode.Data;
using Microsoft.EntityFrameworkCore;
using LMS_SoulCode.Features.Auth.DTOs;
using LMS_SoulCode.Features.Common;
using LMS_SoulCode.Features.Common.Repositories;

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
                .AsNoTracking()
                .Include(u => u.Organization) // Include Org info
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
                filter: u => (!tenantId.HasValue || u.TenantId == tenantId.Value) &&
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
                        .Where(ur => ur.UserId == u.Id && ur.IsActive && ur.Role.IsActive)
                        .Select(ur => ur.Role.Name)
                        .Take(3)) ?? "No Role",
                    TenantId = u.TenantId,
                    OrgName = u.Organization != null ? u.Organization.Name : null,
                    GroupId = u.GroupId,
                    GroupName = u.Group != null ? u.Group.GroupName : null,
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

            if (tenantId.HasValue)
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
                        .Where(ur => ur.UserId == u.Id && ur.IsActive && ur.Role.IsActive)
                        .Select(ur => ur.Role.Name)
                        .Take(3)) // Limit to 3 roles to avoid very long strings
                        ?? "No Role", // Show all active roles, comma-separated
                    TenantId = u.TenantId,
                    OrgName = u.Organization != null ? u.Organization.Name : null,
                    GroupId = u.GroupId,
                    GroupName = u.Group != null ? u.Group.GroupName : null,
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
                
            if (user == null || (tenantId.HasValue && user.TenantId != tenantId.Value)) return null;

            // Use AutoMapper to update only non-null properties
            _mapper.Map(request, user);

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
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
            if (user == null || user.IsDeleted || (tenantId.HasValue && user.TenantId != tenantId.Value)) return false;

            user.IsDeleted = true;
            _context.Users.Update(user);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<bool> RestoreUserAsync(int id, int? tenantId, CancellationToken cancellationToken = default)
        {
            var user = await _context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
            if (user == null || !user.IsDeleted || (tenantId.HasValue && user.TenantId != tenantId.Value)) return false;

            user.IsDeleted = false;
            // DeletedAt will be cleared automatically by DbContext SaveChangesAsync override
            _context.Users.Update(user);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<User?> GetAdminUserByTenantIdAsync(int tenantId, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.TenantId == tenantId && !u.IsDeleted)
                .OrderBy(u => u.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task SaveAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    }
}

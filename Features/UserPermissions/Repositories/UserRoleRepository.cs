using LMS_SoulCode.Data;
using LMS_SoulCode.Features.UserPermissions.Models;
using Microsoft.EntityFrameworkCore;
using LMS_SoulCode.Features.Common.Repositories;

namespace LMS_SoulCode.Features.UserPermissions.Repositories
{
    public class UserRoleRepository : BaseRepository<UserRole>, IUserRoleRepository
    {
        public UserRoleRepository(LmsDbContext context) : base(context)
        {
        }

        public async Task<List<UserRole>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default) =>
            await _context.UserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == userId)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

        public async Task<bool> AnyWithRoleAsync(int roleId, CancellationToken cancellationToken = default)
            => await _context.UserRoles.AnyAsync(ur => ur.RoleId == roleId && ur.IsActive && !ur.IsDeleted, cancellationToken);
    }
}

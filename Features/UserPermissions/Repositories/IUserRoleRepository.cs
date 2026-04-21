using LMS_SoulCode.Features.UserPermissions.Models;
using LMS_SoulCode.Features.Common.Repositories;

namespace LMS_SoulCode.Features.UserPermissions.Repositories
{
    public interface IUserRoleRepository : IBaseRepository<UserRole>
    {
        Task<List<UserRole>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
        Task<bool> AnyWithRoleAsync(int roleId, CancellationToken cancellationToken = default);
    }
}

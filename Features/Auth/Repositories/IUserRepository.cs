
using LMS_SoulCode.Features.Auth.DTOs;
using LMS_SoulCode.Features.Auth.Models;
using LMS_SoulCode.Features.Common.Repositories;

namespace LMS_SoulCode.Features.Auth.Repositories
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> IsEmailTakenAsync(string email, int? tenantId = null, CancellationToken cancellationToken = default);
        Task<bool> IsMobileTakenAsync(string mobile, int? tenantId = null, CancellationToken cancellationToken = default);
        // Task AddAsync(User user); // Removed as it is in BaseRepository
        Task<string?> GenerateResetTokenAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> ValidateResetTokenAsync(string token, CancellationToken cancellationToken = default);
        Task UpdatePasswordAsync(string email, string newPassword, CancellationToken cancellationToken = default);
        Task<string?> GenerateRefreshTokenAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> ValidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
        Task UpdateRefreshTokenAsync(string email, string newRefreshToken, CancellationToken cancellationToken = default);
        Task<User?> GetUserByResetTokenAsync(string token, CancellationToken cancellationToken = default);
        Task UpdatePasswordAsync(User user, CancellationToken cancellationToken = default);
        Task<(IEnumerable<UserDto> Items, int TotalCount)> GetUsersAsync(string? searchTerm, int pageNumber, int pageSize, int? tenantId, CancellationToken cancellationToken);
        Task<UserDto?> GetUserByIdAsync(int id, int? tenantId, CancellationToken cancellationToken = default);
        Task<UserDto?> UpdateUserAsync(int id, UpdateUserRequest request, int? tenantId, CancellationToken cancellationToken = default);
        Task<bool> SoftDeleteUserAsync(int id, int? tenantId, CancellationToken cancellationToken = default);
        Task<bool> RestoreUserAsync(int id, int? tenantId, CancellationToken cancellationToken = default);
        Task<User?> GetAdminUserByTenantIdAsync(int tenantId, CancellationToken cancellationToken = default);
        Task<bool> IsUserOrgAdminAsync(int userId, CancellationToken cancellationToken = default);
        Task BulkUpdateGroupAsync(int groupId, List<int> userIds, int? tenantId, CancellationToken cancellationToken = default);
        Task<bool> AnyUsersInTenantAsync(int tenantId, CancellationToken cancellationToken = default);
        Task<bool> AnyUsersInGroupAsync(int groupId, CancellationToken cancellationToken = default);
        Task UnassignUsersFromGroupAsync(int groupId, CancellationToken cancellationToken = default);
        Task<bool> AddUserWithSecurityAsync(User user, List<int> roleIds, int? groupId, bool isActive = true, CancellationToken cancellationToken = default);
        Task SaveAsync(CancellationToken cancellationToken = default);
    }
}

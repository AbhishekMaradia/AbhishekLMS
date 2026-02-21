using AutoMapper;
using AutoMapper.QueryableExtensions;
using LMS_SoulCode.Data;
using LMS_SoulCode.Features.UserPermissions.Models;
using Microsoft.EntityFrameworkCore;
using static LMS_SoulCode.Features.UserPermissions.DTOs.RoleDtos;
using LMS_SoulCode.Features.Common;
using LMS_SoulCode.Features.Common.Repositories;

namespace LMS_SoulCode.Features.UserPermissions.Repositories
{
    public class RoleRepository : BaseRepository<Role>, IRoleRepository
    {
        private readonly IMapper _mapper;

        public RoleRepository(LmsDbContext context, IMapper mapper) : base(context)
        {
            _mapper = mapper;
        }

        public async Task<(IEnumerable<GetRoleDto> Items, int TotalCount)> GetRolesAsync(string? searchTerm, int pageNumber, int pageSize, bool? isActive, int? tenantId, CancellationToken cancellationToken)
        {
            return await GetPagedAsync<GetRoleDto>(
                pageNumber,
                pageSize,
                filter: r => (!tenantId.HasValue || (r.TenantId == tenantId.Value || (r.TenantId == null && r.IsDefault))) &&
                            (isActive.HasValue ? r.IsActive == isActive.Value : r.IsActive) &&
                            (string.IsNullOrWhiteSpace(searchTerm) || 
                             r.Name.ToLower().Contains(searchTerm.ToLower()) || 
                             r.Code.ToLower().Contains(searchTerm.ToLower())),
                projection: q => q.AsNoTracking().OrderByDescending(r => r.Id).ProjectTo<GetRoleDto>(_mapper.ConfigurationProvider),
                cancellationToken: cancellationToken
            );
        }

        public async Task<GetRoleDto?> GetDtoByIdAsync(int id, CancellationToken cancellationToken = default) =>
            await _context.Roles
                .AsNoTracking()
                .Where(r => r.Id == id && r.IsActive) // Added IsActive filter
                .ProjectTo<GetRoleDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

        public async Task<Role?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
            await _context.Roles.FirstOrDefaultAsync(r => r.Code == code && !r.IsDeleted, cancellationToken);

        public async Task<int> CascadeRoleIsActiveToUserRolesAsync(int roleId, bool isActive, CancellationToken cancellationToken = default)
        {
            // Use ExecuteUpdateAsync for better performance (bulk update)
            // This is more efficient than loading all records into memory
            var affectedRows = await _context.UserRoles
                .Where(ur => ur.RoleId == roleId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(ur => ur.IsActive, isActive), cancellationToken);

            return affectedRows;
        }
    }
}

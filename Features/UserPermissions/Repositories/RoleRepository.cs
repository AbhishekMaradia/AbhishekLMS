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
            var query = _context.Roles.IgnoreQueryFilters().Where(r => !r.IsDeleted && r.Code != "SUPER_ADMIN");

            if (tenantId.HasValue && tenantId.Value != 0)
            {
                query = query.Where(r => r.TenantId == tenantId.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(r => r.IsActive == isActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerSearch = searchTerm.ToLower();
                query = query.Where(r => r.Name.ToLower().Contains(lowerSearch) || r.Code.ToLower().Contains(lowerSearch));
            }

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query.AsNoTracking()
                .OrderByDescending(r => r.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new GetRoleDto(
                    r.Id,
                    r.Code,
                    r.Name,
                    r.IsActive,
                    r.IsDefault,
                    r.TenantId,
                    _context.Organizations.IgnoreQueryFilters().Where(o => o.Id == r.TenantId).Select(o => o.Name).FirstOrDefault() ?? "Super Admin"
                ))
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<GetRoleDto?> GetDtoByIdAsync(int id, CancellationToken cancellationToken = default) =>
            await _context.Roles
                .AsNoTracking()
                .Where(r => r.Id == id)
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

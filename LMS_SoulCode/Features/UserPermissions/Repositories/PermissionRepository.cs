using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using LMS_SoulCode.Features.Common.Repositories;
using LMS_SoulCode.Data;
using LMS_SoulCode.Features.UserPermissions.Models;
using Microsoft.EntityFrameworkCore;
using static LMS_SoulCode.Features.UserPermissions.DTOs.PermissionDtos;
using LMS_SoulCode.Features.Common;

namespace LMS_SoulCode.Features.UserPermissions.Repositories
{
    public class PermissionRepository : BaseRepository<Permission>, IPermissionRepository
    {
        private readonly IMapper _mapper;

        public PermissionRepository(LmsDbContext context, IMapper mapper) : base(context)
        {
            _mapper = mapper;
        }

        public async Task<(IEnumerable<GetPermissionDto> Items, int TotalCount)> GetPermissionsAsync(string? searchTerm, int pageNumber, int pageSize, bool? isActive, CancellationToken cancellationToken)
        {
            return await GetPagedAsync<GetPermissionDto>(
                pageNumber,
                pageSize,
                filter: p => (isActive.HasValue ? p.IsActive == isActive.Value : (p.IsActive ?? true)) &&
                            (string.IsNullOrWhiteSpace(searchTerm) || 
                             p.Name.ToLower().Contains(searchTerm.ToLower()) || 
                             p.Code.ToLower().Contains(searchTerm.ToLower())),
                projection: q => q.AsNoTracking().OrderByDescending(p => p.Id).ProjectTo<GetPermissionDto>(_mapper.ConfigurationProvider),
                cancellationToken: cancellationToken
            );
        }

        public async Task<GetPermissionDto?> GetDtoByIdAsync(int id, CancellationToken cancellationToken = default) =>
            await _context.Permissions
                .AsNoTracking()
                .Where(p => p.Id == id && (p.IsActive ?? true)) // Added IsActive filter
                .ProjectTo<GetPermissionDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);
    }
}

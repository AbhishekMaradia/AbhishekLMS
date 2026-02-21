using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using LMS_SoulCode.Features.Common.Repositories;
using LMS_SoulCode.Data;
using LMS_SoulCode.Features.Common;
using LMS_SoulCode.Features.UserPermissions.Models;
using LMS_SoulCode.Features.UserPermissions.DTOs;
using Microsoft.EntityFrameworkCore;
using static LMS_SoulCode.Features.UserPermissions.DTOs.ModuleDtos;

namespace LMS_SoulCode.Features.UserPermissions.Repositories
{
    public class ModuleRepository : BaseRepository<Module>, IModuleRepository
    {
        private readonly IMapper _mapper;

        public ModuleRepository(LmsDbContext context, IMapper mapper) : base(context)
        {
            _mapper = mapper;
        }

        public async Task<List<GetModuleDto>> GetAllDtoAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Modules
                .AsNoTracking()
                .Where(m => m.IsActive ?? true)
                .ProjectTo<GetModuleDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }

        public async Task<GetModuleDto?> GetDtoByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Modules
                .AsNoTracking()
                .Where(m => m.Id == id && (m.IsActive ?? true))
                .ProjectTo<GetModuleDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<(IEnumerable<GetModuleDto> Items, int TotalCount)> GetModulesAsync(
            string? searchTerm, 
            int pageNumber, 
            int pageSize, 
            bool? isActive, 
            CancellationToken cancellationToken)
        {
            return await GetPagedAsync<GetModuleDto>(
                pageNumber,
                pageSize,
                filter: m => (isActive.HasValue ? m.IsActive == isActive.Value : (m.IsActive ?? true)) &&
                            (string.IsNullOrWhiteSpace(searchTerm) || 
                             m.Name.ToLower().Contains(searchTerm.ToLower()) || 
                             m.Code.ToLower().Contains(searchTerm.ToLower())),
                projection: q => q.AsNoTracking().OrderByDescending(m => m.Id).ProjectTo<GetModuleDto>(_mapper.ConfigurationProvider),
                cancellationToken: cancellationToken
            );
        }

        public async Task LinkPermissionsAsync(int moduleId, List<int> permissionIds, CancellationToken cancellationToken = default)
        {
            var existing = await _context.ModulePermissions
                .Where(mp => mp.ModuleId == moduleId)
                .ToListAsync(cancellationToken);
            
            _context.ModulePermissions.RemoveRange(existing);

            var newLinks = permissionIds.Select(pid => new ModulePermission
            {
                ModuleId = moduleId,
                PermissionId = pid
            });

            await _context.ModulePermissions.AddRangeAsync(newLinks, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<PermissionDtos.GetPermissionDto>> GetModulePermissionsAsync(int moduleId, CancellationToken cancellationToken = default)
        {
            return await _context.ModulePermissions
                .AsNoTracking()
                .Where(mp => mp.ModuleId == moduleId)
                .Select(mp => mp.Permission)
                .Where(p => p.IsActive ?? true)
                .ProjectTo<PermissionDtos.GetPermissionDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}

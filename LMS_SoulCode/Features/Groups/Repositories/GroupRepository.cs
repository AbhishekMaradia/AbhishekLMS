using LMS_SoulCode.Data;
using LMS_SoulCode.Features.Groups.Models;
using LMS_SoulCode.Features.Groups.DTOs;
using LMS_SoulCode.Features.Common.Repositories;
using LMS_SoulCode.Features.Common.Pagination; // Assuming this namespace exists for ToPagedListAsync extension
using Microsoft.EntityFrameworkCore;

namespace LMS_SoulCode.Features.Groups.Repositories
{
    public class GroupRepository : BaseRepository<Group>, IGroupRepository
    {
        public GroupRepository(LmsDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Group>> GetGroupsByTenantIdAsync(int? tenantId, CancellationToken cancellationToken = default)
        {
            return await _context.Groups
                .Where(g => g.TenantId == tenantId && !g.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task AddGroupCoursesAsync(IEnumerable<GroupCourse> groupCourses, CancellationToken cancellationToken = default)
        {
            await _context.GroupCourses.AddRangeAsync(groupCourses, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateGroupCoursesAsync(IEnumerable<GroupCourse> groupCourses, CancellationToken cancellationToken = default)
        {
            _context.GroupCourses.UpdateRange(groupCourses);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<(IEnumerable<GroupCourse> Items, int TotalCount)> GetGroupCoursesPagedByGroupIdAsync(int groupId, int? tenantId, string? searchTerm, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            var query = _context.GroupCourses
                .Include(gc => gc.Course)
                .Where(gc => gc.GroupId == groupId && 
                             (!tenantId.HasValue || gc.TenantId == tenantId.Value) && 
                             !gc.IsDeleted);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(gc => gc.Course.Title.ToLower().Contains(searchTerm) || 
                                         (gc.Course.Description != null && gc.Course.Description.ToLower().Contains(searchTerm)));
            }

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(gc => gc.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public override async Task<Group?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Groups
                .Include(g => g.GroupCourses)
                    .ThenInclude(gc => gc.Course)
                .FirstOrDefaultAsync(g => g.Id == id && !g.IsDeleted, cancellationToken);
        }

        public async Task<(IEnumerable<GroupDto> Items, int TotalCount)> GetGroupsPagedAsync(string? searchTerm, int pageNumber, int pageSize, int? tenantId, CancellationToken cancellationToken)
        {
             var query = _context.Groups
                .Include(g => g.GroupCourses)
                    .ThenInclude(gc => gc.Course)
                .AsNoTracking()
                .Where(g => !g.IsDeleted);

             if (tenantId.HasValue)
             {
                 query = query.Where(g => g.TenantId == tenantId.Value);
             }

             if (!string.IsNullOrWhiteSpace(searchTerm))
             {
                 searchTerm = searchTerm.ToLower();
                 query = query.Where(g => g.GroupName.ToLower().Contains(searchTerm));
             }

             var dtoQuery = query
                 .OrderByDescending(g => g.Id)
                 .Select(g => new GroupDto
                 {
                     Id = g.Id,
                     GroupName = g.GroupName,
                     CreatedAt = g.CreatedAt
                 });

            return await dtoQuery.ToPagedListAsync(pageNumber, pageSize, cancellationToken);
        }

        public async Task DeleteGroupCoursesAsync(IEnumerable<GroupCourse> groupCourses, CancellationToken cancellationToken = default)
        {
            foreach (var gc in groupCourses)
            {
                gc.IsDeleted = true;
                gc.DeletedAt = DateTime.UtcNow;
                _context.GroupCourses.Update(gc);
            }
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteGroupCoursesByCourseIdAsync(int courseId, CancellationToken cancellationToken = default)
        {
            var groupCourses = await _context.GroupCourses
                .IgnoreQueryFilters()
                .Where(gc => gc.CourseId == courseId)
                .ToListAsync(cancellationToken);

            if (groupCourses.Any())
            {
                foreach (var gc in groupCourses)
                {
                    gc.IsDeleted = true;
                    gc.DeletedAt = DateTime.UtcNow;
                    _context.GroupCourses.Update(gc);
                }
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}

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

        public async Task UpdateGroupCourseAsync(GroupCourse groupCourse, CancellationToken cancellationToken = default)
        {
            _context.GroupCourses.Update(groupCourse);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateGroupCoursesAsync(IEnumerable<GroupCourse> groupCourses, CancellationToken cancellationToken = default)
        {
            _context.GroupCourses.UpdateRange(groupCourses);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<GroupCourse?> GetGroupCourseByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.GroupCourses
                .Include(gc => gc.Course)
                .Include(gc => gc.Group)
                .FirstOrDefaultAsync(gc => gc.Id == id && !gc.IsDeleted, cancellationToken);
        }

        public async Task<IEnumerable<GroupCourse>> GetGroupCoursesByGroupIdAsync(int groupId, CancellationToken cancellationToken = default)
        {
             return await _context.GroupCourses
                .Include(gc => gc.Course)
                .Where(gc => gc.GroupId == groupId && !gc.IsDeleted)
                .ToListAsync(cancellationToken);
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
                     //GroupCourses = g.GroupCourses.Where(gc => !gc.IsDeleted).Select(gc => new GroupCourseDto
                     //{
                     //    Id = gc.Id,
                     //    GroupId = gc.GroupId,
                     //    CourseId = gc.CourseId,
                     //    CourseName = gc.Course.Title,
                     //    IsEnable = gc.IsEnable
                     //}).ToList()
                 });

            return await dtoQuery.ToPagedListAsync(pageNumber, pageSize, cancellationToken);
        }

        public async Task DeleteGroupCoursesAsync(IEnumerable<GroupCourse> groupCourses, CancellationToken cancellationToken = default)
        {
            _context.GroupCourses.RemoveRange(groupCourses);
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
                _context.GroupCourses.RemoveRange(groupCourses);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}

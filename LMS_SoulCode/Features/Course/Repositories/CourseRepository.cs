using LMS_SoulCode.Data;
using LMS_SoulCode.Features.Course.Models;
using LMS_SoulCode.Features.Common.Repositories;
using LMS_SoulCode.Features.CourseVideos.Models;
using Microsoft.EntityFrameworkCore;
using CourseEntity = LMS_SoulCode.Features.Course.Models.Course;
using LMS_SoulCode.Features.Common;

namespace LMS_SoulCode.Features.Course.Repositories
{
    public class CourseRepository : BaseRepository<CourseEntity>, ICourseRepository
    {
        public CourseRepository(LmsDbContext context) : base(context)
        {
        }
        public async Task<(IEnumerable<CourseEntity> Items, int TotalCount)> GetCoursesAsync(string? searchTerm, int pageNumber, int pageSize, bool? isActive, int? tenantId, CancellationToken cancellationToken)
        => await GetPagedAsync(
                pageNumber, 
                pageSize,
                filter: c => (!tenantId.HasValue || c.TenantId == tenantId) &&
                            (!isActive.HasValue || c.IsActive == isActive) &&
                            (string.IsNullOrWhiteSpace(searchTerm) || 
                             c.Title.ToLower().Contains(searchTerm.ToLower()) || 
                             (c.Description != null && c.Description.ToLower().Contains(searchTerm.ToLower()))),
                queryModifier: q => q.Include(c => c.Videos).AsNoTracking().OrderByDescending(c => c.Id),
                cancellationToken: cancellationToken
            );

        public async Task<(IEnumerable<CourseEntity> Items, int TotalCount)> GetCoursesByUserGroupAsync(int? groupId, string? searchTerm, int pageNumber, int pageSize, int? tenantId, CancellationToken cancellationToken)
        {
            var query = _context.Courses
                .Include(c => c.Videos)
                .AsNoTracking()
                .Where(c => (!tenantId.HasValue || c.TenantId == tenantId) && c.IsActive);

            // 1. If User has a Group -> Show Group Courses
            if (groupId.HasValue)
            {
                query = query.Where(c => _context.GroupCourses
                    .Any(gc => gc.GroupId == groupId.Value && 
                               gc.CourseId == c.Id && 
                               gc.IsEnable == true &&
                               !gc.IsDeleted));
            }
            // 2. If User has NO Group -> Check Organization's Group Usage
            else
            {
                // Check if this Organization has ANY groups created.
                // If 0 Groups -> Assume "Non-Group Org" -> Show All Courses (Open Access)
                // If >0 Groups -> Assume "Group-Based Org" -> Show No Courses (Strict Access)
                
                bool orgHasGroups = false;
                if (tenantId.HasValue)
                {
                    orgHasGroups = await _context.Groups
                        .AnyAsync(g => g.TenantId == tenantId.Value && !g.IsDeleted, cancellationToken);
                }

                if (orgHasGroups)
                {
                    // Org uses groups, but user is not in one -> Access Denied (Empty List)
                    return (Enumerable.Empty<CourseEntity>(), 0);
                }
                
                // Else (orgHasGroups == false) -> Fallthrough to return ALL active courses defined in initial `query`
            }

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(c => c.Title.ToLower().Contains(searchTerm) || 
                                        (c.Description != null && c.Description.ToLower().Contains(searchTerm)));
            }

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(c => c.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }
        public async Task<CourseEntity?> GetByIdAsync(int id, int? tenantId, CancellationToken cancellationToken = default)
        {
            var query = _context.Courses.Where(c => c.Id == id && c.IsActive);
            if (tenantId.HasValue) query = query.Where(c => c.TenantId == tenantId.Value);
            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<CourseEntity?> GetByIdForAdminAsync(int id, int? tenantId, CancellationToken cancellationToken = default)
        {
             var query = _context.Courses.Include(c => c.Videos).Where(c => c.Id == id);
             if (tenantId.HasValue) query = query.Where(c => c.TenantId == tenantId.Value);
             return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<CourseEntity>> GetByCategoryIdAsync(int categoryId, int? tenantId, CancellationToken cancellationToken = default)
        {
            var query = _context.Courses.Where(c => c.CategoryId == categoryId && c.IsActive);
            if (tenantId.HasValue) query = query.Where(c => c.TenantId == tenantId.Value);
            return await query.ToListAsync(cancellationToken);
        }

        public async Task<List<CourseEntity>> GetAllActiveCoursesAsync(int? tenantId, CancellationToken cancellationToken = default)
        {
            var query = _context.Courses.Where(c => c.IsActive);
            if (tenantId.HasValue) query = query.Where(c => c.TenantId == tenantId.Value);
            return await query.ToListAsync(cancellationToken);
        }

        public async Task AddVideoAsync(CourseVideo video, CancellationToken cancellationToken = default)
        {
             _context.CourseVideos.Add(video);
            await _context.SaveChangesAsync(cancellationToken);
        }
        public async Task AddDocsAsync(CourseDocument docs, CancellationToken cancellationToken = default)
        {
            _context.CourseDocuments.Add(docs);
            await _context.SaveChangesAsync(cancellationToken);
        }
        public async Task<IEnumerable<CourseVideo>> GetVideosByCourseIdAsync(int courseId, CancellationToken cancellationToken = default)
        => await _context.CourseVideos
                .Include(v => v.Course)
                .Where(v => v.CourseId == courseId && v.Course.IsActive)
                .ToListAsync(cancellationToken);
        
        public async Task<IEnumerable<CourseDocument>> GetDocsByCourseIdAsync(int courseId, CancellationToken cancellationToken = default)
        => await _context.CourseDocuments
                .Include(d => d.Course)
                .Where(v => v.CourseId == courseId && v.Course.IsActive)
                .ToListAsync(cancellationToken);

        public async Task<CourseDocument?> GetDocumentByIdAsync(int documentId, int? tenantId, CancellationToken cancellationToken = default)
        => await _context.CourseDocuments
                .Include(d => d.Course)
                .Where(d => d.Id == documentId && 
                           (!tenantId.HasValue || d.TenantId == tenantId.Value) && 
                           d.Course.IsActive)
                .FirstOrDefaultAsync(cancellationToken);
        

    }
}

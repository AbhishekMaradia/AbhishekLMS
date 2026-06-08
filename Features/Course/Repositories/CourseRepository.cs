using LMS_SoulCode.Data;
using LMS_SoulCode.Features.Course.Models;
using LMS_SoulCode.Features.Common.Repositories;
using LMS_SoulCode.Features.CourseVideos.Models;
using Microsoft.EntityFrameworkCore;
using CourseEntity = LMS_SoulCode.Features.Course.Models.Course;
using LMS_SoulCode.Features.Course.DTOs;
using LMS_SoulCode.Features.Groups.Models;

namespace LMS_SoulCode.Features.Course.Repositories
{
    public class CourseRepository : BaseRepository<CourseEntity>, ICourseRepository
    {
        public CourseRepository(LmsDbContext context) : base(context)
        {
        }

        public async Task<(IEnumerable<CourseResponse> Items, int TotalCount)> GetCoursesAsync(string? searchTerm, int pageNumber, int pageSize, bool? isActive, int? tenantId, CancellationToken cancellationToken)
        {
            var query = _context.Courses
                .AsNoTracking()
                .Where(c => ((tenantId == null || tenantId == 0) ? true : c.TenantId == tenantId) &&
                            !c.IsDeleted &&
                            (!isActive.HasValue || c.IsActive == isActive) &&
                            (string.IsNullOrWhiteSpace(searchTerm) || 
                             c.Title.ToLower().Contains(searchTerm.ToLower()) || 
                             (c.Description != null && c.Description.ToLower().Contains(searchTerm.ToLower()))));

            return await query
                .OrderByDescending(c => c.Id)
                .Select(c => new CourseResponse
                {
                    CourseId = c.Id,
                    Title = c.Title,
                    CategoryId = c.CategoryId,
                    CourseMainImageUrl = c.CourseMainImageUrl,
                    ThumbnailUrl = c.ThumbnailUrl,
                    Description = c.Description,
                    Instructor = c.Instructor,
                    Difficulty = c.Difficulty,
                    DurationHours = c.DurationHours,
                    Rating = c.Rating,
                    Price = c.Price,
                    IsActive = c.IsActive,
                    TenantId = c.TenantId,
                    OrgName = _context.Organizations.Where(o => o.Id == c.TenantId).Select(o => o.Name).FirstOrDefault() ?? "",
                    VideoUrls = c.Videos.Select(v => v.VideoUrl).ToList()
                })
                .ToPagedListAsync(pageNumber, pageSize, cancellationToken);
        }

        public async Task<(IEnumerable<CourseResponse> Items, int TotalCount)> GetCoursesByUserGroupAsync(int? userId, int? groupId, string? searchTerm, int pageNumber, int pageSize, int? tenantId, CancellationToken cancellationToken)
        {
            var query = _context.Courses
                .AsNoTracking()
                .Where(c => ((tenantId == null || tenantId == 0) ? true : c.TenantId == tenantId) && c.IsActive);

            if (groupId.HasValue || userId.HasValue)
            {
                query = query.Where(c => 
                    (groupId.HasValue && _context.GroupCourses
                        .Any(gc => gc.GroupId == groupId.Value && 
                                   gc.CourseId == c.Id && 
                                   gc.IsEnable == true &&
                                   !gc.IsDeleted)) ||
                    (userId.HasValue && (_context.Set<LMS_SoulCode.Features.SubscribedCourse.Models.UserCourse>()
                        .Any(uc => uc.UserId == userId.Value && 
                                   uc.CourseId == c.Id && 
                                   !uc.IsDeleted) ||
                         _context.UserGroups
                        .Any(ug => ug.UserId == userId.Value &&
                                   !ug.IsDeleted &&
                                   _context.GroupCourses.Any(gc => gc.GroupId == ug.GroupId &&
                                                                   gc.CourseId == c.Id &&
                                                                   gc.IsEnable == true &&
                                                                   !gc.IsDeleted))))
                );
            }
            else
            {
                bool orgHasGroups = false;
                if (tenantId.HasValue)
                {
                    orgHasGroups = await _context.Groups
                        .AnyAsync(g => g.TenantId == tenantId.Value && !g.IsDeleted, cancellationToken);
                }

                if (orgHasGroups)
                {
                    return (Enumerable.Empty<CourseResponse>(), 0);
                }
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(c => c.Title.ToLower().Contains(searchTerm) || 
                                        (c.Description != null && c.Description.ToLower().Contains(searchTerm)));
            }

            return await query
                .OrderByDescending(c => c.Id)
                .Select(c => new CourseResponse
                {
                    CourseId = c.Id,
                    Title = c.Title,
                    CategoryId = c.CategoryId,
                    CourseMainImageUrl = c.CourseMainImageUrl,
                    ThumbnailUrl = c.ThumbnailUrl,
                    Description = c.Description,
                    Instructor = c.Instructor,
                    Difficulty = c.Difficulty,
                    DurationHours = c.DurationHours,
                    Rating = c.Rating,
                    Price = c.Price,
                    IsActive = c.IsActive,
                    TenantId = c.TenantId,
                    OrgName = _context.Organizations.Where(o => o.Id == c.TenantId).Select(o => o.Name).FirstOrDefault() ?? "",
                    VideoUrls = c.Videos.Select(v => v.VideoUrl).ToList()
                })
                .ToPagedListAsync(pageNumber, pageSize, cancellationToken);
        }
        public async Task<CourseEntity?> GetByIdAsync(int id, int? tenantId, CancellationToken cancellationToken = default)
        {
            var query = _context.Courses.Where(c => c.Id == id && c.IsActive);
            if (tenantId.HasValue && tenantId != 0) query = query.Where(c => c.TenantId == tenantId.Value);
            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<CourseEntity?> GetByIdForAdminAsync(int id, int? tenantId, CancellationToken cancellationToken = default)
        {
             var query = _context.Courses.Include(c => c.Videos).Where(c => c.Id == id);
             if (tenantId.HasValue && tenantId != 0) query = query.Where(c => c.TenantId == tenantId.Value);
             return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<CourseEntity>> GetByCategoryIdAsync(int categoryId, int? tenantId, CancellationToken cancellationToken = default)
        {
            var query = _context.Courses.Where(c => c.CategoryId == categoryId && c.IsActive);
            if (tenantId.HasValue && tenantId != 0) query = query.Where(c => c.TenantId == tenantId.Value);
            return await query.ToListAsync(cancellationToken);
        }

        public async Task<List<CourseEntity>> GetAllActiveCoursesAsync(int? tenantId, CancellationToken cancellationToken = default)
        {
            var query = _context.Courses.Where(c => c.IsActive);
            if (tenantId.HasValue && tenantId != 0) query = query.Where(c => c.TenantId == tenantId.Value);
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
        
        public async Task DeleteDocsAsync(CourseDocument doc, CancellationToken cancellationToken = default)
        {
            _context.CourseDocuments.Remove(doc);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> AnyInTenantAsync(int tenantId, CancellationToken cancellationToken = default)
        => await _context.Courses.AnyAsync(c => c.TenantId == tenantId && !c.IsDeleted, cancellationToken);

        public async Task<bool> AnyInCategoryAsync(int categoryId, CancellationToken cancellationToken = default)
        => await _context.Courses.AnyAsync(c => c.CategoryId == categoryId && !c.IsDeleted, cancellationToken);

        public async Task<bool> AnyInGroupAsync(int courseId, CancellationToken cancellationToken = default)
        => await _context.GroupCourses.AnyAsync(gc => gc.CourseId == courseId && !gc.IsDeleted, cancellationToken);

        public async Task<bool> AnyEnrolledAsync(int courseId, CancellationToken cancellationToken = default)
        => await _context.Set<LMS_SoulCode.Features.SubscribedCourse.Models.UserCourse>().AnyAsync(uc => uc.CourseId == courseId && !uc.IsDeleted && uc.IsActive, cancellationToken);
    }
}

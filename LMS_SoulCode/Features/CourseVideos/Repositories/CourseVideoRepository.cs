using LMS_SoulCode.Data;
using LMS_SoulCode.Features.Course.Models;
using LMS_SoulCode.Features.CourseVideos.Models;
using LMS_SoulCode.Features.CourseVideos.DTOs;
using LMS_SoulCode.Features.Common;
using Microsoft.EntityFrameworkCore;

namespace LMS_SoulCode.Features.CourseVideos.Repositories
{
    public class CourseVideoRepository : BaseRepository<CourseVideo>, ICourseVideoRepository
    {
        public CourseVideoRepository(LmsDbContext context) : base(context) { }

        public async Task<IEnumerable<CourseVideo?>> GetByCourseIdAsync(int courseId, int? tenantId, CancellationToken cancellationToken = default)
        => await _dbSet
                .Include(v => v.Course)
                .Where(v => v.CourseId == courseId && v.Course.IsActive && (!tenantId.HasValue || v.Course.TenantId == tenantId.Value))
                .ToListAsync(cancellationToken);

        public async Task<(IEnumerable<CourseVideo> Items, int TotalCount)> GetByCourseIdAsync(
            int courseId, 
            string? searchTerm, 
            int pageNumber, 
            int pageSize, 
            int? tenantId,
            CancellationToken cancellationToken)
        => await GetPagedAsync(
                pageNumber,
                pageSize,
                filter: v => v.CourseId == courseId && v.Course.IsActive && 
                            (!tenantId.HasValue || v.Course.TenantId == tenantId.Value) &&
                            (string.IsNullOrWhiteSpace(searchTerm) || 
                             v.Title.ToLower().Contains(searchTerm.ToLower()) || 
                             v.Description.ToLower().Contains(searchTerm.ToLower())),
                queryModifier: q => q.Include(v => v.Course).AsNoTracking().OrderBy(v => v.Id),
                cancellationToken: cancellationToken
            );

        public async Task<CourseVideo?> GetByIdAsync(int id, int? tenantId, CancellationToken cancellationToken = default)
        => await _dbSet
                .Include(v => v.Course)
                .Where(v => v.Id == id && v.Course.IsActive && (!tenantId.HasValue || v.Course.TenantId == tenantId.Value))
                .FirstOrDefaultAsync(cancellationToken);

        public async Task<IEnumerable<CourseVideo>> GetAllCourseVideoAsync(int? tenantId, CancellationToken cancellationToken = default)
        => await _dbSet
                .Include(v => v.Course)
                .Where(v => v.Course.IsActive && (!tenantId.HasValue || v.Course.TenantId == tenantId.Value))
                .ToListAsync(cancellationToken);

        public async Task<(IEnumerable<CourseVideoDto> Items, int TotalCount)> GetCourseVideosAsync(
            string? searchTerm, 
            int pageNumber, 
            int pageSize, 
            int? courseId, 
            int? tenantId,
            CancellationToken cancellationToken)
        => await GetPagedAsync<CourseVideoDto>(
                pageNumber,
                pageSize,
                filter: cv => cv.Course.IsActive && 
                             (!tenantId.HasValue || cv.Course.TenantId == tenantId.Value) &&
                             (!courseId.HasValue || cv.CourseId == courseId.Value) &&
                             (string.IsNullOrWhiteSpace(searchTerm) || 
                              cv.Title.ToLower().Contains(searchTerm.ToLower()) || 
                              cv.Description.ToLower().Contains(searchTerm.ToLower()) ||
                              cv.Course.Title.ToLower().Contains(searchTerm.ToLower())),
                projection: q => q.AsNoTracking().OrderByDescending(cv => cv.Id).Select(cv => new CourseVideoDto
                {
                    Id = cv.Id,
                    CourseId = cv.CourseId,
                    Title = cv.Title,
                    Description = cv.Description,
                    VideoUrl = cv.VideoUrl,
                    CreatedAt = cv.CreatedAt,
                    CourseName = cv.Course.Title
                }),
                cancellationToken: cancellationToken
            );

        public async Task UpdateProgressAsync(UserVideoProgress progress, CancellationToken cancellationToken = default)
        {
            var existing = await _context.UserVideoProgresses
                .FirstOrDefaultAsync(p => p.UserId == progress.UserId && p.VideoId == progress.VideoId, cancellationToken);

            if (existing == null)
            {
                _context.UserVideoProgresses.Add(progress);
            }
            else
            {
                existing.WatchedPercentage = progress.WatchedPercentage;
                existing.IsCompleted = progress.IsCompleted;
                existing.LastWatchedAt = progress.LastWatchedAt;
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<UserVideoProgress?> GetProgressAsync(int userId, int videoId, int? tenantId, CancellationToken cancellationToken = default)
        {
            var query = _context.UserVideoProgresses.AsQueryable();

            if (tenantId.HasValue)
            {
                query = query.Where(p => p.TenantId == tenantId.Value);
            }

            return await query.FirstOrDefaultAsync(p => p.UserId == userId && p.VideoId == videoId, cancellationToken);
        }
    }
}
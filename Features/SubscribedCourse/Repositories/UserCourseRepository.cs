using LMS_SoulCode.Data;
using LMS_SoulCode.Features.Common;
using LMS_SoulCode.Features.SubscribedCourse.DTOs;
using Microsoft.EntityFrameworkCore;
using LMS_SoulCode.Features.SubscribedCourse.Models;
using LMS_SoulCode.Features.Common.Repositories;

namespace LMS_SoulCode.Features.SubscribedCourse.Repositories
{
    public class UserCourseRepository : BaseRepository<UserCourse>, IUserCourseRepository
    {
        public UserCourseRepository(LmsDbContext context) : base(context) { }

        public async Task<UserCourse?> GetAsync(int userId, int courseId, int? tenantId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(uc => uc.Course)
                .Include(uc => uc.User)
                .Where(x => x.UserId == userId && x.CourseId == courseId && x.Course.IsActive && (!tenantId.HasValue || x.TenantId == tenantId.Value))
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IEnumerable<UserCourse>> GetByUserAsync(int userId, int? tenantId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(uc => uc.Course)
                .Where(x => x.UserId == userId && x.IsActive && x.Course.IsActive && (!tenantId.HasValue || x.TenantId == tenantId.Value))
                .ToListAsync(cancellationToken);
        }

        public async Task SubscribeAsync(UserCourse userCourse, CancellationToken cancellationToken = default)
        {
            var existing = await GetAsync(userCourse.UserId, userCourse.CourseId, userCourse.TenantId, cancellationToken);
            if (existing == null)
            {
                await AddAsync(userCourse, cancellationToken);
            }
            else
            {
                existing.IsActive = true;
                existing.CreatedAt = DateTime.UtcNow; // Using CreatedAt as per new IAuditEntity standard
                await UpdateAsync(existing, cancellationToken);
            }
        }

        public async Task UnsubscribeAsync(UserCourse userCourse, CancellationToken cancellationToken = default)
        {
            var existing = await GetAsync(userCourse.UserId, userCourse.CourseId, userCourse.TenantId, cancellationToken);
            if (existing != null)
            {
                existing.IsActive = false;
                await UpdateAsync(existing, cancellationToken);
            }
        }

        public async Task<bool> IsSubscribedAsync(int userId, int courseId, int? tenantId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(uc => uc.Course)
                .AnyAsync(x => x.UserId == userId && x.CourseId == courseId && x.IsActive && x.Course.IsActive && (!tenantId.HasValue || x.TenantId == tenantId.Value), cancellationToken);
        }

        public async Task<IEnumerable<UserCourse>> GetAllSubscribedAsync(int? tenantId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(x => x.Course)
                .Include(x => x.User)
                .Where(x => x.Course.IsActive && (!tenantId.HasValue || x.TenantId == tenantId.Value))
                .ToListAsync(cancellationToken);
        }

        public async Task<(IEnumerable<UserCourseListDto> Items, int TotalCount)> GetUserCoursesAsync(
            string? searchTerm, 
            int pageNumber, 
            int pageSize, 
            int? userId, 
            int? courseId, 
            DateTime? subscribedFrom, 
            DateTime? subscribedTo, 
            int? tenantId,
            CancellationToken cancellationToken)
        {
            return await GetPagedAsync<UserCourseListDto>(
                pageNumber,
                pageSize,
                filter: uc => uc.IsActive && uc.Course.IsActive &&
                             (!tenantId.HasValue || uc.TenantId == tenantId.Value) &&
                             (!userId.HasValue || uc.UserId == userId.Value) &&
                             (!courseId.HasValue || uc.CourseId == courseId.Value) &&
                             (!subscribedFrom.HasValue || uc.CreatedAt >= subscribedFrom.Value) &&
                             (!subscribedTo.HasValue || uc.CreatedAt <= subscribedTo.Value) &&
                             (string.IsNullOrWhiteSpace(searchTerm) || 
                              uc.User.Email.ToLower().Contains(searchTerm.ToLower()) ||
                              uc.Course.Title.ToLower().Contains(searchTerm.ToLower()) ||
                              uc.Course.Instructor.ToLower().Contains(searchTerm.ToLower())),
                projection: q => q.AsNoTracking().OrderByDescending(uc => uc.CreatedAt).Select(uc => new UserCourseListDto
                {
                    UserId = uc.UserId,
                    UserEmail = uc.User.Email,
                    CourseId = uc.CourseId,
                    CourseTitle = uc.Course.Title,
                    CourseInstructor = uc.Course.Instructor,
                    CoursePrice = uc.Course.Price,
                    CategoryName = uc.Course.Category != null ? uc.Course.Category.CategoryName : "Unknown",
                    UserName = uc.User != null ? uc.User.FirstName + " " + uc.User.LastName : "Unknown",
                    TenantName = (uc.User != null && uc.User.Organization != null) 
                        ? uc.User.Organization.Name 
                        : (uc.TenantId.HasValue && uc.TenantId.Value > 0 
                            ? (_context.Organizations.Where(o => o.Id == uc.TenantId).Select(o => o.Name).FirstOrDefault() ?? "Main Campus")
                            : "Main Campus"),
                    SubscribedAt = uc.CreatedAt
                }),
                cancellationToken: cancellationToken
            );
        }
    }
}

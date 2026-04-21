using LMS_SoulCode.Features.SubscribedCourse.Models;
using LMS_SoulCode.Features.SubscribedCourse.DTOs;
using LMS_SoulCode.Features.Common.Repositories;

namespace LMS_SoulCode.Features.SubscribedCourse.Repositories
{
    public interface IUserCourseRepository
    {
        Task<UserCourse?> GetAsync(int userId, int courseId, int? tenantId, CancellationToken cancellationToken = default);
        Task<IEnumerable<UserCourse>> GetByUserAsync(int userId, int? tenantId, CancellationToken cancellationToken = default);
        Task SubscribeAsync(UserCourse userCourse, CancellationToken cancellationToken = default);
        Task UnsubscribeAsync(UserCourse userCourse, CancellationToken cancellationToken = default);
        Task<bool> IsSubscribedAsync(int userId, int courseId, int? tenantId, CancellationToken cancellationToken = default);
        Task<IEnumerable<UserCourse>> GetAllSubscribedAsync(int? tenantId, CancellationToken cancellationToken = default);
        Task<(IEnumerable<UserCourseListDto> Items, int TotalCount)> GetUserCoursesAsync(string? searchTerm, int pageNumber, int pageSize, int? userId, int? courseId, DateTime? subscribedFrom, DateTime? subscribedTo, int? tenantId, CancellationToken cancellationToken);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}

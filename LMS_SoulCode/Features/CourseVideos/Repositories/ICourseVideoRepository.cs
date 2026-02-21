using LMS_SoulCode.Features.CourseVideos.Models;
using LMS_SoulCode.Features.CourseVideos.DTOs;

namespace LMS_SoulCode.Features.CourseVideos.Repositories
{
    public interface ICourseVideoRepository : LMS_SoulCode.Features.Common.Repositories.IBaseRepository<CourseVideo>
    {
        Task<IEnumerable<CourseVideo?>> GetByCourseIdAsync(int courseId, int? tenantId, CancellationToken cancellationToken = default);
        Task<(IEnumerable<CourseVideo> Items, int TotalCount)> GetByCourseIdAsync(int courseId, string? searchTerm, int pageNumber, int pageSize, int? tenantId, CancellationToken cancellationToken);
        Task<CourseVideo?> GetByIdAsync(int id, int? tenantId, CancellationToken cancellationToken = default);
        Task<IEnumerable<CourseVideo>> GetAllCourseVideoAsync(int? tenantId, CancellationToken cancellationToken = default);
        Task<(IEnumerable<CourseVideoDto> Items, int TotalCount)> GetCourseVideosAsync(string? searchTerm, int pageNumber, int pageSize, int? courseId, int? tenantId, CancellationToken cancellationToken);
        Task UpdateProgressAsync(UserVideoProgress progress, CancellationToken cancellationToken = default);
        Task<UserVideoProgress?> GetProgressAsync(int userId, int videoId, int? tenantId, CancellationToken cancellationToken = default);
    }
}

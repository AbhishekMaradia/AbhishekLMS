using CourseEntity= LMS_SoulCode.Features.Course.Models.Course;
using LMS_SoulCode.Features.Common.Repositories;
using LMS_SoulCode.Features.CourseVideos.Models;

namespace LMS_SoulCode.Features.Course.Repositories
{
    public interface ICourseRepository : IBaseRepository<CourseEntity>
    {
        Task<(IEnumerable<CourseEntity> Items, int TotalCount)> GetCoursesAsync(string? searchTerm, int pageNumber, int pageSize, bool? isActive, int? tenantId, CancellationToken cancellationToken);
        Task<(IEnumerable<CourseEntity> Items, int TotalCount)> GetCoursesByUserGroupAsync(int? userId, int? groupId, string? searchTerm, int pageNumber, int pageSize, int? tenantId, CancellationToken cancellationToken);
        Task<CourseEntity?> GetByIdAsync(int id, int? tenantId, CancellationToken cancellationToken = default);
        Task<CourseEntity?> GetByIdForAdminAsync(int id, int? tenantId, CancellationToken cancellationToken = default);
        Task<List<CourseEntity>> GetByCategoryIdAsync(int categoryId, int? tenantId, CancellationToken cancellationToken = default);
        Task<List<CourseEntity>> GetAllActiveCoursesAsync(int? tenantId, CancellationToken cancellationToken = default);

        //Course Video Connection
        Task AddVideoAsync(CourseVideo video, CancellationToken cancellationToken = default);
        Task AddDocsAsync(CourseDocument docs, CancellationToken cancellationToken = default);
        Task<IEnumerable<CourseVideo>> GetVideosByCourseIdAsync(int courseId, CancellationToken cancellationToken = default);
        Task<IEnumerable<CourseDocument>> GetDocsByCourseIdAsync(int courseId, CancellationToken cancellationToken = default);
        Task<CourseDocument?> GetDocumentByIdAsync(int documentId, int? tenantId, CancellationToken cancellationToken = default);
    }
}

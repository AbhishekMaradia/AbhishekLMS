using LMS_SoulCode.Features.CourseVideos.Models;
using LMS_SoulCode.Features.Common.Repositories;

namespace LMS_SoulCode.Features.CourseVideos.Repositories
{
    public interface ICourseDocumentRepository : IBaseRepository<CourseDocument>
    {
        Task<IEnumerable<CourseDocument>> GetByCourseIdAsync(int courseId, int? tenantId = null, CancellationToken cancellationToken = default);
    }
}

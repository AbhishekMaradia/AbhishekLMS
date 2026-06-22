using LMS_SoulCode.Data;
using LMS_SoulCode.Features.Course.Models;
using LMS_SoulCode.Features.CourseVideos.Models;
using Microsoft.EntityFrameworkCore;

namespace LMS_SoulCode.Features.CourseVideos.Repositories
{
    public class CourseDocumentRepository : BaseRepository<CourseDocument>, ICourseDocumentRepository
    {
        public CourseDocumentRepository(LmsDbContext context) : base(context) { }

        public async Task<IEnumerable<CourseDocument>> GetByCourseIdAsync(int courseId, int? tenantId = null, CancellationToken cancellationToken = default)
        => await _dbSet
                .Include(v => v.Course)
                .Where(v => v.CourseId == courseId && 
                           (!tenantId.HasValue || v.Course.TenantId == tenantId.Value || v.Course.TenantId == null))
                .ToListAsync(cancellationToken);
    }
}

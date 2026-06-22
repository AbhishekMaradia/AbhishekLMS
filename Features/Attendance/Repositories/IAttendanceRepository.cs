using LMS_SoulCode.Features.Attendance.Models;
using LMS_SoulCode.Features.Common.Repositories;

namespace LMS_SoulCode.Features.Attendance.Repositories
{
    public interface IAttendanceRepository : IBaseRepository<Models.Attendance>
    {
        Task<IEnumerable<Models.Attendance>> GetAttendanceByGroupAndCourseAsync(int groupId, int courseId, DateTime date, CancellationToken cancellationToken = default);
        Task BulkUpsertAttendanceAsync(IEnumerable<Models.Attendance> records, CancellationToken cancellationToken = default);
        Task<IEnumerable<Models.Attendance>> GetAllAttendancesAsync(int? tenantId = null, CancellationToken cancellationToken = default);
    }
}

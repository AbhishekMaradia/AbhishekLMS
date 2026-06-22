using LMS_SoulCode.Features.Attendance.DTOs;

namespace LMS_SoulCode.Features.Attendance.Services
{
    public interface IAttendanceService
    {
        Task<bool> SubmitAttendanceAsync(SubmitAttendanceDto dto, CancellationToken cancellationToken = default);
        Task<IEnumerable<AttendanceListDto>> GetAllAttendancesAsync(int? tenantId = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<AttendanceListDto>> GetAttendanceByFiltersAsync(int groupId, int courseId, string date, CancellationToken cancellationToken = default);
        Task<bool> DeleteAttendanceAsync(int id, CancellationToken cancellationToken = default);
    }
}

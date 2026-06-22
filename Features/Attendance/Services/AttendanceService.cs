using LMS_SoulCode.Features.Attendance.DTOs;
using LMS_SoulCode.Features.Attendance.Models;
using LMS_SoulCode.Features.Attendance.Repositories;

namespace LMS_SoulCode.Features.Attendance.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IAttendanceRepository _attendanceRepository;

        public AttendanceService(IAttendanceRepository attendanceRepository)
        {
            _attendanceRepository = attendanceRepository;
        }

        public async Task<bool> SubmitAttendanceAsync(SubmitAttendanceDto dto, CancellationToken cancellationToken = default)
        {
            if (dto == null || dto.Records == null || !dto.Records.Any())
                return false;

            var attendanceDate = DateTime.Parse(dto.StartDate);

            var records = dto.Records.Select(r => new Models.Attendance
            {
                TenantId = (dto.TenantId.HasValue && dto.TenantId.Value > 0) ? dto.TenantId.Value : 0,
                GroupId = dto.GroupId,
                CourseId = dto.CourseId,
                UserId = r.Id,
                AttendanceDate = attendanceDate,
                SessionStartTime = !string.IsNullOrEmpty(r.StartTime) ? r.StartTime : dto.SessionStartTime,
                SessionEndTime = !string.IsNullOrEmpty(r.EndTime) ? r.EndTime : dto.SessionEndTime,
                Status = r.S,
                Remarks = r.D,
                DocumentUrl = r.DocumentUrl,
                ThumbUrl = r.ThumbUrl
            });

            await _attendanceRepository.BulkUpsertAttendanceAsync(records, cancellationToken);
            return true;
        }

        public async Task<IEnumerable<AttendanceListDto>> GetAllAttendancesAsync(int? tenantId = null, CancellationToken cancellationToken = default)
        {
            var records = await _attendanceRepository.GetAllAttendancesAsync(tenantId, cancellationToken);
            
            return records.Select(a => new AttendanceListDto
            {
                Id = a.Id,
                StudentName = a.User != null ? $"{a.User.FirstName} {a.User.LastName}" : "Unknown",
                OrgName = a.User?.Organization?.Name ?? "Global",
                GroupId = a.GroupId,
                CourseId = a.CourseId,
                GroupName = a.Group?.GroupName ?? "Unknown",
                CourseName = a.Course?.Title ?? "Unknown",
                AttendanceDate = a.AttendanceDate,
                StartTime = a.SessionStartTime,
                EndTime = a.SessionEndTime,
                Status = a.Status,
                Remarks = a.Remarks,
                DocumentUrl = a.DocumentUrl,
                ThumbUrl = a.ThumbUrl
            });
        }
        public async Task<IEnumerable<AttendanceListDto>> GetAttendanceByFiltersAsync(int groupId, int courseId, string date, CancellationToken cancellationToken = default)
        {
            var attendanceDate = DateTime.Parse(date);
            var records = await _attendanceRepository.GetAttendanceByGroupAndCourseAsync(groupId, courseId, attendanceDate, cancellationToken);
            
            return records.Select(a => new AttendanceListDto
            {
                Id = a.UserId, // Note: We use UserId here to match students in frontend
                StudentName = "", // Not needed for the mark view match
                GroupName = "",
                CourseName = "",
                AttendanceDate = a.AttendanceDate,
                StartTime = a.SessionStartTime,
                EndTime = a.SessionEndTime,
                Status = a.Status,
                Remarks = a.Remarks
            });
        }

        public async Task<bool> DeleteAttendanceAsync(int id, CancellationToken cancellationToken = default)
        {
            var record = await _attendanceRepository.GetByIdAsync(id, cancellationToken);
            if (record == null) return false;
            
            await _attendanceRepository.DeleteAsync(id, cancellationToken);
            return true;
        }
    }
}

namespace LMS_SoulCode.Features.Attendance.DTOs
{
    public class SubmitAttendanceDto
    {
        public int? TenantId { get; set; }
        public int GroupId { get; set; }
        public int CourseId { get; set; }
        public string StartDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;
        public string? SessionStartTime { get; set; }
        public string? SessionEndTime { get; set; }
        public List<AttendanceItemDto> Records { get; set; } = new();
    }

    public class AttendanceItemDto
    {
        public int Id { get; set; } // UserId
        public string S { get; set; } = string.Empty; // Status (Present, Absent, etc.)
        public string? D { get; set; } // Description/Remarks
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
        public string? DocumentUrl { get; set; }
        public string? ThumbUrl { get; set; }
    }
}

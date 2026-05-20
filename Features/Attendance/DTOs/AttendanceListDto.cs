using System;

namespace LMS_SoulCode.Features.Attendance.DTOs
{
    public class AttendanceListDto
    {
        public int Id { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string OrgName { get; set; } = string.Empty;
        public int? GroupId { get; set; }
        public int? CourseId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public DateTime AttendanceDate { get; set; }
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public string? DocumentUrl { get; set; }
        public string? ThumbUrl { get; set; }
    }
}

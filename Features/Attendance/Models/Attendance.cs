using LMS_SoulCode.Features.Common.Models;
using LMS_SoulCode.Features.Groups.Models;
using LMS_SoulCode.Features.Course.Models;
using LMS_SoulCode.Features.Auth.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS_SoulCode.Features.Attendance.Models
{
    [Table("Attendance")]
    public class Attendance : BaseTenantEntity
    {
        public int GroupId { get; set; }
        public int CourseId { get; set; }
        public int UserId { get; set; }
        public DateTime AttendanceDate { get; set; }
        public string? SessionStartTime { get; set; }
        public string? SessionEndTime { get; set; }
        public string Status { get; set; } = "Present"; // Present, Late, Excused, Absent
        public string? Remarks { get; set; }
        public string? DocumentUrl { get; set; }
        public string? ThumbUrl { get; set; }

        [ForeignKey("GroupId")]
        public virtual Group Group { get; set; } = null!;

        [ForeignKey("CourseId")]
        public virtual Course.Models.Course Course { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}

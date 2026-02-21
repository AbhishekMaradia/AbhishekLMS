using LMS_SoulCode.Features.Common.Models;
using LMS_SoulCode.Features.Course.Models;

namespace LMS_SoulCode.Features.Groups.Models
{
    public class GroupCourse : BaseTenantEntity
    {
        public int GroupId { get; set; }
        public Group Group { get; set; } = null!;

        public int CourseId { get; set; }
        public LMS_SoulCode.Features.Course.Models.Course Course { get; set; } = null!;

        public bool IsEnable { get; set; } = false;
    }
}

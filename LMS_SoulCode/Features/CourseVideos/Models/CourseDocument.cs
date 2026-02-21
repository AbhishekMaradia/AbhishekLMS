using LMS_SoulCode.Features.Common.Models;
using CourseEntity = LMS_SoulCode.Features.Course.Models.Course;

namespace LMS_SoulCode.Features.CourseVideos.Models
{
    public class CourseDocument : BaseTenantEntity
    {
        public int CourseId { get; set; }
        public string DocName { get; set; } = string.Empty;
        public string DocUrl { get; set; } = string.Empty;

        public CourseEntity Course { get; set; }
    }
}



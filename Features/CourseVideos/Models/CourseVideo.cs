using LMS_SoulCode.Features.Common.Models;
using CourseEntity = LMS_SoulCode.Features.Course.Models.Course;

namespace LMS_SoulCode.Features.CourseVideos.Models
{
    public class CourseVideo : BaseTenantEntity
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string VideoUrl { get; set; } = string.Empty;
        public CourseEntity Course { get; set; }
    }
}



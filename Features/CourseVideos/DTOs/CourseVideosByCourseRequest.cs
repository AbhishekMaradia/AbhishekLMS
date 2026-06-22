using LMS_SoulCode.Features.Common;

namespace LMS_SoulCode.Features.CourseVideos.DTOs
{
    public class CourseVideosByCourseRequest : BasePagedRequest
    {
        public int CourseId { get; set; }
    }
}
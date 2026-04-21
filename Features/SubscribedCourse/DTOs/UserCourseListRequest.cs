using LMS_SoulCode.Features.Common;

namespace LMS_SoulCode.Features.SubscribedCourse.DTOs
{
    public class UserCourseListRequest : BasePagedRequest
    {
        public string? SearchTerm { get; set; }
        public int? UserId { get; set; }
        public int? CourseId { get; set; }
        public DateTime? SubscribedFrom { get; set; }
        public DateTime? SubscribedTo { get; set; }
        public int? TenantId { get; set; }  // Optional - for SuperAdmin to specify org
    }
}
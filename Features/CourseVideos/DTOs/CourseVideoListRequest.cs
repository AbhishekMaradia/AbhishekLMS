using LMS_SoulCode.Features.Common;

namespace LMS_SoulCode.Features.CourseVideos.DTOs
{
    public class CourseVideoListRequest : BasePagedRequest
    {
        public string? SearchTerm { get; set; }
        public int? CourseId { get; set; }
        public int? TenantId { get; set; }  // Optional - for SuperAdmin to specify org
    }
}
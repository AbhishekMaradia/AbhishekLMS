using LMS_SoulCode.Features.Common;

namespace LMS_SoulCode.Features.Course.DTOs
{
    public class CourseListRequest : BasePagedRequest
    {
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; } // null = all, true = active only, false = inactive only
        public int? TenantId { get; set; }  // Optional - for SuperAdmin to specify org
    }
}

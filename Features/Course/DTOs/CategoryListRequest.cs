using LMS_SoulCode.Features.Common;

namespace LMS_SoulCode.Features.Course.DTOs
{
    public class CategoryListRequest : BasePagedRequest
    {
        public string? SearchTerm { get; set; }
        public int? TenantId { get; set; }  // Optional - for SuperAdmin to specify org
    }
}
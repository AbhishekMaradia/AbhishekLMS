using LMS_SoulCode.Features.Common;

namespace LMS_SoulCode.Features.Auth.DTOs
{
    public class UserListRequest : BasePagedRequest
    {
        public string? SearchTerm { get; set; }
        public int? TenantId { get; set; }  // Optional - for SuperAdmin to specify org
    }
}

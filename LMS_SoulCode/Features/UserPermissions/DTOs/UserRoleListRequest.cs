using LMS_SoulCode.Features.Common;

namespace LMS_SoulCode.Features.UserPermissions.DTOs
{
    public class UserRoleListRequest : BasePagedRequest
    {
        public string? SearchTerm { get; set; }
        public int? UserId { get; set; }
        public int? RoleId { get; set; }
        public bool? IsActive { get; set; }
        public int? TenantId { get; set; }  // Optional - for SuperAdmin to specify org
    }
}

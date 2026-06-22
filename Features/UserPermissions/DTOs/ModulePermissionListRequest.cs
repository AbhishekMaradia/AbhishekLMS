using LMS_SoulCode.Features.Common;

namespace LMS_SoulCode.Features.UserPermissions.DTOs
{
    public class ModulePermissionListRequest : BasePagedRequest
    {
        public int? ModuleId { get; set; }
        public int? PermissionId { get; set; }
        public int? TenantId { get; set; }
    }
}
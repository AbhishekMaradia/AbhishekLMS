using LMS_SoulCode.Features.Common;

namespace LMS_SoulCode.Features.UserPermissions.DTOs
{
    public class RoleModulePermissionListRequest : BasePagedRequest
    {
        public int? RoleId { get; set; }
        public int? ModuleId { get; set; }
        public int? PermissionId { get; set; }
        public int? RoleModuleId { get; set; }
        public int? TenantId { get; set; }
    }
}
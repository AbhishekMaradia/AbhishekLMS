using LMS_SoulCode.Features.Common.Models;

namespace LMS_SoulCode.Features.UserPermissions.Models
{
    public class RoleModulePermission : BaseTenantEntity
    {
        public int RoleModuleId { get; set; }
        public int PermissionId { get; set; }
        public bool IsActive { get; set; } = true;

        public RoleModule RoleModule { get; set; } = null!;
        public Permission Permission { get; set; } = null!;
    }
}

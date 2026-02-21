using LMS_SoulCode.Features.Common.Models;

namespace LMS_SoulCode.Features.UserPermissions.Models
{
    public class ModulePermission : BaseTenantEntity
    {
        public int ModuleId { get; set; }
        public int PermissionId { get; set; }

        public Module Module { get; set; } = null!;
        public Permission Permission { get; set; } = null!;
    }
}

using LMS_SoulCode.Features.Common.Models;

namespace LMS_SoulCode.Features.UserPermissions.Models
{
    public class RoleModule : BaseTenantEntity
    {
        public int RoleId { get; set; }
        public int ModuleId { get; set; }
        public bool IsActive { get; set; } = true;

        public Role Role { get; set; } = null!;
        public Module Module { get; set; } = null!;
    }
}

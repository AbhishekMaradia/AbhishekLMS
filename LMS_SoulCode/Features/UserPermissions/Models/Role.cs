using LMS_SoulCode.Features.Common.Models;

namespace LMS_SoulCode.Features.UserPermissions.Models
{
    public class Role : BaseTenantEntity
    {
        public string Code { get; set; } = null!;   // ADMIN
        public string Name { get; set; } = null!;   // Admin
        public bool IsActive { get; set; } = true;
        public bool IsDefault { get; set; } = false; // System roles that cannot be deleted
    }
}

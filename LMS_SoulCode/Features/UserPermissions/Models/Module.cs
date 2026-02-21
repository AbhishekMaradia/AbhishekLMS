using LMS_SoulCode.Features.Common.Models;

namespace LMS_SoulCode.Features.UserPermissions.Models
{
    public class Module : BaseTenantEntity
    {
        public string Code { get; set; } = null!;   // COURSE
        public string Name { get; set; } = null!;
        public bool? IsActive { get; set; } = true;
    }
}

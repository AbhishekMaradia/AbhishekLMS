using LMS_SoulCode.Features.Common.Models;

namespace LMS_SoulCode.Features.UserPermissions.Models
{
    public class Permission : BaseTenantEntity
    {
        public string Code { get; set; } = null!;   // COURSE_ADD
        public string Name { get; set; } = null!;
        public bool? IsActive { get; set; }
    }
}

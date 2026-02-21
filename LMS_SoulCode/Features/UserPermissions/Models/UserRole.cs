using LMS_SoulCode.Features.Auth.Models;
using LMS_SoulCode.Features.Common.Models;

namespace LMS_SoulCode.Features.UserPermissions.Models
{
    public class UserRole : BaseTenantEntity
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public bool IsActive { get; set; } = true;
        public User User { get; set; } = null!;
        public Role Role { get; set; } = null!;
    }
}

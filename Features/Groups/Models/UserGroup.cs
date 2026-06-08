using LMS_SoulCode.Features.Common.Models;
using LMS_SoulCode.Features.Auth.Models;

namespace LMS_SoulCode.Features.Groups.Models
{
    public class UserGroup : BaseTenantEntity
    {
        public int GroupId { get; set; }
        public Group Group { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}

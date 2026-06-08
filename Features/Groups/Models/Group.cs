using LMS_SoulCode.Features.Common.Models;

namespace LMS_SoulCode.Features.Groups.Models
{
    public class Group : BaseTenantEntity
    {
        public string GroupName { get; set; } = string.Empty;
        public ICollection<GroupCourse> GroupCourses { get; set; } = new List<GroupCourse>();
        public ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();
    }
}

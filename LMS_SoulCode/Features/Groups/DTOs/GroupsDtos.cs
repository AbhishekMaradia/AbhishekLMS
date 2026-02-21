using LMS_SoulCode.Features.Common;
using LMS_SoulCode.Features.Common.Pagination;

namespace LMS_SoulCode.Features.Groups.DTOs
{
    public class CreateGroupRequest
    {
        public string GroupName { get; set; } = string.Empty;
        public int? TenantId { get; set; }
    }

    public class GroupDto
    {
        public int Id { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public List<GroupCourseDto> GroupCourses { get; set; } = new List<GroupCourseDto>();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? TenantId { get; set; }
    }

    public class GroupCourseDto
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public bool IsEnable { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class GroupListRequest : BasePagedRequest
    {
        public string? SearchTerm { get; set; }
        public int? TenantId { get; set; }  // Optional - for SuperAdmin to specify org
    }

    public class GroupCourseUpdateItem
    {
        public int CourseId { get; set; }
        public bool IsEnable { get; set; }
    }

    public class UpdateGroupRequest
    {
        public int Id { get; set; }
        public string? GroupName { get; set; }
        public List<GroupCourseUpdateItem>? Courses { get; set; } = new List<GroupCourseUpdateItem>();
    }
}

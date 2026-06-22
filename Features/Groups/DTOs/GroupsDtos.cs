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
        public string OrgName { get; set; }
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
        public string OrgName { get; set; }
        public string? ThumbnailUrl { get; set; }
        public int CategoryId { get; set; }
        public bool IsEnable { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? CourseTenantId { get; set; }
        public int? GroupTenantId { get; set; }
    }

    public class GroupListRequest : BasePagedRequest
    {
        public string? SearchTerm { get; set; }
        public int? TenantId { get; set; }  // Optional - for SuperAdmin to specify org
    }

    public class GroupCourseListRequest : BasePagedRequest
    {
        public string? SearchTerm { get; set; }
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
        public int? TenantId { get; set; }
    }

    public class BulkUpdateCoursesRequest
    {
        public int GroupId { get; set; }
        public List<GroupCourseUpdateItem> Courses { get; set; } = new List<GroupCourseUpdateItem>();
    }

    public class BulkAssignUsersRequest
    {
        public int GroupId { get; set; }
        public List<int> UserIds { get; set; } = new List<int>();
    }

    public class GroupUserDto
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsAssigned { get; set; }
        public int? GroupId { get; set; }
        public string? GroupName { get; set; }
        public string? OrgName { get; set; }
        public int? TenantId { get; set; }
    }
}


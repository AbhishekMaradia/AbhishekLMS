namespace LMS_SoulCode.Features.Auth.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Mobile { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string UserRole { get; set; } = null!;
        public int? TenantId { get; set; }
        public string? OrgName { get; set; }
        public int? GroupId { get; set; }
        public string? GroupName { get; set; }
        public List<int> GroupIds { get; set; } = new();
        public List<int> GroupCourseIds { get; set; } = new();
        public List<int> GroupUserIds { get; set; } = new();
        public int? RoleId { get; set; }
        public List<int> RoleIds { get; set; } = new();
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

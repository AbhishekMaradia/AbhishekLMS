namespace LMS_SoulCode.Features.UserPermissions.DTOs
{
    public class UserRoleDto
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string RoleCode { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool RoleIsActive { get; set; }
        public string? UserEmail { get; set; }
        public int? TenantId { get; set; }
        public string? OrgName { get; set; }
    }
}

namespace LMS_SoulCode.Features.UserPermissions.DTOs
{
    public class UserPermissionDto
    {
        public int PermissionId { get; set; }
        public string RoleCode { get; set; } = null!;
        public string ModuleCode { get; set; } = null!;
        public string PermissionCode { get; set; } = null!;
        public string PermissionName { get; set; } = null!;
    }
}

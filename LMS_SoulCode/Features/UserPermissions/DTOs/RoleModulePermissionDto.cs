namespace LMS_SoulCode.Features.UserPermissions.DTOs
{
    public class RoleModulePermissionDto
    {
        public int Id { get; set; }
        public int RoleModuleId { get; set; }
        public int PermissionId { get; set; }
        
        // Navigation properties for display
        public string RoleName { get; set; } = null!;
        public string RoleCode { get; set; } = null!;
        public string ModuleName { get; set; } = null!;
        public string ModuleCode { get; set; } = null!;
        public string PermissionName { get; set; } = null!;
        public string PermissionCode { get; set; } = null!;
    }
}
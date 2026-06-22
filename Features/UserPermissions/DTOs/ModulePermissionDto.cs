namespace LMS_SoulCode.Features.UserPermissions.DTOs
{
    public class ModulePermissionDto
    {
        public int Id { get; set; }
        public int ModuleId { get; set; }
        public int PermissionId { get; set; }
        
        // Navigation properties for display
        public string ModuleName { get; set; } = null!;
        public string ModuleCode { get; set; } = null!;
        public string PermissionName { get; set; } = null!;
        public string PermissionCode { get; set; } = null!;
    }
}
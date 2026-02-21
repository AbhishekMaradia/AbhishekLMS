namespace LMS_SoulCode.Features.UserPermissions.DTOs
{
    public class RoleModuleListDto
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string RoleCode { get; set; } = string.Empty;
        public int ModuleId { get; set; }
        public string ModuleName { get; set; } = string.Empty;
        public string ModuleCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
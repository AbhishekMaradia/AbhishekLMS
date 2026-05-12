namespace LMS_SoulCode.Features.UserPermissions.DTOs
{
    public class UpdateUserRoleDto
    {
        public bool IsActive { get; set; }
        public int? NewRoleId { get; set; }
        public int? NewTenantId { get; set; }
    }
}

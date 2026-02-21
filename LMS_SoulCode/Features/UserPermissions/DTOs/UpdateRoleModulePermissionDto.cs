using System.ComponentModel.DataAnnotations;

namespace LMS_SoulCode.Features.UserPermissions.DTOs
{
    public class UpdateRoleModulePermissionDto
    {
        [Required(ErrorMessage = "RoleModuleId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "RoleModuleId must be greater than 0")]
        public int RoleModuleId { get; set; }

        [Required(ErrorMessage = "PermissionId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "PermissionId must be greater than 0")]
        public int PermissionId { get; set; }
    }
}
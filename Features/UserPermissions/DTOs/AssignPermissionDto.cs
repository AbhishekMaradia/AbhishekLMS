namespace LMS_SoulCode.Features.UserPermissions.DTOs
{
    public record AssignPermissionDto(
     int RoleId,
     int ModuleId,
     List<int> PermissionIds,
     int? TenantId = null
 );
}

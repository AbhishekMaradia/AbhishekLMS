namespace LMS_SoulCode.Features.UserPermissions.DTOs
{
    public class RoleDtos
    {
        public record CreateRoleDto(string Code, string Name, bool IsActive = true, int? TenantId = null);
        public record UpdateRoleDto(string Name, bool? IsActive = null, int? TenantId = null);
        public record GetRoleDto(int Id, string Code, string Name, bool IsActive, bool IsDefault, int? TenantId = null, string? OrgName = null);

    }
}

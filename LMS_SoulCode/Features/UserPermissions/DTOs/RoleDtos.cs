namespace LMS_SoulCode.Features.UserPermissions.DTOs
{
    public class RoleDtos
    {
        public record CreateRoleDto(string Code, string Name, int? TenantId = null);
        public record UpdateRoleDto(string Name, bool? IsActive = null);
        public record GetRoleDto(int Id, string Code, string Name, bool IsActive, bool IsDefault);

    }
}

namespace LMS_SoulCode.Features.UserPermissions.DTOs
{
    public class PermissionDtos
    {
        public record CreatePermissionDto(string Code, string Name);
        public record UpdatePermissionDto(string Name, bool IsActive);
        public record GetPermissionDto(int Id, string Code, string Name, bool? IsActive);

    }
}

namespace LMS_SoulCode.Features.UserPermissions.DTOs
{
    public class ModuleDtos
    {
        public record CreateModuleDto(string Code, string Name);
        public record UpdateModuleDto(string Name, bool? IsActive = null);
        public record GetModuleDto(int Id, string Code, string Name, bool IsActive);
        public record AssignModulePermissionsDto(int ModuleId, List<int> PermissionIds);
    }
}

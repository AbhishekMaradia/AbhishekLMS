namespace LMS_SoulCode.Features.UserPermissions.DTOs
{
    public class RoleModuleDtos
    {
        public record CreateRoleModuleDto(int RoleId, int ModuleId);
        public record GetRoleModuleDto(int Id, int RoleId, int ModuleId, string RoleName, string ModuleName);
    }
}
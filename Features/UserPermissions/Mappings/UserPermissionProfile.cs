using AutoMapper;
using LMS_SoulCode.Features.UserPermissions.DTOs;
using LMS_SoulCode.Features.UserPermissions.Models;
using static LMS_SoulCode.Features.UserPermissions.DTOs.ModuleDtos;

using static LMS_SoulCode.Features.UserPermissions.DTOs.RoleDtos;
using static LMS_SoulCode.Features.UserPermissions.DTOs.RoleModuleDtos;

namespace LMS_SoulCode.Features.UserPermissions.Mappings
{
    public class UserPermissionProfile : Profile

    {
        public UserPermissionProfile()
        {
            CreateMap<CreateRoleDto, Role>()
                .ForMember(d => d.Code, o => o.MapFrom(s => (s.Code ?? string.Empty).ToUpperInvariant()))
                .ForMember(d => d.CreatedAt, o => o.MapFrom(_ => DateTime.UtcNow))
                .ForMember(d => d.IsActive, o => o.MapFrom(s => s.IsActive));
            CreateMap<UpdateRoleDto, Role>()
                .ForMember(d => d.IsActive, o => {
                    o.Condition(s => s.IsActive.HasValue);
                    o.MapFrom(s => s.IsActive!.Value);
                })
                .ForMember(d => d.TenantId, o => o.MapFrom(s => s.TenantId));

            CreateMap<CreateModuleDto, Module>()
                .ForMember(d => d.Code, o => o.MapFrom(s => s.Code != null ? s.Code.ToUpperInvariant() : null));
            CreateMap<UpdateModuleDto, Module>()
                .ForMember(d => d.IsActive, o => {
                    o.Condition(s => s.IsActive.HasValue);
                    o.MapFrom(s => s.IsActive!.Value);
                });

            CreateMap<CreatePermissionDto, Permission>()
                .ForMember(d => d.Code, o => o.MapFrom(s => s.Code != null ? s.Code.ToUpperInvariant() : null))
                .ForMember(d => d.IsActive, o => o.MapFrom(_ => true));
            CreateMap<UpdatePermissionDto, Permission>();

            CreateMap<CreateRoleModuleDto, RoleModule>();
            CreateMap<RoleModule, GetRoleModuleDto>()
                .ForMember(d => d.RoleName, o => o.MapFrom(s => s.Role != null ? s.Role.Name : string.Empty))
                .ForMember(d => d.ModuleName, o => o.MapFrom(s => s.Module != null ? s.Module.Name : string.Empty));

            CreateMap<RoleModulePermission, UserPermissionDto>()
              .ForMember(d => d.PermissionId, o => o.MapFrom(s => s.PermissionId))
              .ForMember(d => d.RoleCode,
                  o => o.MapFrom(s => s.RoleModule != null && s.RoleModule.Role != null ? s.RoleModule.Role.Code : null))
              .ForMember(d => d.ModuleCode,
                  o => o.MapFrom(s => s.RoleModule != null && s.RoleModule.Module != null ? s.RoleModule.Module.Code : null))
              .ForMember(d => d.PermissionCode,
                  o => o.MapFrom(s => s.Permission != null ? s.Permission.Code : null))
              .ForMember(d => d.PermissionName,
                  o => o.MapFrom(s => s.Permission != null ? s.Permission.Name : null));

            CreateMap<Permission, GetPermissionDto>()
                .ForMember(d => d.Code, o => o.MapFrom(s => s.Code ?? string.Empty))
                .ForMember(d => d.Name, o => o.MapFrom(s => s.Name ?? string.Empty))
                .ForMember(d => d.IsActive, o => o.MapFrom(s => s.IsActive ?? true));

            CreateMap<Module, GetModuleDto>()
                .ForMember(d => d.Code, o => o.MapFrom(s => s.Code ?? string.Empty))
                .ForMember(d => d.Name, o => o.MapFrom(s => s.Name ?? string.Empty))
                .ForMember(d => d.IsActive, o => o.MapFrom(s => s.IsActive ?? true));

            CreateMap<Role, GetRoleDto>()
                .ForMember(d => d.Code, o => o.MapFrom(s => s.Code ?? string.Empty))
                .ForMember(d => d.Name, o => o.MapFrom(s => s.Name ?? string.Empty))
                .ForMember(d => d.IsActive, o => o.MapFrom(s => s.IsActive))
                .ForMember(d => d.IsDefault, o => o.MapFrom(s => s.IsDefault))
                .ForMember(d => d.TenantId, o => o.MapFrom(s => s.TenantId));

            // RoleModulePermission to RoleModulePermissionDto mapping
            CreateMap<RoleModulePermission, RoleModulePermissionDto>()
                .ForMember(d => d.RoleName, o => o.MapFrom(s => s.RoleModule.Role.Name))
                .ForMember(d => d.RoleCode, o => o.MapFrom(s => s.RoleModule.Role.Code))
                .ForMember(d => d.ModuleName, o => o.MapFrom(s => s.RoleModule.Module.Name))
                .ForMember(d => d.ModuleCode, o => o.MapFrom(s => s.RoleModule.Module.Code))
                .ForMember(d => d.PermissionName, o => o.MapFrom(s => s.Permission.Name))
                .ForMember(d => d.PermissionCode, o => o.MapFrom(s => s.Permission.Code));

            // ModulePermission to ModulePermissionDto mapping
            CreateMap<ModulePermission, ModulePermissionDto>()
                .ForMember(d => d.ModuleName, o => o.MapFrom(s => s.Module.Name))
                .ForMember(d => d.ModuleCode, o => o.MapFrom(s => s.Module.Code))
                .ForMember(d => d.PermissionName, o => o.MapFrom(s => s.Permission.Name))
                .ForMember(d => d.PermissionCode, o => o.MapFrom(s => s.Permission.Code));
        }
    }
}

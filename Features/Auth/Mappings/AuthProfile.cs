using AutoMapper;
using LMS_SoulCode.Features.Auth.DTOs;
using LMS_SoulCode.Features.Auth.Models;

namespace LMS_SoulCode.Features.Auth.Mappings
{
    public class AuthProfile : Profile
    {
        public AuthProfile()
        {
            // User to UserDto mapping
            CreateMap<User, UserDto>()
                .ForMember(d => d.UserRole, o => o.MapFrom(_ => string.Empty)) // Default empty, can be populated separately
                .ForMember(d => d.OrgName, o => o.MapFrom(s => s.Organization != null ? s.Organization.Name : null))
                .ForMember(d => d.GroupName, o => o.MapFrom(s => s.Group != null ? s.Group.GroupName : null));

            // RegisterRequest to User mapping
            CreateMap<RegisterRequest, User>()
                .ForMember(d => d.PasswordHash, o => o.Ignore())
                .ForMember(d => d.Id, o => o.Ignore());

            CreateMap<AdminCreateUserRequest, User>()
                .ForMember(d => d.PasswordHash, o => o.Ignore())
                .ForMember(d => d.Id, o => o.Ignore());

            // OrgRegisterRequest to User mapping
            CreateMap<LMS_SoulCode.Features.Organizations.DTOs.OrgRegisterRequest, User>()
                .ForMember(d => d.PasswordHash, o => o.Ignore())
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.TenantId, o => o.Ignore());

            // UpdateUserRequest to User mapping (for partial updates)
            CreateMap<UpdateUserRequest, User>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        }
    }
}
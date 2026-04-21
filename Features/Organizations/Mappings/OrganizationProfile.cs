using AutoMapper;
using LMS_SoulCode.Features.Organizations.DTOs;
using LMS_SoulCode.Features.Organizations.Models;

namespace LMS_SoulCode.Features.Organizations.Mappings
{
    public class OrganizationProfile : Profile
    {
        public OrganizationProfile()
        {
            // Organization to OrganizationDto mapping
            CreateMap<Organization, OrganizationDto>()
                .ForMember(d => d.OrgName, o => o.MapFrom(s => s.Name))
                .ForMember(d => d.OrgCode, o => o.MapFrom(s => s.Code))
                .ForMember(d => d.PrimaryColor, o => o.MapFrom(s => s.PrimaryColor))
                .ForMember(d => d.SecondaryColor, o => o.MapFrom(s => s.SecondaryColor));

            // OrgRegisterRequest to Organization mapping
            CreateMap<OrgRegisterRequest, Organization>()
                .ForMember(d => d.Name, o => o.MapFrom(s => s.OrgName))
                .ForMember(d => d.Code, o => o.MapFrom(s => s.OrgCode))
                .ForMember(d => d.PrimaryColor, o => o.MapFrom(s => s.PrimaryColor))
                .ForMember(d => d.SecondaryColor, o => o.MapFrom(s => s.SecondaryColor))
                .ForMember(d => d.IsActive, o => o.MapFrom(_ => true))
                .ForMember(d => d.CreatedAt, o => o.MapFrom(_ => DateTime.UtcNow))
                .ForMember(d => d.UpdatedAt, o => o.MapFrom(_ => DateTime.UtcNow));

            // UpdateOrganizationRequest to Organization mapping
            CreateMap<UpdateOrganizationRequest, Organization>()
                .ForMember(d => d.Name, o => o.MapFrom(s => s.OrgName))
                .ForMember(d => d.Code, o => o.MapFrom(s => s.OrgCode))
                .ForMember(d => d.PrimaryColor, o => o.MapFrom(s => s.PrimaryColor))
                .ForMember(d => d.SecondaryColor, o => o.MapFrom(s => s.SecondaryColor))
                .ForMember(d => d.IsActive, o => o.MapFrom(s => s.IsActive))
                .ForMember(d => d.UpdatedAt, o => o.MapFrom(_ => DateTime.UtcNow))
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null)); // Simpler null check
        }
    }
}

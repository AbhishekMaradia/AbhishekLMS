using AutoMapper;
using LMS_SoulCode.Features.Reports.DTOs;

namespace LMS_SoulCode.Features.Reports.Mappings
{
    public class ReportsProfile : Profile
    {
        public ReportsProfile()
        {
            CreateMap<LMS_SoulCode.Features.SubscribedCourse.Models.UserCourse, ReportListDto>()
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => (src.User.FirstName ?? "") + " " + (src.User.LastName ?? "")))
                .ForMember(dest => dest.OrgName, opt => opt.MapFrom(src => src.User.Organization != null ? src.User.Organization.Name : "Platform"))
                .ForMember(dest => dest.GroupName, opt => opt.MapFrom(src => src.User.Group != null ? src.User.Group.GroupName : "General"))
                .ForMember(dest => dest.CourseTitle, opt => opt.MapFrom(src => src.Course.Title))
                .ForMember(dest => dest.CourseInstructor, opt => opt.MapFrom(src => src.Course.Instructor))
                .ForMember(dest => dest.EnrolledAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.TenantId, opt => opt.MapFrom(src => src.User.TenantId));
        }
    }
}
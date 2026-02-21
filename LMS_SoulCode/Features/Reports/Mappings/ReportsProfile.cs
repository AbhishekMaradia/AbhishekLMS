using AutoMapper;
using LMS_SoulCode.Features.Reports.DTOs;

namespace LMS_SoulCode.Features.Reports.Mappings
{
    public class ReportsProfile : Profile
    {
        public ReportsProfile()
        {
            CreateMap<LMS_SoulCode.Features.SubscribedCourse.Models.UserCourse, ReportListDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.CourseTitle, opt => opt.MapFrom(src => src.Course.Title))
                .ForMember(dest => dest.CourseInstructor, opt => opt.MapFrom(src => src.Course.Instructor))
                .ForMember(dest => dest.EnrolledAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.TenantId, opt => opt.MapFrom(src => src.User.TenantId)); // Assuming User tenant matches
        }
    }
}
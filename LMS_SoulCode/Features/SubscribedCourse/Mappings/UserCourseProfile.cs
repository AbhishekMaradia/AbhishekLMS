using AutoMapper;
using LMS_SoulCode.Features.SubscribedCourse.DTOs;
using LMS_SoulCode.Features.SubscribedCourse.Models;

namespace LMS_SoulCode.Features.SubscribedCourse.Mappings
{
    public class UserCourseProfile : Profile
    {
        public UserCourseProfile()
        {
            // UserCourse to UserCourseListDto mapping
            CreateMap<UserCourse, UserCourseListDto>()
                .ForMember(d => d.UserEmail, o => o.Ignore()) // Will be populated from joined data
                .ForMember(d => d.CourseTitle, o => o.Ignore()) // Will be populated from joined data
                .ForMember(d => d.CategoryName, o => o.Ignore()); // Will be populated from joined data
        }
    }
}
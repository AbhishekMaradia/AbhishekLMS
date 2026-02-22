using AutoMapper;
using LMS_SoulCode.Features.Groups.DTOs;
using LMS_SoulCode.Features.Groups.Models;
using LMS_SoulCode.Features.Course.Models;

namespace LMS_SoulCode.Features.Groups.Mappings
{
    public class GroupProfile : Profile
    {
        public GroupProfile()
        {
            CreateMap<CreateGroupRequest, Group>();
            
            CreateMap<Group, GroupDto>()
                .ForMember(dest => dest.GroupCourses, opt => opt.MapFrom(src => src.GroupCourses));

            CreateMap<GroupCourse, GroupCourseDto>()
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course != null ? src.Course.Title : string.Empty));
        }
    }
}

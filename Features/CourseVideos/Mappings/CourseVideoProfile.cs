using AutoMapper;
using LMS_SoulCode.Features.CourseVideos.DTOs;
using LMS_SoulCode.Features.CourseVideos.Models;

namespace LMS_SoulCode.Features.CourseVideos.Mappings
{
    public class CourseVideoProfile : Profile
    {
        public CourseVideoProfile()
        {
            // CourseVideo to CourseVideoDto mapping
            CreateMap<CourseVideo, CourseVideoDto>()
                .ForMember(d => d.CourseName, o => o.MapFrom(s => s.Course != null ? s.Course.Title : string.Empty));

            // UserVideoProgress mapping
            CreateMap<UserVideoProgress, UserVideoProgressDto>()
                .ForMember(d => d.VideoTitle, o => o.MapFrom(s => s.Video != null ? s.Video.Title : string.Empty));

            CreateMap<CourseDocument, CourseDocumentResponse>();
        }
    }
}
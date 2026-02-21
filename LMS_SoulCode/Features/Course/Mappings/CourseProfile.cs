using AutoMapper;
using LMS_SoulCode.Features.Course.DTOs;
using CourseEntity = LMS_SoulCode.Features.Course.Models.Course;

namespace LMS_SoulCode.Features.Course.Mappings
{
    public class CourseProfile : Profile
    {
        public CourseProfile()
        {
            // CourseRequest to Course mapping
            CreateMap<CourseRequest, CourseEntity>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.EnrolledCount, o => o.MapFrom(_ => 0))
                .ForMember(d => d.Rating, o => o.MapFrom(_ => 0.0))
                .ForMember(d => d.CreatedAt, o => o.MapFrom(_ => DateTime.UtcNow))
                .ForMember(d => d.UpdatedAt, o => o.MapFrom(_ => DateTime.UtcNow))
                .ForMember(d => d.IsActive, o => o.MapFrom(_ => true))
                .ForMember(d => d.Videos, o => o.Ignore());

            // UpdateCourseRequest to Course mapping (for partial updates)
            CreateMap<UpdateCourseRequest, CourseEntity>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.EnrolledCount, o => o.Ignore())
                .ForMember(d => d.Rating, o => o.Ignore())
                .ForMember(d => d.CreatedAt, o => o.Ignore())
                .ForMember(d => d.UpdatedAt, o => o.MapFrom(_ => DateTime.UtcNow))
                .ForMember(d => d.Videos, o => o.Ignore());

            // Course to CourseResponse mapping (base mapping without VideoUrls)
            CreateMap<CourseEntity, CourseResponse>()
                .ForMember(d => d.CourseId, o => o.MapFrom(s => s.Id))
                .ForMember(d => d.VideoUrls, o => o.Ignore()); // Will be populated manually with base URL
        }
    }
}
using AutoMapper;
using LMS_SoulCode.Features.Course.DTOs;
using CategoryEntity = LMS_SoulCode.Features.Course.Models.Category;

namespace LMS_SoulCode.Features.Course.Mappings
{
    public class CategoryProfile : Profile
    {
        public CategoryProfile()
        {
            // CategoryRequest to Category mapping
            CreateMap<CategoryRequest, CategoryEntity>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.CreatedAt, o => o.MapFrom(_ => DateTime.UtcNow))
                .ForMember(d => d.UpdatedAt, o => o.MapFrom(_ => DateTime.UtcNow));
                //.ForMember(d => d.IsActive, o => o.MapFrom(_ => true));

            // Category to CategoryResponse mapping
            CreateMap<CategoryEntity, CategoryResponse>()
                .ForMember(d => d.CategoryId, o => o.MapFrom(s => s.Id));
        }
    }
}
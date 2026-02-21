using CategoryEntity = LMS_SoulCode.Features.Course.Models.Category;
using LMS_SoulCode.Features.Course.Models;
using LMS_SoulCode.Features.Course.DTOs;
using LMS_SoulCode.Features.Common.Repositories;

namespace LMS_SoulCode.Features.Course.Repositories
{
    public interface ICategoryRepository : IBaseRepository<CategoryEntity>
    {
        Task<(IEnumerable<CategoryEntity> Items, int TotalCount)> GetCategoriesAsync(string? searchTerm, int pageNumber, int pageSize, int? tenantId, CancellationToken cancellationToken);
    }
}
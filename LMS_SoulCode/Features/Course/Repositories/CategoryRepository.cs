using LMS_SoulCode.Data;
using LMS_SoulCode.Features.Course.Models;
using LMS_SoulCode.Features.Common.Repositories;
using LMS_SoulCode.Features.Course.DTOs;
using LMS_SoulCode.Features.Common;
using Microsoft.EntityFrameworkCore;

namespace LMS_SoulCode.Features.Course.Repositories
{
    public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(LmsDbContext context) : base(context) { }

        public async Task<(IEnumerable<Category> Items, int TotalCount)> GetCategoriesAsync(string? searchTerm, int pageNumber, int pageSize, int? tenantId, CancellationToken cancellationToken)
        {
            return await GetPagedAsync(
                pageNumber,
                pageSize,
                filter: c => (!tenantId.HasValue || c.TenantId == tenantId) &&
                            (string.IsNullOrWhiteSpace(searchTerm) || c.CategoryName.ToLower().Contains(searchTerm.ToLower())),
                queryModifier: q => q.AsNoTracking().OrderBy(c => c.CategoryName),
                cancellationToken: cancellationToken
            );
        }
    }
}

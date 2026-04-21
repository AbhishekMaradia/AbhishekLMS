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

        public async Task<(IEnumerable<CategoryResponse> Items, int TotalCount)> GetCategoriesAsync(string? searchTerm, int pageNumber, int pageSize, int? tenantId, CancellationToken cancellationToken)
        {
            return await GetPagedAsync<CategoryResponse>(
                pageNumber,
                pageSize,
                filter: c => (tenantId == null || tenantId == 0 || c.TenantId == tenantId) &&
                            !c.IsDeleted &&
                            (string.IsNullOrWhiteSpace(searchTerm) || c.CategoryName.ToLower().Contains(searchTerm.ToLower())),
                projection: q => q.AsNoTracking().OrderBy(c => c.CategoryName).Select(c => new CategoryResponse
                {
                    CategoryId = c.Id,
                    CategoryName = c.CategoryName,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    TenantId = c.TenantId,
                    OrgName = _context.Organizations.Where(o => o.Id == c.TenantId).Select(o => o.Name).FirstOrDefault() ?? "Super Admin"
                }),
                cancellationToken: cancellationToken
            );
        }

        public async Task<bool> AnyInTenantAsync(int tenantId, CancellationToken cancellationToken = default)
            => await _context.Categories.AnyAsync(c => c.TenantId == tenantId && !c.IsDeleted, cancellationToken);
    }
}

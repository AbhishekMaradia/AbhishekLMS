using LMS_SoulCode.Features.Course.Models;
using CategoryEntity = LMS_SoulCode.Features.Course.Models.Category;
using LMS_SoulCode.Features.Course.Repositories;
using LMS_SoulCode.Features.Course.DTOs;
using LMS_SoulCode.Features.Common;
using StatusCodes = LMS_SoulCode.Features.Common.StatusCodes;
using AutoMapper;

namespace LMS_SoulCode.Features.Course.Services
{
    public interface ICategoryService
    {
        Task<PagedApiResponse<CategoryResponse>> GetAllAsync(CategoryListRequest request, int? tenantId, CancellationToken cancellationToken);
        Task<ApiResponse<List<CategoryResponse>>> GetByIdAsync(int id, int? tenantId, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<CategoryResponse>>> CreateAsync(string name, int? tenantId, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<string>>> UpdateAsync(int id, string newName, int? tenantId, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<string>>> DeleteAsync(int id, int? tenantId, CancellationToken cancellationToken = default);
    }

    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _category;
        private readonly IMapper _mapper;

        public CategoryService(ICategoryRepository categoryRepo, IMapper mapper)
        {
            _category = categoryRepo;
            _mapper = mapper;
        }

        public async Task<PagedApiResponse<CategoryResponse>> GetAllAsync(CategoryListRequest request, int? tenantId, CancellationToken cancellationToken)
        {
            // Security: Org Admin can only see their own tenant, SuperAdmin can specify or see all
            var targetTenantId = tenantId.HasValue 
                ? tenantId                  // Org Admin - force their own tenant
                : request.TenantId;         // SuperAdmin - use request or null
            
            var (categories, totalCount) = await _category.GetCategoriesAsync(request.SearchTerm, request.PageNumber, request.PageSize, targetTenantId, cancellationToken);
            var categoryResponses = _mapper.Map<IEnumerable<CategoryResponse>>(categories);
            return PagedApiResponse<CategoryResponse>.Success(categoryResponses, request.PageNumber, request.PageSize, totalCount, Messages.Success);
        }
        
        public async Task<ApiResponse<List<CategoryResponse>>> GetByIdAsync(int id, int? tenantId, CancellationToken cancellationToken = default)
        {
            var category = await _category.GetByIdAsync(id, cancellationToken);

            if (category == null)
                return ApiResponse<List<CategoryResponse>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            if (tenantId.HasValue && category.TenantId != tenantId.Value)
                return ApiResponse<List<CategoryResponse>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            var response = _mapper.Map<CategoryResponse>(category);
            return ApiResponse<List<CategoryResponse>>.Success(new List<CategoryResponse> { response }, Messages.Success);
        }
        
        public async Task<ApiResponse<List<CategoryResponse>>> CreateAsync(string name, int? tenantId, CancellationToken cancellationToken = default)
        {
            var category = new CategoryEntity { CategoryName = name, TenantId = tenantId };
            await _category.AddAsync(category, cancellationToken);

            var response = _mapper.Map<CategoryResponse>(category);
            return ApiResponse<List<CategoryResponse>>.Success(new List<CategoryResponse> { response }, Messages.Created);
        }
        public async Task<ApiResponse<List<string>>> UpdateAsync(int id, string newName, int? tenantId, CancellationToken cancellationToken = default)
        {
            var category = await _category.GetByIdAsync(id, cancellationToken);

            if (category == null)
                return ApiResponse<List<string>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            if (tenantId.HasValue && category.TenantId != tenantId.Value)
                return ApiResponse<List<string>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            category.CategoryName = newName;
            await _category.UpdateAsync(category, cancellationToken);

            return ApiResponse<List<string>>.Success(new List<string>(), Messages.Updated);
        }
        public async Task<ApiResponse<List<string>>> DeleteAsync(int id, int? tenantId, CancellationToken cancellationToken = default)
        {
            var category = await _category.GetByIdAsync(id, cancellationToken);

            if (category == null)
                return ApiResponse<List<string>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            if (tenantId.HasValue && category.TenantId != tenantId.Value)
                return ApiResponse<List<string>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            await _category.DeleteAsync(id, cancellationToken);

            return ApiResponse<List<string>>.Success(new List<string>(), Messages.Deleted);
        }
    }
}

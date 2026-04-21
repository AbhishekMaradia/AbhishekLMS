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
        Task<ApiResponse<List<string>>> UpdateAsync(int id, CategoryRequest request, int? currentTenantId, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<string>>> DeleteAsync(int id, int? tenantId, CancellationToken cancellationToken = default);
    }

    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _category;
        private readonly IMapper _mapper;
        private readonly ICourseRepository _courseRepository;

        public CategoryService(ICategoryRepository categoryRepo, IMapper mapper, ICourseRepository courseRepository)
        {
            _category = categoryRepo;
            _mapper = mapper;
            _courseRepository = courseRepository;
        }

        public async Task<PagedApiResponse<CategoryResponse>> GetAllAsync(CategoryListRequest request, int? tenantId, CancellationToken cancellationToken)
        {
            // Security: Org Admin can only see their own tenant, SuperAdmin can specify or see all
            var targetTenantId = tenantId.HasValue 
                ? tenantId                  // Org Admin - force their own tenant
                : request.TenantId;         // SuperAdmin - use request or null
            
            var (categories, totalCount) = await _category.GetCategoriesAsync(request.SearchTerm, request.PageNumber, request.PageSize, targetTenantId, cancellationToken);
            return PagedApiResponse<CategoryResponse>.Success(categories.ToList(), request.PageNumber, request.PageSize, totalCount, Messages.Success);
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
            var category = new CategoryEntity { CategoryName = name, TenantId = tenantId ?? 0 };
            await _category.AddAsync(category, cancellationToken);

            var response = _mapper.Map<CategoryResponse>(category);
            return ApiResponse<List<CategoryResponse>>.Success(new List<CategoryResponse> { response }, Messages.Created);
        }
        public async Task<ApiResponse<List<string>>> UpdateAsync(int id, CategoryRequest request, int? currentTenantId, CancellationToken cancellationToken = default)
        {
            var category = await _category.GetByIdAsync(id, cancellationToken);

            if (category == null)
                return ApiResponse<List<string>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            // Security Check
            if (currentTenantId.HasValue)
            {
                // Org Admin can only update their own categories
                if (category.TenantId != currentTenantId.Value)
                {
                    if (category.TenantId == 0)
                        return ApiResponse<List<string>>.Fail("You do not have permission to edit a global category.", StatusCodes.Forbidden);
                    return ApiResponse<List<string>>.Fail("Category not found or you do not have permission.", StatusCodes.NotFound);
                }
                
                // Org Admin cannot change TenantId (auto-enforce their own)
                category.CategoryName = request.CategoryName;
            }
            else
            {
                // SuperAdmin can update name and organization (TenantId)
                var newTenantId = request.TenantId ?? 0;
                if (category.TenantId != newTenantId)
                {
                    // Strict Validation: Cannot change organization if courses exist in this category
                    if (await _courseRepository.AnyInCategoryAsync(id, cancellationToken))
                    {
                        return ApiResponse<List<string>>.Fail(Messages.UpdateBlockedMove, StatusCodes.BadRequest);
                    }
                    category.TenantId = newTenantId;
                }
                category.CategoryName = request.CategoryName;
            }

            await _category.UpdateAsync(category, cancellationToken);
            return ApiResponse<List<string>>.Success(new List<string>(), Messages.Updated);
        }
        public async Task<ApiResponse<List<string>>> DeleteAsync(int id, int? tenantId, CancellationToken cancellationToken = default)
        {
            var category = await _category.GetByIdAsync(id, cancellationToken);

            if (category == null)
                return ApiResponse<List<string>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            if (tenantId.HasValue && category.TenantId != tenantId.Value)
            {
                if (category.TenantId == 0)
                    return ApiResponse<List<string>>.Fail("You do not have permission to delete a global category.", StatusCodes.Forbidden);
                return ApiResponse<List<string>>.Fail("Category not found or you do not have permission.", StatusCodes.NotFound);
            }

            if (await _courseRepository.AnyInCategoryAsync(id, cancellationToken))
            {
                return ApiResponse<List<string>>.Fail(Messages.DeleteBlockedCategory, StatusCodes.BadRequest);
            }

            await _category.DeleteAsync(id, cancellationToken);

            return ApiResponse<List<string>>.Success(new List<string>(), Messages.Deleted);
        }
    }
}

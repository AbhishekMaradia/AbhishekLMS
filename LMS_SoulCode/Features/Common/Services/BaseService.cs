using AutoMapper;
using LMS_SoulCode.Features.Common.Repositories;
using LMS_SoulCode.Features.Common;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace LMS_SoulCode.Features.Common.Services
{
    public abstract class BaseService<TEntity, TCreateDto, TResponseDto>
        where TEntity : class
    {
        protected readonly IBaseRepository<TEntity> _repository;
        protected readonly IMapper _mapper;
        protected readonly ICacheService _cache;
        protected readonly ILogger _logger;

        protected BaseService(IBaseRepository<TEntity> repository,IMapper mapper,ICacheService cache,ILogger logger)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
            _logger = logger;
        }

        protected virtual async Task<BusinessValidationResult> ValidateBusinessRules(TCreateDto request)
        {
            return await Task.FromResult(new BusinessValidationResult { IsValid = true });
        }

        protected virtual async Task<BusinessValidationResult> ValidateBusinessRulesForUpdate(TCreateDto request, int id)
        {
            return await Task.FromResult(new BusinessValidationResult { IsValid = true });
        }

        public virtual async Task<ApiResponse<IEnumerable<TResponseDto>>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            var response = _mapper.Map<IEnumerable<TResponseDto>>(entities);
            return ApiResponse<IEnumerable<TResponseDto>>.Success(response);
        }

        public virtual async Task<ApiResponse<TResponseDto>> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
            {
                return ApiResponse<TResponseDto>.Fail("Not Found", 404);
            }
            var response = _mapper.Map<TResponseDto>(entity);
            return ApiResponse<TResponseDto>.Success(response);
        }

        public virtual async Task<ApiResponse<TResponseDto>> CreateAsync(TCreateDto request)
        {
            var validation = await ValidateBusinessRules(request);
            if (!validation.IsValid)
            {
                return ApiResponse<TResponseDto>.Fail(validation.ErrorMessage, 400);
            }

            var entity = _mapper.Map<TEntity>(request);
            var created = await _repository.AddAsync(entity);
            var response = _mapper.Map<TResponseDto>(created);
            return ApiResponse<TResponseDto>.Success(response, "Created successfully");
        }

        public virtual async Task<ApiResponse<TResponseDto>> UpdateAsync(int id, TCreateDto request)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
            {
                return ApiResponse<TResponseDto>.Fail("Not Found", 404);
            }

            var validation = await ValidateBusinessRulesForUpdate(request, id);
            if (!validation.IsValid)
            {
                return ApiResponse<TResponseDto>.Fail(validation.ErrorMessage, 400);
            }

            _mapper.Map(request, existing);
            await _repository.UpdateAsync(existing);
            var response = _mapper.Map<TResponseDto>(existing);
            return ApiResponse<TResponseDto>.Success(response, "Updated successfully");
        }

        public virtual async Task<ApiResponse<string>> DeleteAsync(int id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
            {
                return ApiResponse<string>.Fail("Not Found", 404);
            }

            await _repository.DeleteAsync(id);
            return ApiResponse<string>.Success("Deleted successfully");
        }
    }
}

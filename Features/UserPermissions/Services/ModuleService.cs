using AutoMapper;
using LMS_SoulCode.Features.Common;
using LMS_SoulCode.Features.Common.Services;
using LMS_SoulCode.Features.UserPermissions.Repositories;
using LMS_SoulCode.Features.UserPermissions.Models;
using LMS_SoulCode.Features.UserPermissions.DTOs;
using static LMS_SoulCode.Features.UserPermissions.DTOs.ModuleDtos;
using StatusCodes = LMS_SoulCode.Features.Common.StatusCodes;

namespace LMS_SoulCode.Features.UserPermissions.Services
{
    public class ModuleService
    {
        private readonly IModuleRepository _moduleRepo;
        private readonly IMapper _mapper;

        public ModuleService(IModuleRepository moduleRepository, IMapper mapper)
        {
            _moduleRepo = moduleRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<IEnumerable<GetModuleDto>>> GetModulesAsync(CancellationToken cancellationToken = default)
        {
            var modules = await _moduleRepo.GetAllDtoAsync(cancellationToken);
            return ApiResponse<IEnumerable<GetModuleDto>>.Success(modules, Messages.Success);
        }

        public async Task<PagedApiResponse<GetModuleDto>> GetModulesAsync(ModuleListRequest request, CancellationToken cancellationToken)
        {
            var (modules, totalCount) = await _moduleRepo.GetModulesAsync(
                request.SearchTerm, 
                request.PageNumber, 
                request.PageSize, 
                request.IsActive, 
                cancellationToken);

            return PagedApiResponse<GetModuleDto>.Success(modules, request.PageNumber, request.PageSize, totalCount, Messages.Success);
        }
 
        public async Task<ApiResponse<List<GetModuleDto>>> CreateModuleAsync(CreateModuleDto dto, CancellationToken cancellationToken = default)
        {
            var module = _mapper.Map<Module>(dto);
            await _moduleRepo.AddAsync(module, cancellationToken);

            var response = _mapper.Map<GetModuleDto>(module);
            return ApiResponse<List<GetModuleDto>>.Success(new List<GetModuleDto> { response }, Messages.Created);
        }

        public async Task<ApiResponse<List<GetModuleDto>>> UpdateModuleAsync(int id, UpdateModuleDto dto, CancellationToken cancellationToken = default)
        {
            var module = await _moduleRepo.GetByIdAsync(id, cancellationToken);
            if (module == null) return ApiResponse<List<GetModuleDto>>.Fail(Messages.NotFound, StatusCodes.NotFound);
   
            _mapper.Map(dto, module);
            await _moduleRepo.UpdateAsync(module, cancellationToken);

            var response = _mapper.Map<GetModuleDto>(module);
            return ApiResponse<List<GetModuleDto>>.Success(new List<GetModuleDto> { response }, Messages.Updated);
        }

        public async Task<ApiResponse<List<string>>> DeleteModuleAsync(int id, CancellationToken cancellationToken = default)
        {
            var module = await _moduleRepo.GetByIdAsync(id, cancellationToken);
            if (module == null) return ApiResponse<List<string>>.Fail(Messages.NotFound, StatusCodes.NotFound);
  
            await _moduleRepo.DeleteAsync(id, cancellationToken);
            return ApiResponse<List<string>>.Success(new List<string>(), Messages.Deleted);
        }

        public async Task<ApiResponse<List<string>>> AssignPermissionsToModuleAsync(AssignModulePermissionsDto dto, int? tenantId, CancellationToken cancellationToken = default)
        {
            await _moduleRepo.LinkPermissionsAsync(dto.ModuleId, dto.PermissionIds, tenantId, cancellationToken);
            return ApiResponse<List<string>>.Success(new List<string>(), Messages.PermissionsLinked);
        }

        public async Task<ApiResponse<IEnumerable<GetPermissionDto>>> GetModulePermissionsAsync(int moduleId, CancellationToken cancellationToken = default)
        {
            var permissions = await _moduleRepo.GetModulePermissionsAsync(moduleId, cancellationToken);
            return ApiResponse<IEnumerable<GetPermissionDto>>.Success(permissions, Messages.Success);
        }
    }
}

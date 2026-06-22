using LMS_SoulCode.Features.Common;
using LMS_SoulCode.Features.UserPermissions.DTOs;
using LMS_SoulCode.Features.UserPermissions.Repositories;
using StatusCodes = LMS_SoulCode.Features.Common.StatusCodes;
using AutoMapper;

namespace LMS_SoulCode.Features.UserPermissions.Services
{
    public class ModulePermissionService
    {
        private readonly IModulePermissionRepository _modulePermissionRepo;
        private readonly IMapper _mapper;

        public ModulePermissionService(IModulePermissionRepository modulePermissionRepository, IMapper mapper)
        {
            _modulePermissionRepo = modulePermissionRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<List<ModulePermissionDto>>> GetByIdAsync(int id, int? tenantId, CancellationToken cancellationToken = default)
        {
            var modulePermission = await _modulePermissionRepo.GetByIdAsync(id, cancellationToken);
            if (modulePermission == null)
                return ApiResponse<List<ModulePermissionDto>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            if (tenantId.HasValue && modulePermission.TenantId != tenantId.Value)
                  return ApiResponse<List<ModulePermissionDto>>.Fail(Messages.Forbidden, StatusCodes.Forbidden);

            var dto = _mapper.Map<ModulePermissionDto>(modulePermission);
            return ApiResponse<List<ModulePermissionDto>>.Success(new List<ModulePermissionDto> { dto }, Messages.Success);
        }

        public async Task<PagedApiResponse<ModulePermissionDto>> GetPagedAsync(ModulePermissionListRequest request, int? tenantId, CancellationToken cancellationToken)
        {
            var targetTenantId = tenantId.HasValue ? tenantId : request.TenantId;
            var (modulePermissions, totalCount) = await _modulePermissionRepo.GetPagedAsync(
                request.ModuleId,
                request.PermissionId,
                request.SearchTerm,
                request.PageNumber,
                request.PageSize,
                targetTenantId,
                cancellationToken);

            return PagedApiResponse<ModulePermissionDto>.Success(
                modulePermissions, 
                request.PageNumber, 
                request.PageSize, 
                totalCount, 
                Messages.Success);
        }
    }
}
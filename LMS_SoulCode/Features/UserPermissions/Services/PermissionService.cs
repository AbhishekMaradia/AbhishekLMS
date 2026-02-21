using AutoMapper;
using LMS_SoulCode.Features.Common;
using LMS_SoulCode.Features.Common.Services;
using LMS_SoulCode.Features.UserPermissions.Models;
using LMS_SoulCode.Features.UserPermissions.Repositories;
using static LMS_SoulCode.Features.UserPermissions.DTOs.PermissionDtos;
using LMS_SoulCode.Features.UserPermissions.DTOs;
using StatusCodes = LMS_SoulCode.Features.Common.StatusCodes;

namespace LMS_SoulCode.Features.UserPermissions.Services
{
    public class PermissionService
    {
        private readonly IPermissionRepository _permissionRepo;
        private readonly IMapper _mapper;

        public PermissionService(IPermissionRepository permissionRepository, IMapper mapper)
        {
            _permissionRepo = permissionRepository;
            _mapper = mapper;
        }

        public async Task<PagedApiResponse<GetPermissionDto>> GetPermissionsAsync(PermissionListRequest request, CancellationToken cancellationToken)
        {
            var (items, totalCount) = await _permissionRepo.GetPermissionsAsync(request.SearchTerm, request.PageNumber, request.PageSize, request.IsActive, cancellationToken);
            return PagedApiResponse<GetPermissionDto>.Success(items, request.PageNumber, request.PageSize, totalCount);
        }

        public async Task<ApiResponse<List<GetPermissionDto>>> CreatePermissionAsync(CreatePermissionDto dto, CancellationToken cancellationToken = default)
        {
            var permission = _mapper.Map<Permission>(dto);
            await _permissionRepo.AddAsync(permission, cancellationToken);
            
            var response = _mapper.Map<GetPermissionDto>(permission);
            return ApiResponse<List<GetPermissionDto>>.Success(new List<GetPermissionDto> { response }, Messages.Created);
        }

        public async Task<ApiResponse<List<GetPermissionDto>>> UpdatePermissionAsync(int id, UpdatePermissionDto dto, CancellationToken cancellationToken = default)
        {
            var permission = await _permissionRepo.GetByIdAsync(id, cancellationToken);
            if (permission == null) return ApiResponse<List<GetPermissionDto>>.Fail(Messages.NotFound, StatusCodes.NotFound);
 
            _mapper.Map(dto, permission);
            await _permissionRepo.UpdateAsync(permission, cancellationToken);

            var response = _mapper.Map<GetPermissionDto>(permission);
            return ApiResponse<List<GetPermissionDto>>.Success(new List<GetPermissionDto> { response }, Messages.Updated);
        }
    }

}

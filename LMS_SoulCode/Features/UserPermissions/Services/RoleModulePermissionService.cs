using LMS_SoulCode.Features.Common;
using LMS_SoulCode.Features.UserPermissions.DTOs;
using LMS_SoulCode.Features.UserPermissions.Models;
using LMS_SoulCode.Features.UserPermissions.Repositories;
using StatusCodes = LMS_SoulCode.Features.Common.StatusCodes;
using AutoMapper;

namespace LMS_SoulCode.Features.UserPermissions.Services
{
    public class RoleModulePermissionService
    {
        private readonly IRoleModulePermissionRepository _roleModulePermissionRepo;
        private readonly IMapper _mapper;

        public RoleModulePermissionService(IRoleModulePermissionRepository roleModulePermissionRepository, IMapper mapper)
        {
            _roleModulePermissionRepo = roleModulePermissionRepository;
            _mapper = mapper;
        }
        public async Task<ApiResponse<List<RoleModulePermissionDto>>> GetByIdAsync(int id, int? tenantId, CancellationToken cancellationToken = default)
        {
            var roleModulePermission = await _roleModulePermissionRepo.GetByIdAsync(id, cancellationToken);
            if (roleModulePermission == null)
                return ApiResponse<List<RoleModulePermissionDto>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            if (tenantId.HasValue && roleModulePermission.RoleModule.Role.TenantId != tenantId.Value)
                  return ApiResponse<List<RoleModulePermissionDto>>.Fail(Messages.Forbidden, StatusCodes.Forbidden);

            var dto = _mapper.Map<RoleModulePermissionDto>(roleModulePermission);
            return ApiResponse<List<RoleModulePermissionDto>>.Success(new List<RoleModulePermissionDto> { dto }, Messages.Success);
        }

        public async Task<ApiResponse<List<RoleModulePermissionDto>>> CreateAsync(CreateRoleModulePermissionDto dto, CancellationToken cancellationToken = default)
        {
            // Validate RoleModule exists
            var roleModuleExists = await _roleModulePermissionRepo.RoleModuleExistsAsync(dto.RoleModuleId, cancellationToken);
            if (!roleModuleExists)
                return ApiResponse<List<RoleModulePermissionDto>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            // Validate Permission exists and is active
            var permissionExists = await _roleModulePermissionRepo.PermissionExistsAsync(dto.PermissionId, cancellationToken);
            if (!permissionExists)
                return ApiResponse<List<RoleModulePermissionDto>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            // Check if this combination already exists
            var exists = await _roleModulePermissionRepo.RoleModulePermissionExistsAsync(dto.RoleModuleId, dto.PermissionId, cancellationToken);
            if (exists)
                return ApiResponse<List<RoleModulePermissionDto>>.Fail(Messages.AlreadyExists, StatusCodes.BadRequest);

            var roleModulePermission = new RoleModulePermission
            {
                RoleModuleId = dto.RoleModuleId,
                PermissionId = dto.PermissionId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var created = await _roleModulePermissionRepo.AddAsync(roleModulePermission, cancellationToken);
            
            // Get the created entity with navigation properties
            var createdWithNav = await _roleModulePermissionRepo.GetByIdAsync(created.Id, cancellationToken);
            
            var resultDto = _mapper.Map<RoleModulePermissionDto>(createdWithNav!);
            return ApiResponse<List<RoleModulePermissionDto>>.Success(new List<RoleModulePermissionDto> { resultDto }, Messages.Created);
        }

        public async Task<ApiResponse<List<string>>> DeleteAsync(int id, int? tenantId, CancellationToken cancellationToken = default)
        {
            var existing = await _roleModulePermissionRepo.GetByIdAsync(id, cancellationToken);
            if (existing == null)
                return ApiResponse<List<string>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            // Security: Check if user owns the role
            if (tenantId.HasValue && existing.RoleModule.Role.TenantId != tenantId.Value)
                  return ApiResponse<List<string>>.Fail(Messages.Forbidden, StatusCodes.Forbidden);

            await _roleModulePermissionRepo.DeleteAsync(id, cancellationToken);
            return ApiResponse<List<string>>.Success(new List<string>(), Messages.Deleted);
        }

        public async Task<PagedApiResponse<RoleModulePermissionDto>> GetPagedAsync(RoleModulePermissionListRequest request, int? tenantId, CancellationToken cancellationToken)
        {
            var (roleModulePermissions, totalCount) = await _roleModulePermissionRepo.GetPagedAsync(
                request.RoleId,
                request.ModuleId,
                request.PermissionId,
                request.RoleModuleId,
                request.SearchTerm,
                request.PageNumber,
                request.PageSize,
                tenantId,
                cancellationToken);

            return PagedApiResponse<RoleModulePermissionDto>.Success(
                roleModulePermissions, 
                request.PageNumber, 
                request.PageSize, 
                totalCount, 
                Messages.Success);
        }

    }
}
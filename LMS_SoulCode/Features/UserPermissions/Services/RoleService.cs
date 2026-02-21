using AutoMapper;
using LMS_SoulCode.Features.Common;
using LMS_SoulCode.Features.Common.Services;
using LMS_SoulCode.Features.UserPermissions.Models;
using LMS_SoulCode.Features.UserPermissions.Repositories;
using static LMS_SoulCode.Features.UserPermissions.DTOs.RoleDtos;
using LMS_SoulCode.Features.UserPermissions.DTOs;
using StatusCodes = LMS_SoulCode.Features.Common.StatusCodes;

namespace LMS_SoulCode.Features.UserPermissions.Services
{
    public class RoleService
    {
        private readonly IRoleRepository _roleRepo;
        private readonly IMapper _mapper;

        public RoleService(IRoleRepository roleRepository, IMapper mapper)
        {
            _roleRepo = roleRepository;
            _mapper = mapper;
        }

        public async Task<PagedApiResponse<GetRoleDto>> GetRolesAsync(RoleListRequest request, int? tenantId, CancellationToken cancellationToken)
        {
            // Security: Org Admin can only see their own tenant, SuperAdmin can specify or see all
            var targetTenantId = tenantId.HasValue 
                ? tenantId                  // Org Admin - force their own tenant
                : request.TenantId;         // SuperAdmin - use request or null
            
            var (items, totalCount) = await _roleRepo.GetRolesAsync(request.SearchTerm, request.PageNumber, request.PageSize, request.IsActive, targetTenantId, cancellationToken);
            return PagedApiResponse<GetRoleDto>.Success(items, request.PageNumber, request.PageSize, totalCount);
        }
 
        public async Task<ApiResponse<List<GetRoleDto>>> CreateRoleAsync(CreateRoleDto dto, int? tenantId, CancellationToken cancellationToken = default)
        {
            var role = _mapper.Map<Role>(dto);
            role.TenantId = tenantId;
            await _roleRepo.AddAsync(role, cancellationToken);
            
            var response = _mapper.Map<GetRoleDto>(role);
            return ApiResponse<List<GetRoleDto>>.Success(new List<GetRoleDto> { response }, Messages.Created);
        }

        public async Task<ApiResponse<List<GetRoleDto>>> UpdateRoleAsync(int id, UpdateRoleDto dto, int? tenantId, CancellationToken cancellationToken = default)
        {
            var role = await _roleRepo.GetByIdAsync(id, cancellationToken);
            if (role == null) return ApiResponse<List<GetRoleDto>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            // Security: Prevent Cross-Tenant Modification
            if (tenantId.HasValue)
            {
                if (role.TenantId != tenantId.Value)
                    return ApiResponse<List<GetRoleDto>>.Fail("You do not have permission to modify this role", StatusCodes.Forbidden);
            }

            // Check if IsActive is being changed
            bool isActiveChanged = dto.IsActive.HasValue && role.IsActive != dto.IsActive.Value;
 
            _mapper.Map(dto, role);
            await _roleRepo.UpdateAsync(role, cancellationToken);

            var response = _mapper.Map<GetRoleDto>(role);

            // Cascade IsActive to UserRoles if changed
            if (isActiveChanged && dto.IsActive.HasValue)
            {
                var affectedCount = await _roleRepo.CascadeRoleIsActiveToUserRolesAsync(id, dto.IsActive.Value, cancellationToken);
                var message = dto.IsActive.Value 
                    ? $"Role updated and {affectedCount} user-role assignment(s) activated"
                    : $"Role updated and {affectedCount} user-role assignment(s) deactivated";
                return ApiResponse<List<GetRoleDto>>.Success(new List<GetRoleDto> { response }, message);
            }

            return ApiResponse<List<GetRoleDto>>.Success(new List<GetRoleDto> { response }, Messages.Updated);
        }

        public async Task<ApiResponse<List<string>>> DeleteRoleAsync(int id, int? tenantId, CancellationToken cancellationToken = default)
        {
            var role = await _roleRepo.GetByIdAsync(id, cancellationToken);
            if (role == null) return ApiResponse<List<string>>.Fail(Messages.NotFound, StatusCodes.NotFound);
 
            // Security: Prevent Cross-Tenant Deletion
            if (tenantId.HasValue)
            {
                 if (role.TenantId != tenantId.Value)
                    return ApiResponse<List<string>>.Fail("You do not have permission to delete this role", StatusCodes.Forbidden);
            }

            // Safeguard: Prevent Deleting Default/System Roles
            if (role.IsDefault || role.Code == "ORGANIZATION_ADMIN" || role.Code == "SUPER_ADMIN")
                return ApiResponse<List<string>>.Fail("Cannot delete System/Default Role", StatusCodes.Forbidden);

            await _roleRepo.DeleteAsync(id, cancellationToken);
            return ApiResponse<List<string>>.Success(new List<string>(), Messages.Deleted);
        }
    }

}

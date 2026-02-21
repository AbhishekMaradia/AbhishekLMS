using AutoMapper;
using LMS_SoulCode.Features.Common;
using LMS_SoulCode.Features.Common.Services;
using LMS_SoulCode.Features.UserPermissions.Models;
using LMS_SoulCode.Features.UserPermissions.DTOs;
using LMS_SoulCode.Features.UserPermissions.Repositories;
using static LMS_SoulCode.Features.UserPermissions.DTOs.RoleModuleDtos;
using StatusCodes = LMS_SoulCode.Features.Common.StatusCodes;

namespace LMS_SoulCode.Features.UserPermissions.Services
{
    public class RoleModuleService
    {
        private readonly IRoleModuleRepository _roleModuleRepo;
        private readonly IUserPermissionRepository _userPermissionRepo;
        private readonly IRoleRepository _roleRepo;
        private readonly IMapper _mapper;

        public RoleModuleService(IRoleModuleRepository roleModuleRepository, 
            IUserPermissionRepository userPermissionRepository, 
            IRoleRepository roleRepository,
            IMapper mapper)
        {
            _roleModuleRepo = roleModuleRepository;
            _userPermissionRepo = userPermissionRepository;
            _roleRepo = roleRepository;
            _mapper = mapper;
        }



        // New paginated method
        public async Task<PagedApiResponse<RoleModuleListDto>> GetRoleModulesAsync(RoleModuleListRequest request, int? tenantId, CancellationToken cancellationToken)
        {
            var (roleModules, totalCount) = await _roleModuleRepo.GetRoleModulesAsync(
                request.SearchTerm, 
                request.PageNumber, 
                request.PageSize, 
                request.RoleId, 
                request.ModuleId, 
                tenantId,
                cancellationToken);

            return PagedApiResponse<RoleModuleListDto>.Success(roleModules, request.PageNumber, request.PageSize, totalCount, Messages.Success);
        }

        public async Task<ApiResponse<IEnumerable<GetRoleModuleDto>>> GetRoleModulesByRoleAsync(int roleId, int? tenantId, CancellationToken cancellationToken = default)
        {
            // Security: We arguably should check if roleId belongs to Tenant here too, 
            // but reading public/system roles might be okay. 
            // For strictness, let's allow reading any role for now to not break UI logic 
            // (e.g. seeing permissions of a System Role assigned to you).
            
            var roleModules = await _roleModuleRepo.GetByRoleIdAsync(roleId, tenantId, cancellationToken);
            var dtos = _mapper.Map<List<GetRoleModuleDto>>(roleModules);
            return ApiResponse<IEnumerable<GetRoleModuleDto>>.Success(dtos, Messages.Success);
        }

        public async Task<ApiResponse<List<GetRoleModuleDto>>> CreateRoleModuleAsync(CreateRoleModuleDto dto, int? tenantId, CancellationToken cancellationToken = default)
        {
            // 1. Validate Role exists and Check Ownership
            var role = await _roleRepo.GetByIdAsync(dto.RoleId, cancellationToken);
            if (role == null)
                return ApiResponse<List<GetRoleModuleDto>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            if (tenantId.HasValue)
            {
                if (role.TenantId != tenantId.Value)
                    return ApiResponse<List<GetRoleModuleDto>>.Fail("You do not have permission to modify this role", StatusCodes.Forbidden);
            }
            // Optional: Prevent modifying System Roles? 
            if (role.IsDefault || role.TenantId == null)
            {
                // If we want to allow assigning modules to System Roles, we skip this.
                // But usually System Roles are fixed. 
                // Let's assume we cannot modify System Roles.
                if (tenantId.HasValue) // Only block tenants from modifying system roles
                    return ApiResponse<List<GetRoleModuleDto>>.Fail("Cannot modify System Role", StatusCodes.Forbidden);
            }

            // 2. Validate Module exists
            var moduleExists = await _userPermissionRepo.ModuleExistsAsync(dto.ModuleId, cancellationToken);
            if (!moduleExists)
                return ApiResponse<List<GetRoleModuleDto>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            // 3. Check if already exists
            var existing = await _roleModuleRepo.FindByRoleAndModuleAsync(dto.RoleId, dto.ModuleId, cancellationToken);
            if (existing != null)
                return ApiResponse<List<GetRoleModuleDto>>.Fail(Messages.AlreadyExists, StatusCodes.BadRequest);

            var roleModule = _mapper.Map<RoleModule>(dto);
            await _roleModuleRepo.AddAsync(roleModule, cancellationToken);
            
            // Fetch again to get navigation properties for the DTO
            var createdRoleModule = await _roleModuleRepo.GetByIdAsync(roleModule.Id, cancellationToken);
            var response = _mapper.Map<GetRoleModuleDto>(createdRoleModule);
            
            return ApiResponse<List<GetRoleModuleDto>>.Success(new List<GetRoleModuleDto> { response }, Messages.Created);
        }

        public async Task<ApiResponse<List<string>>> DeleteRoleModuleAsync(int id, int? tenantId, CancellationToken cancellationToken = default)
        {
            var roleModule = await _roleModuleRepo.GetByIdAsync(id, cancellationToken);
            if (roleModule == null)
                return ApiResponse<List<string>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            // Access the associated Role to check TenantId
            // Assuming roleModule has Role navigation property populated or we fetch it.
            // RoleModule usually has RoleId.
            var role = await _roleRepo.GetByIdAsync(roleModule.RoleId, cancellationToken);
            
            if (role != null)
            {
                if (tenantId.HasValue)
                {
                    if (role.TenantId != tenantId.Value)
                        return ApiResponse<List<string>>.Fail("You do not have permission to delete this role assignment", StatusCodes.Forbidden);
                }
                if (role.IsDefault && tenantId.HasValue)
                     return ApiResponse<List<string>>.Fail("Cannot modify System Role", StatusCodes.Forbidden);
            }

            await _roleModuleRepo.DeleteAsync(id, cancellationToken);
            return ApiResponse<List<string>>.Success(new List<string>(), Messages.Deleted);
        }
    }
}

using LMS_SoulCode.Features.Common;
using StatusCodes =LMS_SoulCode.Features.Common.StatusCodes;
using LMS_SoulCode.Features.UserPermissions.DTOs;
using LMS_SoulCode.Features.UserPermissions.Repositories;
using LMS_SoulCode.Features.Auth.Repositories; // Added
using Microsoft.AspNetCore.Http;

namespace LMS_SoulCode.Features.UserPermissions.Services
{
    public class UserPermissionService : IUserPermissionService
    {
        private readonly IUserPermissionRepository _userPermissionRepo;
        private readonly IUserRepository _userRepo;
        private readonly IRoleRepository _roleRepo;

        public UserPermissionService(IUserPermissionRepository userPermissionRepository, 
            IUserRepository userRepository, 
            IRoleRepository roleRepository)
        {
            _userPermissionRepo = userPermissionRepository;
            _userRepo = userRepository;
            _roleRepo = roleRepository;
        }

        private async Task ValidateTenantAccessAsync(int? entityTenantId, string entityName, int? currentTenantId)
        {
            if (currentTenantId.HasValue)
            {
                if (entityTenantId.HasValue && entityTenantId.Value != currentTenantId.Value)
                {
                    throw new UnauthorizedAccessException($"You do not have permission to access {entityName}");
                }
            }
        }

        public async Task<ApiResponse<List<string>>> AssignRoleToUserAsync(AssignRoleDto dto, int? tenantId, CancellationToken cancellationToken = default)
        {
            try 
            {
                // 1. Validate User checks
                var user = await _userRepo.GetByIdAsync(dto.UserId, cancellationToken);
                if (user == null) return ApiResponse<List<string>>.Fail(Messages.UserNotFound, StatusCodes.NotFound);
                
                if (!user.IsActive) return ApiResponse<List<string>>.Fail(Messages.UserInactive, StatusCodes.Forbidden);
                
                await ValidateTenantAccessAsync(user.TenantId, "User", tenantId);

                // 2. Validate Role checks
                var role = await _roleRepo.GetByIdAsync(dto.RoleId, cancellationToken);
                if (role == null) return ApiResponse<List<string>>.Fail(Messages.RoleNotFound, StatusCodes.NotFound);

                if (tenantId.HasValue && role.TenantId.HasValue && role.TenantId != tenantId.Value)
                     return ApiResponse<List<string>>.Fail(Messages.RoleNotFound, StatusCodes.NotFound);
                
                // Check if user already has this role
                var userRoleExists = await _userPermissionRepo.UserRoleExistsAsync(dto.UserId, dto.RoleId, cancellationToken);
                if (userRoleExists)
                    return ApiResponse<List<string>>.Fail(Messages.UserRoleExists, StatusCodes.BadRequest);

                var targetTenantId = tenantId ?? user.TenantId;
                await _userPermissionRepo.AssignRoleAsync(dto.UserId, dto.RoleId, targetTenantId, cancellationToken);
                return ApiResponse<List<string>>.Success(new List<string>(), Messages.RoleAssigned);
            }
            catch (UnauthorizedAccessException) 
            {
                return ApiResponse<List<string>>.Fail(Messages.Forbidden, StatusCodes.Forbidden);
            }
        }
 
        public async Task<ApiResponse<List<string>>> AssignPermissionsAsync(AssignPermissionDto dto, int? tenantId, CancellationToken cancellationToken = default)
        {
            // Validate Role exists and check tenant access
            var role = await _roleRepo.GetByIdAsync(dto.RoleId, cancellationToken);
            if (role == null) return ApiResponse<List<string>>.Fail(Messages.RoleNotFound, StatusCodes.NotFound);
            
            if (tenantId.HasValue && role.TenantId.HasValue && role.TenantId != tenantId.Value)
                 return ApiResponse<List<string>>.Fail(Messages.RoleNotFound, StatusCodes.NotFound); // Or Forbidden

            // Validate Module exists and is active
            var moduleExists = await _userPermissionRepo.ModuleExistsAsync(dto.ModuleId, cancellationToken);
            if (!moduleExists)
                return ApiResponse<List<string>>.Fail(Messages.ModuleNotFound, StatusCodes.NotFound);
                    
            // Validate Permissions exist and are active (Skip if empty to allow clearing)
            var validPermissionIds = (dto.PermissionIds != null && dto.PermissionIds.Any()) 
                ? await _userPermissionRepo.ValidatePermissionsAsync(dto.PermissionIds, cancellationToken)
                : new List<int>();

            if (dto.PermissionIds != null && dto.PermissionIds.Any() && validPermissionIds.Count == 0)
                return ApiResponse<List<string>>.Fail(Messages.NoValidPermissions, StatusCodes.BadRequest);

            // Derive target tenant for assignment
            var targetTenantIdForAssignment = dto.TenantId ?? tenantId ?? role.TenantId;

            // Proceed with valid permissions (with warning if some were invalid)
            await _userPermissionRepo.AssignPermissionsAsync(dto, targetTenantIdForAssignment, cancellationToken);
            
            if (validPermissionIds.Count < dto.PermissionIds.Count)
            {
                var invalidCount = dto.PermissionIds.Count - validPermissionIds.Count;
                return ApiResponse<List<string>>.Success(new List<string>(), $"Warning: {invalidCount} permission(s) not found or inactive. Successfully assigned {validPermissionIds.Count} valid permission(s) to role-module");
            }

            return ApiResponse<List<string>>.Success(new List<string>(), $"Successfully assigned {validPermissionIds.Count} permission(s) to role-module");
        }
 
        public async Task<ApiResponse<IEnumerable<UserPermissionDto>>> GetUserPermissionsAsync(int userId, int? tenantId, CancellationToken cancellationToken = default)
        {
            // First check if user exists
            var user = await _userRepo.GetByIdAsync(userId, cancellationToken);
            if (user == null)
                return ApiResponse<IEnumerable<UserPermissionDto>>.Fail(Messages.UserNotFound, StatusCodes.NotFound);

            if (!user.IsActive)
                return ApiResponse<IEnumerable<UserPermissionDto>>.Fail(Messages.UserInactive, StatusCodes.Forbidden);

            try { await ValidateTenantAccessAsync(user.TenantId, "User", tenantId); }
            catch (UnauthorizedAccessException) { return ApiResponse<IEnumerable<UserPermissionDto>>.Fail(Messages.Forbidden, StatusCodes.Forbidden); }

            var permissions = await _userPermissionRepo.GetUserPermissionsAsync(userId, tenantId, cancellationToken);
            
            // If user exists but has no permissions, still return success with empty data
            return ApiResponse<IEnumerable<UserPermissionDto>>.Success(permissions, 
                permissions.Count == 0 ? Messages.NoPermissionsAssigned : Messages.Success);
        }

        public async Task<ApiResponse<List<bool>>> CheckUserPermissionAsync(int userId, string moduleCode, string permissionCode, int? tenantId, CancellationToken cancellationToken = default)
        {
             // First check if user exists
            var user = await _userRepo.GetByIdAsync(userId, cancellationToken);
            if (user == null)
                return ApiResponse<List<bool>>.Fail(Messages.UserNotFound, StatusCodes.NotFound);

            if (!user.IsActive)
                return ApiResponse<List<bool>>.Fail(Messages.UserInactive, StatusCodes.Forbidden);

            if (tenantId.HasValue && user.TenantId != tenantId.Value)
                  return ApiResponse<List<bool>>.Success(new List<bool> { false });

            var hasPermission = await _userPermissionRepo.CheckUserPermissionAsync(userId, moduleCode, permissionCode, tenantId, cancellationToken);
            return ApiResponse<List<bool>>.Success(new List<bool> { hasPermission });
        }

        public async Task<ApiResponse<List<string>>> RemoveRoleFromUserAsync(int userId, int roleId, int? tenantId, CancellationToken cancellationToken = default)
        {
            try 
            {
                // Validate User exists
                var user = await _userRepo.GetByIdAsync(userId, cancellationToken);
                if (user == null) return ApiResponse<List<string>>.Fail(Messages.UserNotFound, StatusCodes.NotFound);

                if (!user.IsActive) return ApiResponse<List<string>>.Fail(Messages.UserInactive, StatusCodes.Forbidden);

                await ValidateTenantAccessAsync(user.TenantId, "User", tenantId);

                // Validate Role
                var role = await _roleRepo.GetByIdAsync(roleId, cancellationToken);
                if (role == null) return ApiResponse<List<string>>.Fail(Messages.RoleNotFound, StatusCodes.NotFound);
                
                if (tenantId.HasValue && role.TenantId.HasValue && role.TenantId != tenantId.Value)
                     return ApiResponse<List<string>>.Fail(Messages.RoleNotFound, StatusCodes.NotFound);

                // Check if user has this role
                var userRoleExists = await _userPermissionRepo.UserRoleExistsAsync(userId, roleId, cancellationToken);
                if (!userRoleExists)
                    return ApiResponse<List<string>>.Fail(Messages.UserRoleNotFound, StatusCodes.NotFound);


                await _userPermissionRepo.RemoveRoleFromUserAsync(userId, roleId, cancellationToken);
                return ApiResponse<List<string>>.Success(new List<string>(), Messages.RoleRemoved);
             }
            catch (UnauthorizedAccessException) 
            {
                return ApiResponse<List<string>>.Fail(Messages.Forbidden, StatusCodes.Forbidden);
            }
        }

        public async Task<ApiResponse<List<string>>> UpdateUserRoleStatusAsync(int userId, int roleId, bool isActive, int? tenantId, CancellationToken cancellationToken = default)
        {
            try
            {
                // Validate User exists
                var user = await _userRepo.GetByIdAsync(userId, cancellationToken);
                if (user == null) return ApiResponse<List<string>>.Fail(Messages.UserNotFound, StatusCodes.NotFound);

                if (!user.IsActive) return ApiResponse<List<string>>.Fail(Messages.UserInactive, StatusCodes.Forbidden);

                await ValidateTenantAccessAsync(user.TenantId, "User", tenantId);

                // Validate Role exists in DB
                var role = await _roleRepo.GetByIdAsync(roleId, cancellationToken);
                if (role == null || (tenantId.HasValue && role.TenantId.HasValue && role.TenantId != tenantId.Value))
                     return ApiResponse<List<string>>.Fail(Messages.RoleNotFound, StatusCodes.NotFound);
                
                // Check if user has this role
                 var userRoleExists = await _userPermissionRepo.UserRoleExistsAsync(userId, roleId, cancellationToken);
                 if (!userRoleExists)
                    return ApiResponse<List<string>>.Fail(Messages.UserRoleNotFound, StatusCodes.NotFound);

                return await _userPermissionRepo.UpdateUserRoleStatusAsync(userId, roleId, isActive, cancellationToken);
            }
            catch (UnauthorizedAccessException) 
            {
                return ApiResponse<List<string>>.Fail(Messages.Forbidden, StatusCodes.Forbidden);
            }
        }

        public async Task<ApiResponse<IEnumerable<UserRoleDto>>> GetUserRolesWithStatusAsync(int userId, int? tenantId, CancellationToken cancellationToken = default)
        {
            // Validate User exists
            var user = await _userRepo.GetByIdAsync(userId, cancellationToken);
            if (user == null)
                return ApiResponse<IEnumerable<UserRoleDto>>.Fail(Messages.UserNotFound, StatusCodes.NotFound);

            if (!user.IsActive)
                return ApiResponse<IEnumerable<UserRoleDto>>.Fail(Messages.UserInactive, StatusCodes.Forbidden);

            try { await ValidateTenantAccessAsync(user.TenantId, "User", tenantId); }
            catch (UnauthorizedAccessException) { return ApiResponse<IEnumerable<UserRoleDto>>.Fail(Messages.Forbidden, StatusCodes.Forbidden); }

            var userRoles = await _userPermissionRepo.GetUserRolesWithStatusAsync(userId, cancellationToken);
            return ApiResponse<IEnumerable<UserRoleDto>>.Success(userRoles, 
                userRoles.Count == 0 ? Messages.NoRolesAssigned : Messages.Success);
        }

        public async Task<PagedApiResponse<UserRoleDto>> GetUserRolesPagedAsync(UserRoleListRequest request, int? tenantId, CancellationToken cancellationToken)
        {
            // Security: Org Admin can only see their own tenant, SuperAdmin can specify or see all
            var targetTenantId = tenantId.HasValue 
                ? tenantId                  // Org Admin - force their own tenant
                : request.TenantId;         // SuperAdmin - use request or null
            
            var (userRoles, totalCount) = await _userPermissionRepo.GetUserRolesPagedAsync(
                request.UserId,
                request.RoleId,
                request.IsActive,
                request.SearchTerm,
                request.PageNumber,
                request.PageSize,
                targetTenantId,
                cancellationToken);

            return PagedApiResponse<UserRoleDto>.Success(userRoles, request.PageNumber, request.PageSize, totalCount, Messages.Success);
        }

        public async Task<ApiResponse<List<string>>> UpdatePermissionsAsync(AssignPermissionDto dto, int? tenantId, CancellationToken cancellationToken = default)
        {
            // Validate Role exists and check tenant access
            var role = await _roleRepo.GetByIdAsync(dto.RoleId, cancellationToken);
            if (role == null) return ApiResponse<List<string>>.Fail(Messages.RoleNotFound, StatusCodes.NotFound);

            if (tenantId.HasValue && role.TenantId.HasValue && role.TenantId != tenantId.Value)
                 return ApiResponse<List<string>>.Fail(Messages.RoleNotFound, StatusCodes.NotFound);

            // Validate Module exists and is active
            var moduleExists = await _userPermissionRepo.ModuleExistsAsync(dto.ModuleId, cancellationToken);
            if (!moduleExists)
                return ApiResponse<List<string>>.Fail(Messages.ModuleNotFound, StatusCodes.NotFound);

            // Validate Permissions exist and are active (Skip if empty to allow clearing)
            var validPermissionIds = (dto.PermissionIds != null && dto.PermissionIds.Any()) 
                ? await _userPermissionRepo.ValidatePermissionsAsync(dto.PermissionIds, cancellationToken)
                : new List<int>();

            if (dto.PermissionIds != null && dto.PermissionIds.Any() && validPermissionIds.Count == 0)
                return ApiResponse<List<string>>.Fail(Messages.NoValidPermissions, StatusCodes.BadRequest);

            // Derive target tenant for assignment
            var targetTenantIdForAssignment = dto.TenantId ?? tenantId ?? role.TenantId;

            // Update permissions (this will replace existing permissions)
            await _userPermissionRepo.AssignPermissionsAsync(dto, targetTenantIdForAssignment, cancellationToken);
            
            if (validPermissionIds.Count < dto.PermissionIds.Count)
            {
                var invalidCount = dto.PermissionIds.Count - validPermissionIds.Count;
                return ApiResponse<List<string>>.Success(new List<string>(), $"Warning: {invalidCount} permission(s) not found or inactive. Successfully updated with {validPermissionIds.Count} valid permission(s)");
            }

            return ApiResponse<List<string>>.Success(new List<string>(), $"Successfully updated role-module with {validPermissionIds.Count} permission(s)");
        }

        public async Task<ApiResponse<IEnumerable<UserPermissionDto>>> GetRoleModulePermissionsAsync(int roleId, int moduleId, int? tenantId, CancellationToken cancellationToken = default)
        {
            // Validate Role exists and check tenant access
            var role = await _roleRepo.GetByIdAsync(roleId, cancellationToken);
            if (role == null) return ApiResponse<IEnumerable<UserPermissionDto>>.Fail(Messages.RoleNotFound, StatusCodes.NotFound);

            if (tenantId.HasValue && role.TenantId.HasValue && role.TenantId != tenantId.Value)
                 return ApiResponse<IEnumerable<UserPermissionDto>>.Fail(Messages.RoleNotFound, StatusCodes.NotFound);

            // Validate Module exists and is active
            var moduleExists = await _userPermissionRepo.ModuleExistsAsync(moduleId, cancellationToken);
            if (!moduleExists)
                return ApiResponse<IEnumerable<UserPermissionDto>>.Fail(Messages.ModuleNotFound, StatusCodes.NotFound);

            var permissions = await _userPermissionRepo.GetRoleModulePermissionsAsync(roleId, moduleId, tenantId, cancellationToken);
            
            return ApiResponse<IEnumerable<UserPermissionDto>>.Success(permissions, permissions.Count == 0 ? "No permissions found for this role-module combination" : Messages.Success);
        }
    }
}

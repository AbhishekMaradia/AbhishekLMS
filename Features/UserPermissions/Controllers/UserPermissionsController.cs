using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LMS_SoulCode.Features.Common;
using LMS_SoulCode.Features.UserPermissions.DTOs;
using LMS_SoulCode.Features.UserPermissions.Services;

namespace LMS_SoulCode.Features.UserPermissions.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/user-permissions")]
    public class UserPermissionsController : BaseApiController
    {
        private readonly IUserPermissionService _userPermissionService;

        public UserPermissionsController(IUserPermissionService userPermissionService, ILogger<UserPermissionsController> logger) : base(logger)
        {
            _userPermissionService = userPermissionService;
        }

        [HttpPost("assign-role")]
        [BackOfficePermission(ModuleCodes.USER_ROLE, PermissionCodes.USER_ROLE_ASSIGN)]
        public async Task<IActionResult> AssignRoleToUser([FromBody] AssignRoleDto dto, CancellationToken cancellationToken)
        {
            var response = await _userPermissionService.AssignRoleToUserAsync(dto, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpPost("assign-permissions")]
        [BackOfficePermission(ModuleCodes.ROLE_MODULE, PermissionCodes.ROLE_MODULE_PERMISSION_ASSIGN)]
        public async Task<IActionResult> AssignPermissionsToRoleModule([FromBody] AssignPermissionDto dto, CancellationToken cancellationToken)
        {
            var response = await _userPermissionService.AssignPermissionsAsync(dto, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpGet("role-module/{roleId:int}/{moduleId:int}/permissions")]
        [BackOfficePermission(ModuleCodes.ROLE_MODULE, PermissionCodes.ROLE_MODULE_PERMISSION_VIEW, PermissionCodes.ROLE_MODULE_PERMISSION_ASSIGN)]
        public async Task<IActionResult> GetRoleModulePermissions(int roleId, int moduleId, [FromQuery] int? tenantId, CancellationToken cancellationToken)
        {
            var targetTenantId = CurrentTenantId ?? tenantId;
            var response = await _userPermissionService.GetRoleModulePermissionsAsync(roleId, moduleId, targetTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        //[HttpGet("user/{userId:int}")]
        //[BackOfficePermission(ModuleCodes.USER, PermissionCodes.USER_PERMISSION_VIEW, PermissionCodes.USER_PERMISSION_ASSIGN)]
        //public async Task<IActionResult> GetUserPermissions(int userId, CancellationToken cancellationToken)
        //{
        //    var response = await _userPermissionService.GetUserPermissionsAsync(userId, CurrentTenantId, cancellationToken);
        //    return StatusCode(response.Code, response);
        //}

        [HttpGet("user/{userId:int}/check/{moduleCode}/{permissionCode}")]
        [BackOfficePermission(ModuleCodes.ROLE_MODULE, PermissionCodes.ROLE_MODULE_PERMISSION_VIEW, PermissionCodes.ROLE_MODULE_PERMISSION_ASSIGN)]
        public async Task<IActionResult> CheckUserPermission(int userId, string moduleCode, string permissionCode, CancellationToken cancellationToken)
        {
            var response = await _userPermissionService.CheckUserPermissionAsync(userId, moduleCode, permissionCode, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpDelete("user/{userId:int}/role/{roleId:int}")]
        [BackOfficePermission(ModuleCodes.USER_ROLE, PermissionCodes.USER_ROLE_DELETE)]
        public async Task<IActionResult> RemoveRoleFromUser(int userId, int roleId, CancellationToken cancellationToken)
        {
            var response = await _userPermissionService.RemoveRoleFromUserAsync(userId, roleId, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpPut("user/{userId:int}/role/{roleId:int}")]
        [BackOfficePermission(ModuleCodes.USER_ROLE, PermissionCodes.USER_ROLE_ASSIGN)]
        public async Task<IActionResult> UpdateUserRole(int userId, int roleId, [FromBody] UpdateUserRoleDto dto, CancellationToken cancellationToken)
        {
            var response = await _userPermissionService.UpdateUserRoleStatusAsync(userId, roleId, dto.IsActive, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpGet("user/{userId:int}/roles")]
        [BackOfficePermission(ModuleCodes.USER_ROLE, PermissionCodes.USER_ROLE_VIEW, PermissionCodes.USER_ROLE_ASSIGN)]
        public async Task<IActionResult> GetUserRoles(int userId, CancellationToken cancellationToken)
        {
            var response = await _userPermissionService.GetUserRolesWithStatusAsync(userId, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpGet("user-roles/list")]
        [BackOfficePermission(ModuleCodes.USER_ROLE, PermissionCodes.USER_ROLE_VIEW)]
        public async Task<IActionResult> GetUserRolesList([FromQuery] UserRoleListRequest request, CancellationToken cancellationToken)
        {
            var response = await _userPermissionService.GetUserRolesPagedAsync(request, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LMS_SoulCode.Features.Common;
using LMS_SoulCode.Features.UserPermissions.DTOs;
using LMS_SoulCode.Features.UserPermissions.Services;

namespace LMS_SoulCode.Features.UserPermissions.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/role-module-permissions")]
    public class RoleModulePermissionsController : BaseApiController
    {
        private readonly RoleModulePermissionService _roleModulePermissionService;

        public RoleModulePermissionsController(RoleModulePermissionService roleModulePermissionService, ILogger<RoleModulePermissionsController> logger) : base(logger)
        {
            _roleModulePermissionService = roleModulePermissionService;
        }

        [HttpGet("list")]
        [BackOfficePermission(ModuleCodes.USER_PERMISSION, PermissionCodes.USER_PERMISSION_VIEW)]
        public async Task<IActionResult> GetPagedList([FromQuery] RoleModulePermissionListRequest request, CancellationToken cancellationToken)
        {
            var response = await _roleModulePermissionService.GetPagedAsync(request, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpGet("{id:int}")]
        [BackOfficePermission(ModuleCodes.USER_PERMISSION, PermissionCodes.USER_PERMISSION_VIEW)]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var response = await _roleModulePermissionService.GetByIdAsync(id, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }
    }
}
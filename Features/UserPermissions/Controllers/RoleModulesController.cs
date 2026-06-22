using LMS_SoulCode.Features.UserPermissions.Services;
using LMS_SoulCode.Features.UserPermissions.DTOs;
using LMS_SoulCode.Features.Common;
using Microsoft.AspNetCore.Mvc;
using static LMS_SoulCode.Features.UserPermissions.DTOs.RoleModuleDtos;

namespace LMS_SoulCode.Features.UserPermissions.Controllers
{
    [ApiController]
    [Route("api/RoleModules")]
    public class RoleModulesController : BaseApiController
    {
        private readonly RoleModuleService _roleModuleService;

        public RoleModulesController(RoleModuleService roleModuleService, ILogger<RoleModulesController> logger) : base(logger)
        {
            _roleModuleService = roleModuleService;
        }

        [HttpGet("list")]
        [BackOfficePermission(ModuleCodes.ROLE_MODULE, PermissionCodes.ROLE_MODULE_VIEW)]
        public async Task<IActionResult> GetRoleModules([FromQuery] RoleModuleListRequest request, CancellationToken cancellationToken)
        {
            var response = await _roleModuleService.GetRoleModulesAsync(request, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpGet("role/{roleId}")]
        [BackOfficePermission(ModuleCodes.ROLE_MODULE, PermissionCodes.ROLE_MODULE_VIEW)]
        public async Task<IActionResult> GetByRole(int roleId, CancellationToken cancellationToken)
        {
            var response = await _roleModuleService.GetRoleModulesByRoleAsync(roleId, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpPost("create")]
        [BackOfficePermission(ModuleCodes.ROLE_MODULE, PermissionCodes.ROLE_MODULE_ADD)]
        public async Task<IActionResult> Create([FromBody] CreateRoleModuleDto dto, CancellationToken cancellationToken)
        {
            var response = await _roleModuleService.CreateRoleModuleAsync(dto, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpDelete("delete/{id}")]
        [BackOfficePermission(ModuleCodes.ROLE_MODULE, PermissionCodes.ROLE_MODULE_DELETE)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var response = await _roleModuleService.DeleteRoleModuleAsync(id, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }
    }
}

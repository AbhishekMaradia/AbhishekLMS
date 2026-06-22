using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LMS_SoulCode.Features.Common;
using LMS_SoulCode.Features.UserPermissions.Services;
using static LMS_SoulCode.Features.UserPermissions.DTOs.RoleDtos;
using LMS_SoulCode.Features.UserPermissions.DTOs;

namespace LMS_SoulCode.Features.UserPermissions.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/Roles")]
    public class RolesController : BaseApiController
    {
        private readonly RoleService _roleService;

        public RolesController(RoleService roleService, ILogger<RolesController> logger) : base(logger)
        {
            _roleService = roleService;
        }

        [HttpGet("list")]
        [BackOfficePermission(ModuleCodes.ROLE, PermissionCodes.ROLE_VIEW)]
        public async Task<IActionResult> Get([FromQuery] RoleListRequest request, CancellationToken cancellationToken)
        {
            var response = await _roleService.GetRolesAsync(request, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpPost("create")]
        [BackOfficePermission(ModuleCodes.ROLE, PermissionCodes.ROLE_ADD)]
        public async Task<IActionResult> Create(CreateRoleDto dto, CancellationToken cancellationToken)
        {
            var targetTenantId = CurrentTenantId ?? dto.TenantId;
            var response = await _roleService.CreateRoleAsync(dto, targetTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpPut("update/{id}")]
        [BackOfficePermission(ModuleCodes.ROLE, PermissionCodes.ROLE_EDIT)]
        public async Task<IActionResult> Update(int id, UpdateRoleDto dto, CancellationToken cancellationToken)
        {
            var response = await _roleService.UpdateRoleAsync(id, dto, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpDelete("delete/{id}")]
        [BackOfficePermission(ModuleCodes.ROLE, PermissionCodes.ROLE_DELETE)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var response = await _roleService.DeleteRoleAsync(id, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LMS_SoulCode.Features.Common;
using LMS_SoulCode.Features.UserPermissions.Services;
using LMS_SoulCode.Features.UserPermissions.DTOs;
using LMS_SoulCode.Features.Common.Models;

namespace LMS_SoulCode.Features.UserPermissions.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/Permissions")]
    public class PermissionsController : BaseApiController
    {
        private readonly PermissionService _permissionService;
        // private readonly DatabaseSeeder _databaseSeeder;

        public PermissionsController(PermissionService permissionService, /*DatabaseSeeder databaseSeeder,*/ ILogger<PermissionsController> logger) : base(logger)
        {
            _permissionService = permissionService;
            // _databaseSeeder = databaseSeeder;
        }

        [HttpGet("list")]
        [BackOfficePermission(ModuleCodes.PERMISSION, PermissionCodes.PERMISSION_VIEW)]
        public async Task<IActionResult> GetAll([FromQuery] PermissionListRequest request, CancellationToken cancellationToken)
        {
            var response = await _permissionService.GetPermissionsAsync(request, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpPost("create")]
        [BackOfficePermission(ModuleCodes.PERMISSION, PermissionCodes.PERMISSION_ADD)]
        public async Task<IActionResult> Create(CreatePermissionDto dto, CancellationToken cancellationToken)
        {
            var response = await _permissionService.CreatePermissionAsync(dto, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpPut("update/{id}")]
        [BackOfficePermission(ModuleCodes.PERMISSION, PermissionCodes.PERMISSION_EDIT)]
        public async Task<IActionResult> Update(int id, UpdatePermissionDto dto, CancellationToken cancellationToken)
        {
            var response = await _permissionService.UpdatePermissionAsync(id, dto, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpDelete("delete/{id}")]
        [BackOfficePermission(ModuleCodes.PERMISSION, PermissionCodes.PERMISSION_DELETE)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var response = await _permissionService.DeletePermissionAsync(id, cancellationToken);
            return StatusCode(response.Code, response);
        }
    }
}

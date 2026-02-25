using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LMS_SoulCode.Features.Common;
using LMS_SoulCode.Features.UserPermissions.DTOs;
using LMS_SoulCode.Features.UserPermissions.Services;

namespace LMS_SoulCode.Features.UserPermissions.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/module-permissions")]
    public class ModulePermissionsController : BaseApiController
    {
        private readonly ModulePermissionService _modulePermissionService;

        public ModulePermissionsController(ModulePermissionService modulePermissionService, ILogger<ModulePermissionsController> logger) : base(logger)
        {
            _modulePermissionService = modulePermissionService;
        }

        // GET: api/module-permissions/list
        [HttpGet("list")]
        [BackOfficePermission(ModuleCodes.USER_PERMISSION, PermissionCodes.USER_PERMISSION_VIEW)]
        public async Task<IActionResult> GetPagedList([FromQuery] ModulePermissionListRequest request, CancellationToken cancellationToken)
        {
            var response = await _modulePermissionService.GetPagedAsync(request, cancellationToken);
            return StatusCode(response.Code, response);
        }

        // GET: api/module-permissions/{id}
        [HttpGet("{id:int}")]
        [BackOfficePermission(ModuleCodes.USER_PERMISSION, PermissionCodes.USER_PERMISSION_VIEW, PermissionCodes.USER_PERMISSION_ASSIGN)]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var response = await _modulePermissionService.GetByIdAsync(id, cancellationToken);
            return StatusCode(response.Code, response);
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LMS_SoulCode.Features.Common;
using LMS_SoulCode.Features.UserPermissions.Services;
using LMS_SoulCode.Features.UserPermissions.DTOs;
using LMS_SoulCode.Features.Common.Models; // For ApiResponse
using static LMS_SoulCode.Features.UserPermissions.DTOs.ModuleDtos;

namespace LMS_SoulCode.Features.UserPermissions.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/Modules")]
    public class ModulesController : BaseApiController
    {
        private readonly ModuleService _moduleService;
        private readonly DatabaseSeeder _databaseSeeder;

        public ModulesController(ModuleService moduleService, DatabaseSeeder databaseSeeder, ILogger<ModulesController> logger) : base(logger)
        {
            _moduleService = moduleService;
            _databaseSeeder = databaseSeeder;
        }

        [HttpGet("list")]
        [BackOfficePermission(ModuleCodes.MODULE, PermissionCodes.MODULE_VIEW)]
        public async Task<IActionResult> GetModules([FromQuery] ModuleListRequest request, CancellationToken cancellationToken)
        {
            var response = await _moduleService.GetModulesAsync(request, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpPost("sync")]
        [BackOfficePermission(ModuleCodes.MODULE, PermissionCodes.MODULE_EDIT)]
        public async Task<IActionResult> SyncModules()
        {
            await _databaseSeeder.SyncModulesScriptAsync();
            return Ok(new ApiResponse<string> 
            { 
                Code = 200, 
                IsSuccess = true, 
                Message = "Modules Synced Successfully",
                Data = "Sync Complete"
            });
        }

        [HttpGet("{moduleId}/permissions")]
        [BackOfficePermission(ModuleCodes.MODULE, PermissionCodes.MODULE_VIEW)]
        public async Task<IActionResult> GetModulePermissions(int moduleId, CancellationToken cancellationToken)
        {
            var response = await _moduleService.GetModulePermissionsAsync(moduleId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpPost("create")]
        [BackOfficePermission(ModuleCodes.MODULE, PermissionCodes.MODULE_ADD)]
        public async Task<IActionResult> Create(CreateModuleDto dto, CancellationToken cancellationToken)
        {
            var response = await _moduleService.CreateModuleAsync(dto, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpPut("update/{id}")]
        [BackOfficePermission(ModuleCodes.MODULE, PermissionCodes.MODULE_EDIT)]
        public async Task<IActionResult> Update(int id, UpdateModuleDto dto, CancellationToken cancellationToken)
        {
            var response = await _moduleService.UpdateModuleAsync(id, dto, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpDelete("delete/{id}")]
        [BackOfficePermission(ModuleCodes.MODULE, PermissionCodes.MODULE_DELETE)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var response = await _moduleService.DeleteModuleAsync(id, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpPost("assign-permissions")]
        [BackOfficePermission(ModuleCodes.MODULE, PermissionCodes.MODULE_EDIT)]
        public async Task<IActionResult> AssignPermissions(AssignModulePermissionsDto dto, CancellationToken cancellationToken)
        {
            var response = await _moduleService.AssignPermissionsToModuleAsync(dto, cancellationToken);
            return StatusCode(response.Code, response);
        }
    }
}

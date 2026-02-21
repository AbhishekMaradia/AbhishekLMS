using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LMS_SoulCode.Features.Common;
using LMS_SoulCode.Features.Groups.DTOs;
using LMS_SoulCode.Features.Groups.Services;
using StatusCodes = LMS_SoulCode.Features.Common.StatusCodes;
namespace LMS_SoulCode.Features.Groups.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class GroupsController : BaseApiController
    {
        private readonly IGroupService _groupService;

        public GroupsController(IGroupService groupService, ILogger<GroupsController> logger) : base(logger)
        {
            _groupService = groupService;
        }

        [HttpPost("create")]
        [BackOfficePermission(ModuleCodes.GROUP, PermissionCodes.GROUP_ADD)]
        public async Task<IActionResult> CreateGroup([FromBody] CreateGroupRequest request, CancellationToken cancellationToken)
        {
            // If SuperAdmin (null tenant), use TenantId from request if provided. 
            // If OrgAdmin, CurrentTenantId will override whatever they sent in request.
            var targetTenantId = CurrentTenantId ?? request.TenantId;

            var response = await _groupService.CreateGroupAsync(request, targetTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpGet("list")]
        [BackOfficePermission(ModuleCodes.GROUP, PermissionCodes.GROUP_VIEW)]
        public async Task<IActionResult> GetGroups([FromQuery] GroupListRequest request, CancellationToken cancellationToken)
        {
            // Security: Org Admin can only see their own tenant, SuperAdmin can specify or see all
            var targetTenantId = CurrentTenantId.HasValue 
                ? CurrentTenantId              // Org Admin - force their own tenant
                : request.TenantId;            // SuperAdmin - use request or null
            
            var response = await _groupService.GetGroupsAsync(request, targetTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpGet("{groupId}")]
        [BackOfficePermission(ModuleCodes.GROUP, PermissionCodes.GROUP_VIEW)]
        public async Task<IActionResult> GetGroupById([FromRoute] int groupId, CancellationToken cancellationToken)
        {
            var response = await _groupService.GetGroupByIdAsync(groupId, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpDelete("Delete/{groupId}")]
        [BackOfficePermission(ModuleCodes.GROUP, PermissionCodes.GROUP_DELETE)]
        public async Task<IActionResult> DeleteGroup([FromRoute] int groupId, CancellationToken cancellationToken)
        {
            var response = await _groupService.DeleteGroupAsync(groupId, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpPut("update/{groupId}")]
        [BackOfficePermission(ModuleCodes.GROUP, PermissionCodes.GROUP_EDIT)]
        public async Task<IActionResult> UpdateGroup([FromRoute] int groupId, [FromBody] UpdateGroupRequest request, CancellationToken cancellationToken)
        {
            // Ensure Id from request body matches route if provided
            if (request.Id != 0 && request.Id != groupId)
            {
                return BadRequest(ApiResponse<string>.Fail("Group ID mismatch", StatusCodes.BadRequest));
            }

            var response = await _groupService.UpdateGroupAsync(groupId, request, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpGet("courses-edit/{groupId}")]
        [BackOfficePermission(ModuleCodes.GROUP, PermissionCodes.GROUP_EDIT)]
        public async Task<IActionResult> GetGroupCoursesForEdit([FromRoute] int groupId, CancellationToken cancellationToken)
        {
            var response = await _groupService.GetGroupCoursesForEditAsync(groupId, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }
    }
}

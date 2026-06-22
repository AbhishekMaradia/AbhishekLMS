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
        [BackOfficePermission(ModuleCodes.GROUP, PermissionCodes.GROUP_VIEW, PermissionCodes.GROUP_EDIT)]
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
            if (request.Id != 0 && request.Id != groupId)
            {
                return BadRequest(ApiResponse<string>.Fail("Group ID mismatch", StatusCodes.BadRequest));
            }

            var response = await _groupService.UpdateGroupAsync(groupId, request, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpGet("group-courses/{groupId}")]
        [BackOfficePermission(ModuleCodes.GROUP, PermissionCodes.GROUP_VIEW, PermissionCodes.GROUP_COURSE_EDIT)]
        public async Task<IActionResult> GetGroupCoursesByGroupId([FromRoute] int groupId, [FromQuery] GroupCourseListRequest request, CancellationToken cancellationToken)
        {
            var response = await _groupService.GetGroupCoursesByGroupIdAsync(groupId, request, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpPut("bulk-update-courses")]
        [BackOfficePermission(ModuleCodes.GROUP, PermissionCodes.GROUP_COURSE_EDIT)]
        public async Task<IActionResult> BulkUpdateCourses([FromBody] BulkUpdateCoursesRequest request, CancellationToken cancellationToken)
        {
            var response = await _groupService.BulkUpdateGroupCoursesAsync(request, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpGet("group-users/{groupId}")]
        [BackOfficePermission(ModuleCodes.GROUP, PermissionCodes.GROUP_VIEW, PermissionCodes.GROUP_USER_EDIT)]
        public async Task<IActionResult> GetGroupUsers([FromRoute] int groupId, CancellationToken cancellationToken)
        {
            var response = await _groupService.GetGroupUsersAsync(groupId, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpPut("assign-users")]
        [BackOfficePermission(ModuleCodes.GROUP, PermissionCodes.GROUP_USER_EDIT)]
        public async Task<IActionResult> BulkAssignUsers([FromBody] BulkAssignUsersRequest request, CancellationToken cancellationToken)
        {
            var response = await _groupService.BulkAssignUsersAsync(request, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }
    }
}

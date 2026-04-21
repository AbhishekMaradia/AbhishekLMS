using LMS_SoulCode.Features.SubscribedCourse.Services;
using LMS_SoulCode.Features.SubscribedCourse.DTOs;
using LMS_SoulCode.Features.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS_SoulCode.Features.SubscribedCourse.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserCourseController : BaseApiController
    {
        private readonly IUserCourseService _service;
        public UserCourseController(IUserCourseService service, ILogger<UserCourseController> logger) : base(logger) => _service = service;

        [HttpPost("subscribe")]
        public async Task<IActionResult> Subscribe([FromBody] SubscribeRequest req, CancellationToken cancellationToken)
        {
            var response = await _service.SubscribeAsync(CurrentUserId ?? 0, req.CourseId, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpPost("unsubscribe")]
        public async Task<IActionResult> Unsubscribe([FromBody] SubscribeRequest req, CancellationToken cancellationToken)
        {
            var response = await _service.UnsubscribeAsync(CurrentUserId ?? 0, req.CourseId, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpGet("my-courses")]
        public async Task<IActionResult> MyCourses(CancellationToken cancellationToken)
        {
            var response = await _service.GetUserCoursesAsync(CurrentUserId ?? 0, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpGet("Subscribed-List")]
        [BackOfficePermission(ModuleCodes.SUBSCRIPTION, PermissionCodes.SUBSCRIPTION_VIEW)]
        public async Task<IActionResult> GetAllSubscribed(CancellationToken cancellationToken)
        {
            var response = await _service.GetAllSubscribedAsync(CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpGet("check/{courseId}")]
        public async Task<IActionResult> Check(int courseId, CancellationToken cancellationToken)
        {
            var response = await _service.IsSubscribedAsync(CurrentUserId ?? 0, courseId, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpGet("list")]
        [BackOfficePermission(ModuleCodes.SUBSCRIPTION, PermissionCodes.SUBSCRIPTION_VIEW)]
        public async Task<IActionResult> GetUserCourses([FromQuery] UserCourseListRequest request, CancellationToken cancellationToken)
        {
            var response = await _service.GetUserCoursesAsync(request, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpPost("revoke")]
        [BackOfficePermission(ModuleCodes.SUBSCRIPTION, PermissionCodes.SUBSCRIPTION_DELETE)]
        public async Task<IActionResult> Revoke([FromBody] RevokeRequest req, CancellationToken cancellationToken)
        {
            var response = await _service.UnsubscribeAsync(req.UserId, req.CourseId, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }
    }

    public record SubscribeRequest(int CourseId);
    public record RevokeRequest(int UserId, int CourseId);
}

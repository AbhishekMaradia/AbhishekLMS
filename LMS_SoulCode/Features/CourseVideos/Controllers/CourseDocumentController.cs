using LMS_SoulCode.Features.Common;
using LMS_SoulCode.Features.CourseVideos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS_SoulCode.Features.CourseVideos.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CourseDocumentController : BaseApiController
    {
        private readonly CourseDocumentService _service;

        public CourseDocumentController(CourseDocumentService service, ILogger<CourseDocumentController> logger) : base(logger)
        {
            _service = service;
        }

        [HttpGet("course/{courseId}")]
        [BackOfficePermission(ModuleCodes.COURSE, PermissionCodes.COURSE_VIEW)]
        public async Task<IActionResult> GetByCourse(int courseId, CancellationToken cancellationToken)
        {
            int? tenantId = null;
            var tenantClaim = User.FindFirst("TenantId");
            if (tenantClaim != null && int.TryParse(tenantClaim.Value, out int tid))
            {
                tenantId = tid;
            }

            var response = await _service.GetByCourseAsync(courseId, tenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }
    }
}

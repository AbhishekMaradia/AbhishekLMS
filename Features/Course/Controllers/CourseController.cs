using LMS_SoulCode.Features.Course.DTOs;
using LMS_SoulCode.Features.Course.Services;
using LMS_SoulCode.Features.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StatusCodes = LMS_SoulCode.Features.Common.StatusCodes;
using LMS_SoulCode.Features.CourseVideos.Services;

namespace LMS_SoulCode.Features.Course.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CourseController : BaseApiController
    {
        private readonly ICourseService _courseService;
        private readonly CourseVideoService _videoService;
        private readonly CourseDocumentService _docService;
        private readonly IWebHostEnvironment _env;

        public CourseController(
            ICourseService courseService, 
            CourseVideoService videoService,
            CourseDocumentService docService,
            IWebHostEnvironment env, 
            ILogger<CourseController> logger) : base(logger)
        {
            _courseService = courseService;
            _videoService = videoService;
            _docService = docService;
            _env = env;
        }

        [HttpGet("list")]
        [BackOfficePermission(ModuleCodes.COURSE, PermissionCodes.COURSE_VIEW)]
        public async Task<IActionResult> GetCourseAll([FromQuery] CourseListRequest request, CancellationToken cancellationToken)
        {
            var response = await _courseService.GetAllCourseAsync(request, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpGet("{id}")]
        [BackOfficePermission(ModuleCodes.COURSE, PermissionCodes.COURSE_VIEW, PermissionCodes.COURSE_EDIT)]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var response = await _courseService.GetByIdAsync(id, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpGet("category/{categoryId}")]
        [BackOfficePermission(ModuleCodes.COURSE, PermissionCodes.COURSE_VIEW)]
        public async Task<IActionResult> GetByCategory(int categoryId, CancellationToken cancellationToken)
        {
            var response = await _courseService.GetCourseByCategoryIdAsync(categoryId, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpPost("create")]
        [BackOfficePermission(ModuleCodes.COURSE, PermissionCodes.COURSE_ADD)]
        public async Task<IActionResult> Create([FromForm] CourseRequest request, CancellationToken cancellationToken)
        {
            if (request == null) return BadRequest(ApiResponse<string>.Fail("Request body is missing.", StatusCodes.BadRequest));
            var targetTenantId = CurrentTenantId ?? request.TenantId ?? 0;
            var response = await _courseService.AddAsync(request, targetTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpPut("update/{id}")]
        [BackOfficePermission(ModuleCodes.COURSE, PermissionCodes.COURSE_EDIT)]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateCourseRequest request, CancellationToken cancellationToken)
        {
            var response = await _courseService.UpdateAsync(id, request, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpDelete("delete/{id}")]
        [BackOfficePermission(ModuleCodes.COURSE, PermissionCodes.COURSE_DELETE)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var response = await _courseService.DeleteAsync(id, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpPost("{courseId}/upload-video")]
        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = 1_610_612_736, ValueLengthLimit = int.MaxValue)]
        [BackOfficePermission(ModuleCodes.COURSE, PermissionCodes.COURSE_EDIT)]
        public async Task<IActionResult> UploadVideo(int courseId, [FromForm] IFormFile file, [FromForm] string title, [FromForm] string description, CancellationToken cancellationToken)
        {
            var response = await _videoService.UploadAsync(courseId, file, title, description, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpPost("{courseId}/upload-document")]
        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = 1_610_612_736, ValueLengthLimit = int.MaxValue)]
        [BackOfficePermission(ModuleCodes.COURSE, PermissionCodes.COURSE_EDIT)]
        public async Task<IActionResult> UploadDocument(int courseId, [FromForm] IFormFile file, [FromForm] string? docName, [FromForm] string? description, CancellationToken cancellationToken)
        {
            var response = await _docService.UploadAsync(courseId, file, docName ?? string.Empty, description ?? string.Empty, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpGet("{courseId}/documents")]
        [BackOfficePermission(ModuleCodes.COURSE, PermissionCodes.COURSE_VIEW)]
        public async Task<IActionResult> GetDocuments(int courseId, CancellationToken cancellationToken)
        {
            var response = await _docService.GetByCourseAsync(courseId, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpDelete("delete-document/{documentId}")]
        [BackOfficePermission(ModuleCodes.COURSE, PermissionCodes.COURSE_DELETE)]
        public async Task<IActionResult> DeleteDocument(int documentId, CancellationToken cancellationToken)
        {
            var response = await _docService.DeleteAsync(documentId, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }
    }
}

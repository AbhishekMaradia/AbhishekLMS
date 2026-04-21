using LMS_SoulCode.Features.Common;
using LMS_SoulCode.Features.CourseVideos.Services;
using LMS_SoulCode.Features.CourseVideos.DTOs;
using LMS_SoulCode.Features.Security.Services;
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
        private readonly CryptographyService _cryptoService;
        private readonly IWebHostEnvironment _env;

        public CourseDocumentController(
            CourseDocumentService service, 
            CryptographyService cryptographyService,
            IWebHostEnvironment env,
            ILogger<CourseDocumentController> logger) : base(logger)
        {
            _service = service;
            _cryptoService = cryptographyService;
            _env = env;
        }

        [HttpGet("course/{courseId}")]
        // [BackOfficePermission(ModuleCodes.COURSE, PermissionCodes.COURSE_VIEW, PermissionCodes.COURSE_EDIT)]
        public async Task<IActionResult> GetByCourse(int courseId, CancellationToken cancellationToken)
        {
            var response = await _service.GetByCourseAsync(courseId, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpPost("upload/{courseId}")]
        [BackOfficePermission(ModuleCodes.COURSE, PermissionCodes.COURSE_EDIT)]
        public async Task<IActionResult> Upload(int courseId, [FromForm] IFormFile file, [FromForm] string? docName, [FromForm] string? description, CancellationToken cancellationToken)
        {
            var response = await _service.UploadAsync(courseId, file, docName ?? string.Empty, description ?? string.Empty, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpDelete("delete/{documentId}")]
        [BackOfficePermission(ModuleCodes.COURSE, PermissionCodes.COURSE_DELETE)]
        public async Task<IActionResult> Delete(int documentId, CancellationToken cancellationToken)
        {
            var response = await _service.DeleteAsync(documentId, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpGet("download/{documentId}")]
        // [BackOfficePermission(ModuleCodes.COURSE, PermissionCodes.COURSE_VIEW)]
        public async Task Download(int documentId, CancellationToken cancellationToken)
        {
            try
            {
                var doc = await _service.GetRawByIdAsync(documentId, CurrentTenantId, cancellationToken);
                
                if (doc == null)
                {
                    Response.StatusCode = 404;
                    return;
                }

                var fullPath = Path.Combine(_env.ContentRootPath, doc.DocUrl.TrimStart('/'));

                if (!System.IO.File.Exists(fullPath))
                {
                    Response.StatusCode = 404;
                    return;
                }

                var stream = _cryptoService.GetDecryptStream(fullPath);
                
                // Determine content type
                string ext = Path.GetExtension(doc.DocName).ToLower();
                if (string.IsNullOrEmpty(ext)) ext = Path.GetExtension(doc.DocUrl.Replace(".enc", "")).ToLower();
                
                Response.ContentType = ext switch
                {
                    ".pdf" => "application/pdf",
                    ".doc" => "application/msword",
                    ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    ".xls" => "application/vnd.ms-excel",
                    ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    ".png" => "image/png",
                    ".jpg" or ".jpeg" => "image/jpeg",
                    _ => "application/octet-stream"
                };

                await stream.CopyToAsync(Response.Body, cancellationToken);
            }
            catch
            {
                if (!Response.HasStarted)
                    Response.StatusCode = 500;
            }
        }

        [HttpPut("update/{documentId}")]
        [BackOfficePermission(ModuleCodes.COURSE, PermissionCodes.COURSE_EDIT)]
        public async Task<IActionResult> Update(int documentId, [FromBody] UpdateDocumentRequest request, CancellationToken cancellationToken)
        {
            var response = await _service.UpdateAsync(documentId, request.DocName, request.Description, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }
    }
}

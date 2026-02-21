using LMS_SoulCode.Features.CourseVideos.Services;
using LMS_SoulCode.Features.CourseVideos.DTOs;
using LMS_SoulCode.Features.SubscribedCourse.Services;
using LMS_SoulCode.Features.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StatusCodes = LMS_SoulCode.Features.Common.StatusCodes;
using LMS_SoulCode.Features.Security.Services;

namespace LMS_SoulCode.Features.CourseVideos.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CourseVideoController : BaseApiController
    {
        private readonly CourseVideoService _courseVideoService;
        private readonly IUserCourseService _userCourseService;
        private readonly CryptographyService _cryptoService;
        private readonly IWebHostEnvironment _env;

        public CourseVideoController(
            CourseVideoService courseVideoService, 
            IUserCourseService userCourseService,
            CryptographyService cryptographyService,
            IWebHostEnvironment env,
            ILogger<CourseVideoController> logger) : base(logger)
        {
            _courseVideoService = courseVideoService;
            _userCourseService = userCourseService;
            _cryptoService = cryptographyService;
            _env = env;
        }

        [HttpGet("list/{courseId}")]
        [BackOfficePermission(ModuleCodes.VIDEO, PermissionCodes.VIDEO_VIEW)]
        public async Task<IActionResult> GetVideos(int courseId, [FromQuery] string? searchTerm = null, [FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null, CancellationToken cancellationToken = default)
        {
            if (pageNumber.HasValue || pageSize.HasValue)
            {
                var request = new CourseVideosByCourseRequest
                {
                    CourseId = courseId,
                    SearchTerm = searchTerm,
                    PageNumber = pageNumber ?? 1,
                    PageSize = pageSize ?? 10
                };

                var paginatedResponse = await _courseVideoService.GetByCourseAsync(request, CurrentTenantId, cancellationToken);
                return StatusCode(paginatedResponse.Code, paginatedResponse);
            }

            var response = await _courseVideoService.GetByCourseAsync(courseId, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpGet("coursevideolist")]
        [BackOfficePermission(ModuleCodes.VIDEO, PermissionCodes.VIDEO_VIEW)]
        public async Task<IActionResult> GetAllCourseVideos([FromQuery] string? searchTerm = null, [FromQuery] int? courseId = null, [FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null, CancellationToken cancellationToken = default)
        {
            if (pageNumber.HasValue || pageSize.HasValue)
            {
                var request = new CourseVideoListRequest
                {
                    SearchTerm = searchTerm,
                    CourseId = courseId,
                    PageNumber = pageNumber ?? 1,
                    PageSize = pageSize ?? 10
                };

                var paginatedResponse = await _courseVideoService.GetCourseVideosAsync(request, CurrentTenantId, cancellationToken);
                return StatusCode(paginatedResponse.Code, paginatedResponse);
            }

            var response = await _courseVideoService.GetAllCourseVideoAsync(CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpGet("list")]
        [BackOfficePermission(ModuleCodes.VIDEO, PermissionCodes.VIDEO_VIEW)]
        public async Task<IActionResult> GetCourseVideos([FromQuery] CourseVideoListRequest request, CancellationToken cancellationToken)
        {
            var response = await _courseVideoService.GetCourseVideosAsync(request, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpGet("stream/{videoId}")]
        // Removed [AllowAnonymous] to enforce strict security
        public async Task Stream(int videoId, CancellationToken cancellationToken)
        {
            try
            {
                // Strict Security:
                // 1. User must be logged in (Controller is [Authorize])
                // 2. Video must belong to the User's Tenant (handled by Service->Repo check using CurrentTenantId)
                var video = await _courseVideoService.GetRawByIdAsync(videoId, CurrentTenantId, cancellationToken);
                
                if (video == null)
                {
                    Response.StatusCode = 404; // Video not found or Access Denied (different tenant)
                    return;
                }

                var rootPath = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
                var fullPath = Path.Combine(rootPath, video.VideoUrl.TrimStart('/'));

                if (!System.IO.File.Exists(fullPath))
                {
                    Response.StatusCode = 404;
                    return;
                }

                string encryptedBase64 = await System.IO.File.ReadAllTextAsync(fullPath, cancellationToken);
                
                Response.ContentType = "video/mp4";
                
                // Decrypt and stream directly to the response
                await _cryptoService.DecryptToStreamAsync(encryptedBase64, Response.Body, cancellationToken);
            }
            catch
            {
                if (!Response.HasStarted)
                    Response.StatusCode = 500;
            }
        }

        [HttpPost("update-progress")]
        [BackOfficePermission(ModuleCodes.VIDEO, PermissionCodes.VIDEO_EDIT)]
        public async Task<IActionResult> UpdateProgress([FromBody] UpdateProgressRequest request, CancellationToken cancellationToken)
        {
            if (!CurrentUserId.HasValue)
                return StatusCode(StatusCodes.Unauthorized, ApiResponse<List<string>>.Fail("Invalid user", StatusCodes.Unauthorized));

            var response = await _courseVideoService.UpdateProgressAsync(CurrentUserId.Value, request.VideoId, request.WatchedPercentage, request.IsCompleted, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpGet("progress/{videoId}")]
        [BackOfficePermission(ModuleCodes.VIDEO, PermissionCodes.VIDEO_VIEW)]
        public async Task<IActionResult> GetProgress(int videoId, CancellationToken cancellationToken)
        {
            if (!CurrentUserId.HasValue)
                return StatusCode(StatusCodes.Unauthorized, ApiResponse<List<string>>.Fail("Invalid user", StatusCodes.Unauthorized));

            var response = await _courseVideoService.GetProgressAsync(CurrentUserId.Value, videoId, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }
    }
}

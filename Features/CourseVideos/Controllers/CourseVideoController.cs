using LMS_SoulCode.Features.CourseVideos.Services;
using System.Security.Cryptography;
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
        // [BackOfficePermission(ModuleCodes.VIDEO, Permis2sionCodes.VIDEO_VIEW)]
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
        public async Task Stream(int videoId, CancellationToken cancellationToken)
        {
            try
            {
                // Strict Security:
                // 1. User must be logged in (Controller is [Authorize])
                // 2. Video must belong to the User's Tenant (handled by GetRawByIdAsync using CurrentTenantId)
                var video = await _courseVideoService.GetRawByIdAsync(videoId, CurrentTenantId, cancellationToken);
                
                if (video == null)
                {
                    Response.StatusCode = 403; // Forbidden or Not Found
                    return;
                }

                var fullPath = Path.Combine(_env.ContentRootPath, video.VideoUrl.TrimStart('/'));

                using var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                
                // --- SMART LENGTH CALCULATION ---
                // We read the last 32 bytes to determine the exact PKCS7 padding size
                // and report 100% accurate Content-Length to the browser.
                long decryptedLength = fileStream.Length - 16; // Default
                if (fileStream.Length >= 32)
                {
                    byte[] lastTwoBlocks = new byte[32];
                    fileStream.Seek(-32, SeekOrigin.End);
                    await fileStream.ReadAsync(lastTwoBlocks, 0, 32, cancellationToken);
                    fileStream.Seek(0, SeekOrigin.Begin); // Reset for streaming

                    using var aesLen = Aes.Create();
                    aesLen.Key = _cryptoService.Key;
                    aesLen.IV = lastTwoBlocks.Take(16).ToArray(); // Previous block is IV for last block
                    aesLen.Padding = PaddingMode.None; // Manual padding check
                    using var decryptorLen = aesLen.CreateDecryptor();
                    byte[] lastBlockDec = decryptorLen.TransformFinalBlock(lastTwoBlocks.Skip(16).ToArray(), 0, 16);
                    
                    int paddingSize = lastBlockDec[15]; // PKCS7: last byte is the padding size
                    if (paddingSize >= 1 && paddingSize <= 16) decryptedLength -= paddingSize;
                }

                using var stream = _cryptoService.GetDecryptStream(fullPath);
                
                Response.ContentType = "video/mp4";
                Response.ContentLength = decryptedLength; 
                Response.Headers["Accept-Ranges"] = "none";

                _logger.LogInformation("[LMS_STREAM] Starting stream for {VideoId}. Exact Size: {Size} bytes", videoId, decryptedLength);

                byte[] buffer = new byte[1048576]; // 1MB buffer
                long totalBytesSent = 0;
                int bytesRead;

                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                {
                    // Ensure we don't send padding bytes if any (though CryptoStream usually handles this, 
                    // providing exact Content-Length is the key for Chrome)
                    await Response.Body.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                    await Response.Body.FlushAsync(cancellationToken); 
                    totalBytesSent += bytesRead;
                }

                _logger.LogInformation("[LMS_STREAM] Successfully completed stream for {VideoId}", videoId);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("[LMS_STREAM] Playback cancelled for {VideoId}", videoId);
            }
        }

        [HttpPost("update-progress")]
        // [BackOfficePermission(ModuleCodes.VIDEO, PermissionCodes.VIDEO_EDIT)]
        public async Task<IActionResult> UpdateProgress([FromBody] UpdateProgressRequest request, CancellationToken cancellationToken)
        {
            if (!CurrentUserId.HasValue)
                return StatusCode(StatusCodes.Unauthorized, ApiResponse<List<string>>.Fail("Invalid user", StatusCodes.Unauthorized));

            var response = await _courseVideoService.UpdateProgressAsync(CurrentUserId.Value, request.VideoId, request.WatchedPercentage, request.IsCompleted, request.GroupId, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpGet("progress/{videoId}")]
        // [BackOfficePermission(ModuleCodes.VIDEO, PermissionCodes.VIDEO_VIEW, PermissionCodes.VIDEO_EDIT)]
        public async Task<IActionResult> GetProgress(int videoId, CancellationToken cancellationToken)
        {
            if (!CurrentUserId.HasValue)
                return StatusCode(StatusCodes.Unauthorized, ApiResponse<List<string>>.Fail("Invalid user", StatusCodes.Unauthorized));

            var response = await _courseVideoService.GetProgressAsync(CurrentUserId.Value, videoId, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }
        [HttpDelete("delete/{videoId}")]
        [BackOfficePermission(ModuleCodes.VIDEO, PermissionCodes.VIDEO_DELETE)]
        public async Task<IActionResult> Delete(int videoId, CancellationToken cancellationToken)
        {
            var response = await _courseVideoService.DeleteAsync(videoId, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpPost("upload/{courseId}")]
        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = 1_610_612_736, ValueLengthLimit = int.MaxValue)]
        [BackOfficePermission(ModuleCodes.VIDEO, PermissionCodes.VIDEO_ADD)]
        public async Task<IActionResult> Upload(int courseId, [FromForm] IFormFile file, [FromForm] string title, [FromForm] string description, CancellationToken cancellationToken)
        {
            var response = await _courseVideoService.UploadAsync(courseId, file, title, description, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }

        [HttpPut("update/{videoId}")]
        [BackOfficePermission(ModuleCodes.VIDEO, PermissionCodes.VIDEO_EDIT)]
        public async Task<IActionResult> Update(int videoId, [FromBody] UpdateVideoRequest request, CancellationToken cancellationToken)
        {
            var response = await _courseVideoService.UpdateAsync(videoId, request.Title, request.Description, CurrentTenantId, cancellationToken);
            return StatusCode(response.Code, response);
        }
    }
}

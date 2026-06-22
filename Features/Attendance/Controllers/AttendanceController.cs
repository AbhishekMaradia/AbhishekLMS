using LMS_SoulCode.Features.Attendance.DTOs;
using LMS_SoulCode.Features.Attendance.Services;
using LMS_SoulCode.Features.Auth;
using LMS_SoulCode.Features.Common;
using LMS_SoulCode.Features.Security.Services;
using Microsoft.AspNetCore.Mvc;

namespace LMS_SoulCode.Features.Attendance.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [BackOfficePermission(ModuleCodes.ATTENDANCE)]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;
        private readonly CryptographyService _cryptoService;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        public AttendanceController(IAttendanceService attendanceService, CryptographyService cryptoService, IWebHostEnvironment env, IConfiguration config)
        {
            _attendanceService = attendanceService;
            _cryptoService = cryptoService;
            _env = env;
            _config = config;
        }

        [HttpPost("submit")]
        [BackOfficePermission(ModuleCodes.ATTENDANCE, PermissionCodes.ATTENDANCE_EDIT)]
        public async Task<ActionResult<ApiResponse<string>>> SubmitAttendance([FromForm] SubmitAttendanceDto dto, [FromForm] List<IFormFile> files)
        {
            if (dto == null || dto.Records == null || !dto.Records.Any()) 
                return BadRequest(ApiResponse<string>.Fail(Messages.NoAttendanceRecords, LMS_SoulCode.Features.Common.StatusCodes.BadRequest));

            if (dto.GroupId <= 0 || dto.CourseId <= 0)
                return BadRequest(ApiResponse<string>.Fail(Messages.AttendanceModuleRequired, LMS_SoulCode.Features.Common.StatusCodes.BadRequest));

            try 
            {
                // File processing logic - Independent from Course Videos
                if (files != null && files.Any())
                {
                    var folderPath = _config["AppSettings:AttendanceDocsPath"] ?? "wwwroot/uploads/attendance";
                    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                    foreach (var file in files)
                    {
                        string extension = Path.GetExtension(file.FileName).ToLower();
                        string fileName = Guid.NewGuid().ToString() + extension + ".enc";
                        string thumbName = Guid.NewGuid().ToString() + "_thumb" + extension + ".enc";
                        
                        string filePath = Path.Combine(folderPath, fileName);
                        string thumbPath = Path.Combine(folderPath, thumbName);

                        string? docUrl = null;
                        string? thumbUrl = null;

                        // Check if it's an image to generate thumb
                        var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
                        if (imageExtensions.Contains(extension))
                        {
                            using var stream = file.OpenReadStream();
                            var thumbSize = _config.GetValue<int>("AppSettings:OrgLogoThumbSize", 150);
                            var (mainBytes, thumbBytes) = await _cryptoService.ProcessImageWithThumbAsync(stream, thumbSize, default);

                            // Encrypt Main
                            using (var mainMs = new MemoryStream(mainBytes))
                                await _cryptoService.EncryptLargeFileAsync(mainMs, filePath);

                            // Encrypt Thumb
                            using (var thumbMs = new MemoryStream(thumbBytes))
                                await _cryptoService.EncryptLargeFileAsync(thumbMs, thumbPath);

                            var gateway = _config["AppSettings:AttendanceDocUrl"] ?? "/api/Crypto/get?path=";
                            docUrl = $"{gateway}{Uri.EscapeDataString(Path.Combine(folderPath, fileName).Replace("\\", "/"))}";
                            thumbUrl = $"{gateway}{Uri.EscapeDataString(Path.Combine(folderPath, thumbName).Replace("\\", "/"))}";
                        }
                        else 
                        {
                            // Non-image document
                            using (var inputStream = file.OpenReadStream())
                                await _cryptoService.EncryptLargeFileAsync(inputStream, filePath);

                            var gateway = _config["AppSettings:AttendanceDocUrl"] ?? "/api/Crypto/get?path=";
                            docUrl = $"{gateway}{Uri.EscapeDataString(Path.Combine(folderPath, fileName).Replace("\\", "/"))}";
                        }

                        if (file.FileName.StartsWith("file_"))
                        {
                            string idPart = file.FileName.Split('_')[1];
                            if (idPart.Contains('.'))
                            {
                                idPart = idPart.Split('.')[0];
                            }
                            if (int.TryParse(idPart, out int userId))
                            {
                                var record = dto.Records.FirstOrDefault(r => r.Id == userId);
                                if (record != null) 
                                {
                                    record.DocumentUrl = docUrl;
                                    record.ThumbUrl = thumbUrl;
                                }
                            }
                        }
                    }
                }

                var result = await _attendanceService.SubmitAttendanceAsync(dto);
                
                if (result)
                    return Ok(ApiResponse<string>.Success(Messages.Success));
                
                return StatusCode(500, ApiResponse<string>.Fail(Messages.Error, LMS_SoulCode.Features.Common.StatusCodes.ServerError));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail($"{Messages.Error}: {ex.Message}", LMS_SoulCode.Features.Common.StatusCodes.ServerError));
            }
        }

        [HttpGet("list")]
        [BackOfficePermission(ModuleCodes.ATTENDANCE, PermissionCodes.ATTENDANCE_VIEW)]
        public async Task<ActionResult<ApiResponse<IEnumerable<AttendanceListDto>>>> GetAttendances([FromQuery] int? tenantId, [FromQuery] int? groupId, [FromQuery] int? courseId)
        {
            try 
            {
                var result = await _attendanceService.GetAllAttendancesAsync(tenantId, default);
                
                if (groupId.HasValue && groupId.Value > 0)
                    result = result.Where(a => a.GroupId == groupId.Value);
                
                if (courseId.HasValue && courseId.Value > 0)
                    result = result.Where(a => a.CourseId == courseId.Value);
                
                return Ok(ApiResponse<IEnumerable<AttendanceListDto>>.Success(result, Messages.Success));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<AttendanceListDto>>.Fail($"{Messages.Error}: {ex.Message}", LMS_SoulCode.Features.Common.StatusCodes.ServerError));
            }
        }
        [HttpGet("get")]
        [BackOfficePermission(ModuleCodes.ATTENDANCE, PermissionCodes.ATTENDANCE_VIEW)]
        public async Task<ActionResult<ApiResponse<IEnumerable<AttendanceListDto>>>> GetAttendanceByFilters([FromQuery] int groupId, [FromQuery] int courseId, [FromQuery] string date)
        {
            try 
            {
                var result = await _attendanceService.GetAttendanceByFiltersAsync(groupId, courseId, date);
                return Ok(ApiResponse<IEnumerable<AttendanceListDto>>.Success(result, Messages.Success));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<AttendanceListDto>>.Fail($"{Messages.Error}: {ex.Message}", LMS_SoulCode.Features.Common.StatusCodes.ServerError));
            }
        }
        [HttpDelete("delete/{id}")]
        [BackOfficePermission(ModuleCodes.ATTENDANCE, PermissionCodes.ATTENDANCE_DELETE)]
        public async Task<ActionResult<ApiResponse<string>>> DeleteAttendance(int id, CancellationToken cancellationToken)
        {
            try 
            {
                var result = await _attendanceService.DeleteAttendanceAsync(id, cancellationToken);
                if (result)
                    return Ok(ApiResponse<string>.Success(Messages.Success));
                
                return NotFound(ApiResponse<string>.Fail("Record not found", LMS_SoulCode.Features.Common.StatusCodes.NotFound));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail($"{Messages.Error}: {ex.Message}", LMS_SoulCode.Features.Common.StatusCodes.ServerError));
            }
        }

        // ============================================================
        // SECURE DOCUMENT STREAMING — same pattern as CryptoController
        // ============================================================
        [HttpGet("view/{id}")]
        [BackOfficePermission(ModuleCodes.ATTENDANCE, PermissionCodes.ATTENDANCE_VIEW)]
        public async Task<IActionResult> ViewAttachment(int id, CancellationToken cancellationToken)
        {
            try
            {
                // 1. Fetch attendance record to get DocumentUrl
                var allRecords = await _attendanceService.GetAllAttendancesAsync(null, cancellationToken);
                var record = allRecords.FirstOrDefault(a => a.Id == id);

                if (record == null || string.IsNullOrWhiteSpace(record.DocumentUrl))
                    return NotFound("Attachment not found for this attendance record.");

                // 2. Extract raw file path from stored URL
                //    DocumentUrl format: "/api/Crypto/get?path=wwwroot/uploads/attendance/..."
                var storedUrl = record.DocumentUrl;
                string rawPath;

                if (storedUrl.Contains("?path="))
                    rawPath = Uri.UnescapeDataString(storedUrl.Split("?path=")[1].Split('&')[0]);
                else
                    rawPath = storedUrl.TrimStart('/');

                // 3. Resolve physical path
                var normalizedPath = rawPath
                    .Replace("/", Path.DirectorySeparatorChar.ToString())
                    .TrimStart(Path.DirectorySeparatorChar);

                var rootPath = _env.ContentRootPath;
                var fullPath = Path.GetFullPath(Path.Combine(rootPath, normalizedPath));

                // Security: prevent path traversal
                if (!fullPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
                    return BadRequest("Invalid path.");

                if (!System.IO.File.Exists(fullPath))
                {
                    // Try wwwroot sub-path fallback
                    if (normalizedPath.StartsWith("wwwroot", StringComparison.OrdinalIgnoreCase))
                    {
                        var noWww = normalizedPath.Substring("wwwroot".Length).TrimStart(Path.DirectorySeparatorChar);
                        var altPath = Path.GetFullPath(Path.Combine(_env.ContentRootPath, "wwwroot", noWww));
                        if (System.IO.File.Exists(altPath))
                            fullPath = altPath;
                        else
                            return NotFound("Attachment file not found on disk.");
                    }
                    else
                    {
                        return NotFound("Attachment file not found on disk.");
                    }
                }

                // 4. Determine extension (strip .enc suffix)
                string cleanPath = rawPath.EndsWith(".enc", StringComparison.OrdinalIgnoreCase)
                    ? rawPath[..^4]
                    : rawPath;

                string detectedExt = Path.GetExtension(cleanPath).ToLowerInvariant();

                // 5. MIME type from extension
                string contentType = detectedExt switch
                {
                    ".pdf"  => "application/pdf",
                    ".doc"  => "application/msword",
                    ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    ".xls"  => "application/vnd.ms-excel",
                    ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    ".png"  => "image/png",
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".gif"  => "image/gif",
                    ".webp" => "image/webp",
                    ".bmp"  => "image/bmp",
                    ".mp4"  => "video/mp4",
                    ".txt"  => "text/plain",
                    _       => "application/octet-stream"
                };

                // 6. Clean file name for Content-Disposition
                string cleanFileName = Path.GetFileName(cleanPath);
                if (!Path.HasExtension(cleanFileName) && !string.IsNullOrWhiteSpace(detectedExt))
                    cleanFileName += detectedExt;

                // 7. Decrypt into memory
                var memoryStream = new MemoryStream();
                await _cryptoService.DecryptLargeFileToStreamAsync(fullPath, memoryStream, cancellationToken);
                memoryStream.Position = 0;

                // 8. Magic byte sniffing (for extensionless / GUID uploads)
                if (string.IsNullOrWhiteSpace(detectedExt))
                {
                    byte[] header = new byte[8];
                    int read = await memoryStream.ReadAsync(header, 0, header.Length, cancellationToken);
                    memoryStream.Position = 0;

                    string magicExt = "";
                    if (read >= 4 && header[0] == 0x25 && header[1] == 0x50 && header[2] == 0x44 && header[3] == 0x46)
                    { contentType = "application/pdf"; magicExt = ".pdf"; }
                    else if (read >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
                    { contentType = "image/jpeg"; magicExt = ".jpg"; }
                    else if (read >= 4 && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47)
                    { contentType = "image/png"; magicExt = ".png"; }
                    else if (read >= 4 && header[0] == 0x50 && header[1] == 0x4B && header[2] == 0x03 && header[3] == 0x04)
                    { contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document"; magicExt = ".docx"; }

                    if (!Path.HasExtension(cleanFileName) && !string.IsNullOrWhiteSpace(magicExt))
                        cleanFileName += magicExt;
                }

                // 9. Stream inline
                Response.Headers["Content-Disposition"] = $"inline; filename=\"{cleanFileName}\"";
                return File(memoryStream, contentType, enableRangeProcessing: false);
            }
            catch (OperationCanceledException)
            {
                return BadRequest("Request cancelled.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error streaming attachment.", error = ex.Message });
            }
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using LMS_SoulCode.Features.Security.Services;
using LMS_SoulCode.Features.Common;
using Microsoft.AspNetCore.Authorization;
using StatusCodes = LMS_SoulCode.Features.Common.StatusCodes;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace LMS_SoulCode.Features.Security.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CryptoController : BaseApiController
    {
        private readonly CryptographyService _crypto;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;

        public CryptoController(CryptographyService crypto, IConfiguration config, ILogger<CryptoController> logger, IWebHostEnvironment env) : base(logger)
        {
            _crypto = crypto;
            _config = config;
            _env = env;
        }

        [HttpPost("upload-file")]
        [BackOfficePermission(ModuleCodes.FILE_MANAGEMENT, PermissionCodes.FILE_UPLOAD)]
        public async Task<IActionResult> UploadFile(IFormFile file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
            {
                var error = ApiResponse<List<string>>.Fail("No file selected.", StatusCodes.BadRequest);
                return StatusCode(error.Code, error);
            }

            try
            {
                string ext = Path.GetExtension(file.FileName).ToLower();
                string folderPath = GetPathByExt(ext);

                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                using var ms = new MemoryStream();
                await file.CopyToAsync(ms, cancellationToken);
                
                string savePath = Path.Combine(folderPath, file.FileName + ".enc");

                if (IsImage(ext))
                {
                    using var processedMs = new MemoryStream(await _crypto.ProcessImageAsync(ms, 1200, cancellationToken));
                    await _crypto.EncryptLargeFileAsync(processedMs, savePath, cancellationToken);
                }
                else
                {
                    ms.Position = 0;
                    await _crypto.EncryptLargeFileAsync(ms, savePath, cancellationToken);
                }

                var response = ApiResponse<List<object>>.Success(new List<object> { new { path = savePath.Replace("\\", "/") } }, Messages.Uploaded);
                return StatusCode(response.Code, response);
            }
            catch (Exception ex)
            {
                var error = ApiResponse<List<string>>.Fail(ex.Message, StatusCodes.ServerError);
                return StatusCode(error.Code, error);
            }
        }

        [HttpPost("upload-secure-with-thumb")]
        [BackOfficePermission(ModuleCodes.FILE_MANAGEMENT, PermissionCodes.FILE_UPLOAD)]
        public async Task<IActionResult> UploadSecureWithThumb(IFormFile file, CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
            {
                var error = ApiResponse<List<string>>.Fail("No file selected.", StatusCodes.BadRequest);
                return StatusCode(error.Code, error);
            }

            try
            {
                string ext = Path.GetExtension(file.FileName).ToLower();
                if (!IsImage(ext)) 
                {
                    var error = ApiResponse<List<string>>.Fail("Only image files are supported.", StatusCodes.BadRequest);
                    return StatusCode(error.Code, error);
                }

                string folderPath = _config["AppSettings:OrgLogosPath"] ?? "wwwroot/uploads/OrgLogos";
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                using var ms = new MemoryStream();
                await file.CopyToAsync(ms, cancellationToken);

                var thumbSize = int.Parse(_config["AppSettings:OrgLogoThumbSize"] ?? "150");
                var (mainBytes, thumbBytes) = await _crypto.ProcessImageWithThumbAsync(ms, thumbSize, cancellationToken);

                string fileGuid = Guid.NewGuid().ToString();
                string mainFileName = $"{fileGuid}_main{ext}.enc";
                string thumbFileName = $"{fileGuid}_thumb{ext}.enc";

                string mainPath = Path.Combine(folderPath, mainFileName);
                string thumbPath = Path.Combine(folderPath, thumbFileName);

                await _crypto.EncryptLargeFileAsync(new MemoryStream(mainBytes), mainPath, cancellationToken);
                await _crypto.EncryptLargeFileAsync(new MemoryStream(thumbBytes), thumbPath, cancellationToken);

                var response = ApiResponse<List<object>>.Success(new List<object> { new 
                { 
                    mainPath = mainPath.Replace("\\", "/"), 
                    thumbPath = thumbPath.Replace("\\", "/") 
                } }, Messages.Uploaded);

                return StatusCode(response.Code, response);
            }
            catch (Exception ex)
            {
                var error = ApiResponse<List<string>>.Fail(ex.Message, StatusCodes.ServerError);
                return StatusCode(error.Code, error);
            }
        }

        [HttpGet("get")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMedia(
      [FromQuery] string path,
      CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                    return BadRequest("Path is required.");

                // =========================
                // PATH NORMALIZATION
                // =========================

                var normalizedPath = path
                    .Replace("/", Path.DirectorySeparatorChar.ToString())
                    .TrimStart(Path.DirectorySeparatorChar);

                var rootPath = _env.ContentRootPath;

                var fullPath = Path.GetFullPath(
                    Path.Combine(rootPath, normalizedPath));

                // Prevent path traversal attacks
                if (!fullPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
                    return BadRequest("Invalid path.");

                // =========================
                // FILE EXISTENCE CHECK
                // =========================

                if (!System.IO.File.Exists(fullPath))
                {
                    // Try current directory
                    var altPath = Path.GetFullPath(
                        Path.Combine(Directory.GetCurrentDirectory(), normalizedPath));

                    if (System.IO.File.Exists(altPath))
                    {
                        fullPath = altPath;
                    }
                    else if (normalizedPath.StartsWith("wwwroot", StringComparison.OrdinalIgnoreCase))
                    {
                        var noWwwRoot = normalizedPath.Substring("wwwroot".Length)
                            .TrimStart(Path.DirectorySeparatorChar);

                        var webRootPath = Path.Combine(
                            _env.ContentRootPath,
                            "wwwroot",
                            noWwwRoot);

                        webRootPath = Path.GetFullPath(webRootPath);

                        if (System.IO.File.Exists(webRootPath))
                        {
                            fullPath = webRootPath;
                        }
                        else
                        {
                            return NotFound($"File not found: {normalizedPath}");
                        }
                    }
                    else
                    {
                        return NotFound($"File not found: {normalizedPath}");
                    }
                }

                // =========================
                // EXTENSION DETECTION
                // =========================

                string cleanPath = path;

                if (cleanPath.EndsWith(".enc", StringComparison.OrdinalIgnoreCase))
                {
                    cleanPath = cleanPath[..^4];
                }

                string detectedExt = Path.GetExtension(cleanPath).ToLowerInvariant();

                // =========================
                // MIME TYPE
                // =========================

                string contentType = detectedExt switch
                {
                    ".pdf" => "application/pdf",
                    ".doc" => "application/msword",
                    ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    ".xls" => "application/vnd.ms-excel",
                    ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",

                    ".png" => "image/png",
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".gif" => "image/gif",
                    ".webp" => "image/webp",
                    ".bmp" => "image/bmp",

                    ".txt" => "text/plain",
                    ".css" => "text/css",
                    ".csv" => "text/csv",
                    ".html" or ".htm" => "text/html",
                    ".json" => "application/json",

                    ".mp3" => "audio/mpeg",
                    ".wav" => "audio/wav",

                    ".mp4" => "video/mp4",
                    ".webm" => "video/webm",
                    ".mkv" => "video/x-matroska",

                    _ => "application/octet-stream"
                };

                // =========================
                // CLEAN FILE NAME
                // =========================

                string cleanFileName = Path.GetFileName(cleanPath);

                if (!Path.HasExtension(cleanFileName) &&
                    !string.IsNullOrWhiteSpace(detectedExt))
                {
                    cleanFileName += detectedExt;
                }

                // =========================
                // VIDEO HANDLING
                // =========================
                // IMPORTANT:
                // Range processing ONLY works
                // with seekable streams.
                // If your decrypt stream is not seekable,
                // disable range processing.

                if (detectedExt == ".mp4" ||
                    detectedExt == ".webm" ||
                    detectedExt == ".mkv")
                {
                    var videoStream = _crypto.GetDecryptStream(fullPath);

                    // DEBUG
                    // Console.WriteLine(videoStream.CanSeek);

                    Response.Headers["Content-Disposition"] =
                        $"inline; filename=\"{cleanFileName}\"";

                    // If decrypt stream is NOT seekable:
                    if (!videoStream.CanSeek)
                    {
                        return File(
                            videoStream,
                            contentType,
                            enableRangeProcessing: false);
                    }

                    // If stream IS seekable:
                    return File(
                        videoStream,
                        contentType,
                        enableRangeProcessing: true);
                }

                // =========================
                // DOCUMENT / IMAGE HANDLING
                // =========================

                var memoryStream = new MemoryStream();

                await _crypto.DecryptLargeFileToStreamAsync(
                    fullPath,
                    memoryStream,
                    cancellationToken);

                memoryStream.Position = 0;

                // =========================
                // MAGIC BYTE DETECTION
                // =========================

                string magicExt = "";
                if (string.IsNullOrWhiteSpace(detectedExt))
                {
                    byte[] header = new byte[8];

                    int bytesRead = await memoryStream.ReadAsync(
                        header,
                        0,
                        header.Length,
                        cancellationToken);

                    memoryStream.Position = 0;

                    if (bytesRead >= 4 &&
                        header[0] == 0x25 &&
                        header[1] == 0x50 &&
                        header[2] == 0x44 &&
                        header[3] == 0x46)
                    {
                        contentType = "application/pdf";
                        magicExt = ".pdf";
                    }
                    else if (bytesRead >= 3 &&
                             header[0] == 0xFF &&
                             header[1] == 0xD8 &&
                             header[2] == 0xFF)
                    {
                        contentType = "image/jpeg";
                        magicExt = ".jpg";
                    }
                    else if (bytesRead >= 4 &&
                             header[0] == 0x89 &&
                             header[1] == 0x50 &&
                             header[2] == 0x4E &&
                             header[3] == 0x47)
                    {
                        contentType = "image/png";
                        magicExt = ".png";
                    }
                    else if (bytesRead >= 4 &&
                             header[0] == 0x50 &&
                             header[1] == 0x4B &&
                             header[2] == 0x03 &&
                             header[3] == 0x04)
                    {
                        contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                        magicExt = ".docx";
                    }
                }

                // =========================
                // RESPONSE HEADERS
                // =========================

                string resolvedExt = !string.IsNullOrWhiteSpace(detectedExt) ? detectedExt : magicExt;
                if (!Path.HasExtension(cleanFileName) && !string.IsNullOrWhiteSpace(resolvedExt))
                {
                    cleanFileName += resolvedExt;
                }

                Response.Headers["Content-Disposition"] =
                    $"inline; filename=\"{cleanFileName}\"";

                // =========================
                // RETURN STREAM DIRECTLY
                // =========================
                // DO NOT USE:
                // ms.ToArray()

                return File(
                    memoryStream,
                    contentType,
                    enableRangeProcessing: false);
            }
            catch (OperationCanceledException)
            {
                return BadRequest("Request cancelled.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error serving media.",
                    error = ex.Message
                });
            }
        }

        private string GetPathByExt(string ext) => ext switch
        {
            ".mp4" or ".avi" or ".mkv" => _config["AppSettings:VideosPath"] ?? "wwwroot/uploads/videos",
            ".pdf" or ".doc" or ".docx" => _config["AppSettings:DocsPath"] ?? "wwwroot/uploads/documents",
            _ => _config["AppSettings:OrgLogosPath"] ?? "wwwroot/uploads/OrgLogos"
        };

        private bool IsImage(string ext) => ext switch
        {
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp" => true,
            _ => false
        };

        private string GetContentType(string ext) => ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".pdf" => "application/pdf",
            ".mp4" => "video/mp4",
            _ => "application/octet-stream"
        };
    }
}
